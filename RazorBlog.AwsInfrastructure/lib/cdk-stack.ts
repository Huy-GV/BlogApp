import { Construct } from 'constructs';
import { Stack, StackProps, CfnOutput, SecretValue, RemovalPolicy } from 'aws-cdk-lib';
import { DotenvParseOutput, config } from 'dotenv'
import { join } from 'path';
import { exit } from 'process';
import { ManagedPolicy, Role, ServicePrincipal } from 'aws-cdk-lib/aws-iam';
import { InstanceClass, InstanceSize, InstanceType, Peer, Port, SecurityGroup, SubnetType, Vpc } from 'aws-cdk-lib/aws-ec2';
import { FargateService, Cluster, FargateTaskDefinition, Protocol, ContainerImage, AwsLogDriver, AppProtocol } from 'aws-cdk-lib/aws-ecs';
import { DatabaseInstance, DatabaseInstanceEngine, SqlServerEngineVersion, SubnetGroup } from 'aws-cdk-lib/aws-rds';
import { IRepository, Repository } from 'aws-cdk-lib/aws-ecr';
import { BlockPublicAccess, Bucket } from 'aws-cdk-lib/aws-s3';

interface EnvVariableProps {
  SeedUserPassword: string;
  DatabaseUserId: string;
  SqlServerPassword: string;
  AspNetCoreKestrelCertPassword: string;
  AspNetCoreKestrelCertPath: string;
  AspNetCorUrls: string;
  AspNetCoreHttpsPort: number;

  AwsSecretKey: string;
  AwsAccessKey: string;
  AwsS3BucketName: string;

  RawConfig: DotenvParseOutput;
}

const VPC_NAME = 'razor-blog-vpc';
const DATABASE_NAME = 'RazorBlogDatabase';
const DATABASE_SUBNET_GROUP_NAME = 'razor-blog-db-subnet-group'
const CLUSTER_NAME = 'RazorBlogCluster';
const TASK_DEFINITION_NAME = 'RazorBlogTaskDefinition';
const FARGATE_SERVICE_NAME = 'RazorBlogService';
const FARGATE_EXECUTION_ROLE = 'RazorBlogExecutionRole';
const CONTAINER_NAME = 'razor-blog-container';
const DOCKER_IMAGE_TAG = 'latest'
const ECR_NAME = 'razor-blog-repository';
const S3_BUCKET_NAME = 'razor-blog-bucket';

export class AppCdkStack extends Stack {
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);
    const envVariableProps = this.parseEnvVariableProps();
    if (!envVariableProps) {
      exit(-1);
    }

    this.createS3Bucket(S3_BUCKET_NAME);
    const vpc = this.createVpc();
    const databaseTierSecurityGroup = this.createDatabaseTierSecurityGroup(vpc);
    const webServerTierSecurityGroup = this.createWebServerTierSecurityGroup(vpc);

    webServerTierSecurityGroup.addEgressRule(
      databaseTierSecurityGroup,
      Port.tcp(1433),
      'Allow MSSQL traffic to DatabaseTier'
    );

    databaseTierSecurityGroup.addIngressRule(
      webServerTierSecurityGroup,
      Port.tcp(1433),
      'Allow HTTP traffic from any IPv4 addresses'
    );

    const ecrRepository = Repository.fromRepositoryName(this, `existing-${ECR_NAME}`, ECR_NAME)

    const databaseInstance = this.createRdsDatabase(
      DATABASE_NAME,
      vpc,
      databaseTierSecurityGroup,
      envVariableProps.DatabaseUserId,
      envVariableProps.SqlServerPassword
    );

    const razorBlogTaskExecutionRole = this.createEcsExecutionRole();
    const taskDefinition = this.createTaskDefinition(
      razorBlogTaskExecutionRole,
      databaseInstance,
      DATABASE_NAME,
      envVariableProps.DatabaseUserId,
      envVariableProps.SqlServerPassword,
      envVariableProps,
      ecrRepository,
      DOCKER_IMAGE_TAG
    );

    this.createFargateService(
      taskDefinition,
      webServerTierSecurityGroup,
      vpc
    );

    new CfnOutput(this, 'VpcId', {
      value: vpc.vpcId,
    });
  }

  private createS3Bucket(
    bucketName: string,
  ) {
    return new Bucket(
      this,
      bucketName,
      {
        blockPublicAccess: BlockPublicAccess.BLOCK_ALL,
        versioned: false,
        removalPolicy: RemovalPolicy.DESTROY,
      }
    );
  }

  private createFargateService(
    taskDefinition: FargateTaskDefinition,
    webServerTierSecurityGroup: SecurityGroup,
    vpc: Vpc
  ) : FargateService {
    return new FargateService(
      this,
      FARGATE_SERVICE_NAME,
      {
        taskDefinition,
        cluster: new Cluster(this, CLUSTER_NAME, { vpc }),
        desiredCount: 1,
        assignPublicIp: true,
        securityGroups: [
          webServerTierSecurityGroup
        ],
        vpcSubnets: {
          subnets: vpc.publicSubnets
        },
      }
    );
  }

  private createTaskDefinition(
    taskExecutionRole: Role,
    databaseInstance: DatabaseInstance,
    databaseName: string,
    databaseUserId: string,
    databasePassword: string,
    envVariableProps: EnvVariableProps,
    ecrRepository: IRepository,
    dockerImageTag: string
  ): FargateTaskDefinition {
    const taskDefinition = new FargateTaskDefinition(
      this,
      TASK_DEFINITION_NAME,
      {
        cpu: 256,
        memoryLimitMiB: 512,
        taskRole: taskExecutionRole,
        executionRole: taskExecutionRole,
      },
    );

    // ensure the database endpoint is correctly loaded to the connection string
    taskDefinition.node.addDependency(databaseInstance);

    const connectionString = `Server=${databaseInstance.dbInstanceEndpointAddress},${databaseInstance.dbInstanceEndpointPort};Database=${databaseName};User ID=${databaseUserId};Password=${databasePassword};MultipleActiveResultSets=false;TrustServerCertificate=true;`

    const containerEnvVariables = {
      ...envVariableProps.RawConfig,
      ConnectionStrings__DefaultConnection: connectionString
    };

    const logging = new AwsLogDriver({ streamPrefix: "razor-blog" });
    taskDefinition.addContainer(
      CONTAINER_NAME,
      {
        image: ContainerImage.fromEcrRepository(ecrRepository, dockerImageTag),
        memoryLimitMiB: 256,
        cpu: 128,
        environment: containerEnvVariables,
        logging: logging,
        portMappings: [
          {
            containerPort: 80,
            hostPort: 80,
            protocol: Protocol.TCP,
            appProtocol: AppProtocol.http,
            name: 'http-mappings'
          },
          {
            containerPort: 443,
            hostPort: 443,
            protocol: Protocol.TCP,
            appProtocol: AppProtocol.http,
            name: 'https-mappings'
          }
        ]
      }
    );

    return taskDefinition;
  }

  private createRdsDatabase(
    name: string,
    vpc: Vpc,
    securityGroup: SecurityGroup,
    userId: string,
    password: string
  ) {
    return new DatabaseInstance(
      this,
      name,
      {
        engine: DatabaseInstanceEngine.sqlServerEx({ version: SqlServerEngineVersion.VER_16 }),
        vpc,
        instanceType: InstanceType.of(
          InstanceClass.T3,
          InstanceSize.MICRO),
        credentials: {
          username: userId,
          password: SecretValue.unsafePlainText(password),
        },
        vpcSubnets: vpc.selectSubnets({ subnetType: SubnetType.PRIVATE_ISOLATED }),
        securityGroups: [
          securityGroup
        ],
        subnetGroup: new SubnetGroup(this, DATABASE_SUBNET_GROUP_NAME, {
          vpc: vpc,
          subnetGroupName: DATABASE_SUBNET_GROUP_NAME,
          vpcSubnets: vpc.selectSubnets({ subnetType: SubnetType.PRIVATE_ISOLATED }),
          description: 'private-subnet-group-for-db'
        })
      }
    );
  }

  private createEcsExecutionRole(): Role {
    const razorBlogTaskExecutionRole = new Role(this, FARGATE_EXECUTION_ROLE, {
      assumedBy: new ServicePrincipal('ecs-tasks.amazonaws.com'),
    });

    // required to pull images from the ECR repository
    razorBlogTaskExecutionRole.addManagedPolicy(
      ManagedPolicy.fromAwsManagedPolicyName('service-role/AmazonECSTaskExecutionRolePolicy')
    );

    return razorBlogTaskExecutionRole;
  }

  private parseEnvVariableProps(): EnvVariableProps | null {
    const envFilePath = join(__dirname, '.env');
    console.log(`Reading env variables from '${envFilePath}'`)
    const envConfigResult = config({
      path: envFilePath
    });

    if (envConfigResult.error) {
      console.error(envConfigResult);
      return null;
    }

    const parsedConfig = envConfigResult.parsed;
    if (parsedConfig === undefined || parsedConfig === null) {
      return null;
    }

    if (Object.values(parsedConfig).some(x => x === undefined || x === null)) {
      return null;
    }

    const envVariableProps: EnvVariableProps = {
      SeedUserPassword: parsedConfig.SeedUser__Password,
      DatabaseUserId: parsedConfig.Database__UserId,
      SqlServerPassword: parsedConfig.SqlServer__Password,
      AspNetCoreKestrelCertPassword: parsedConfig.ASPNETCORE_Kestrel__Certificates__Default__Password,
      AspNetCoreKestrelCertPath: parsedConfig.ASPNETCORE_Kestrel__Certificates__Default__Path,
      AwsSecretKey: parsedConfig.Aws__SecretKey,
      AwsAccessKey: parsedConfig.Aws__S3__BucketName,
      AwsS3BucketName: parsedConfig.Aws__AccessKey,
      AspNetCoreHttpsPort: Number.parseInt(parsedConfig.ASPNETCORE_HTTPS_PORT),
      AspNetCorUrls: parsedConfig.ASPNETCORE_URLS,

      RawConfig: parsedConfig
    }

    return envVariableProps;
  }

  private createVpc(): Vpc {
    return new Vpc(
      this,
      VPC_NAME,
      {
        maxAzs: 2,
        subnetConfiguration: [
          {
            cidrMask: 24,
            name: 'public-subnet-1',
            subnetType: SubnetType.PUBLIC,
          },
          {
            cidrMask: 24,
            name: 'public-subnet-2',
            subnetType: SubnetType.PUBLIC,
          },
          {
            cidrMask: 24,
            name: 'private-subnet-1',
            subnetType: SubnetType.PRIVATE_ISOLATED,
          },
          {
            cidrMask: 24,
            name: 'private-subnet-2',
            subnetType: SubnetType.PRIVATE_ISOLATED,
          },
        ],
      }
    );
  }

  private createWebServerTierSecurityGroup(
    vpc: Vpc
  ) {
    const webServerTierSG = new SecurityGroup(
      this,
      'WebServerTierSecurityGroup',
      {
        vpc,
        securityGroupName: 'WebServerTier',
      }
    );

    webServerTierSG.addIngressRule(
      Peer.anyIpv4(),
      Port.tcp(80),
      'Allow HTTP traffic from any IPv4 addresses'
    );

    webServerTierSG.addIngressRule(
      Peer.anyIpv6(),
      Port.tcp(80),
      'Allow HTTP traffic from any IPv6 addresses'
    );

    webServerTierSG.addIngressRule(
      Peer.anyIpv4(),
      Port.tcp(443),
      'Allow HTTPS traffic from any IPv4 addresses'
    );

    webServerTierSG.addIngressRule(
      Peer.anyIpv6(),
      Port.tcp(443),
      'Allow HTTPS traffic from any IPv6 addresses'
    );

    webServerTierSG.addEgressRule(
      Peer.anyIpv4(),
      Port.tcp(443),
      'Allow HTTPS traffic to any IPv4 addresses'
    );

    webServerTierSG.addEgressRule(
      Peer.anyIpv6(),
      Port.tcp(443),
      'Allow HTTPS traffic to any IPv6 addresses'
    );

    return webServerTierSG;
  }

  private createDatabaseTierSecurityGroup(
    vpc: Vpc
  ) {
    return new SecurityGroup(this, 'DatabaseTierSecurityGroup', {
      vpc,
      securityGroupName: 'DatabaseTier',
    });
  }
}
