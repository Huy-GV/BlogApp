import { StackProps, Stack, aws_elasticloadbalancingv2, aws_certificatemanager, Duration } from "aws-cdk-lib";
import { ISecurityGroup, IVpc, PublicSubnet, SubnetType } from "aws-cdk-lib/aws-ec2";
import { IRepository } from "aws-cdk-lib/aws-ecr";
import { FargateTaskDefinition, FargateService, Cluster, AwsLogDriver, ContainerImage, AppProtocol, Protocol } from "aws-cdk-lib/aws-ecs";
import { Effect, ManagedPolicy, PolicyStatement, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { Construct } from "constructs";
import { AppConfiguration } from "../config/appConfiguration";
import { IBucket } from "aws-cdk-lib/aws-s3";
import { ARecord, HostedZone, PublicHostedZone, RecordTarget } from "aws-cdk-lib/aws-route53";
import { LoadBalancerTarget } from "aws-cdk-lib/aws-route53-targets";

interface ContainerServiceStackProps extends StackProps {
	vpc: IVpc,
	dataBucket: IBucket,
	loadBalancerTierSecurityGroup: ISecurityGroup
	webTierSecurityGroup: ISecurityGroup
	databaseUserId: string
	databasePassword: string
	databaseName: string
	databaseEndpoint: string
	databasePort: string
	ecrRepository: IRepository,
	appConfiguration: AppConfiguration
}

export class ContainerServiceStack extends Stack {
	constructor(scope: Construct, id: string, props: ContainerServiceStackProps) {
		super(scope, id);

		const taskExecutionRole = this.createEcsExecutionRole(props.dataBucket);
		const cluster = new Cluster(this, 'RzCdkCluster', {
			clusterName: "razorblog-cdk-cluster",
			vpc: props.vpc
		})

		const taskDefinition = this.createTaskDefinition(
			taskExecutionRole,
			props.databaseEndpoint,
			props.databasePort,
			props.databaseName,
			props.databaseUserId,
			props.databasePassword,
			props.appConfiguration,
			props.ecrRepository
		);

		const fargateService = this.createFargateService(
			taskDefinition,
			props.webTierSecurityGroup,
			props.vpc,
			cluster
		);

		const appLoadBalancer = new aws_elasticloadbalancingv2.ApplicationLoadBalancer(
			this,
			'RzCdkAlb',
			{
				vpc: props.vpc,
				securityGroup: props.loadBalancerTierSecurityGroup,
				vpcSubnets: props.vpc.selectSubnets({ subnetType: SubnetType.PUBLIC }),
				internetFacing: true
			}
		);

		const listener = appLoadBalancer.addListener('HttpsListener', {
			port: 443,
			open: true,
			certificates: [aws_certificatemanager.Certificate.fromCertificateArn(
				this,
				'RzCdkHttpsCertificateArn',
				props.appConfiguration.Aws__CertificateArn)]
		});

		const targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(
			this,
			'RzAlbTargetGroup',
			{
				vpc: props.vpc,
				port: 80,
				targets: [fargateService],
				targetType: aws_elasticloadbalancingv2.TargetType.IP,
				healthCheck: {
					path: '/',
					interval: Duration.seconds(30),
					timeout: Duration.seconds(5)
				}
			}
		);

		listener.addTargetGroups('DefaultTargetGroup', {
			targetGroups: [targetGroup]
		});
	}

	private createEcsExecutionRole(dataBucket: IBucket): Role {
		const razorBlogTaskExecutionRole = new Role(this, 'RazorBlogFargateExeRole', {
		  	assumedBy: new ServicePrincipal('ecs-tasks.amazonaws.com'),
		});

		// required to pull images from the ECR repository
		razorBlogTaskExecutionRole.addManagedPolicy(
		  	ManagedPolicy.fromAwsManagedPolicyName('service-role/AmazonECSTaskExecutionRolePolicy')
		);

		razorBlogTaskExecutionRole.addToPolicy(
			new PolicyStatement({
				actions: ['s3:*'],
				resources: [dataBucket.bucketArn, `${dataBucket.bucketArn}/*`],
				effect: Effect.ALLOW
			})
		);

		return razorBlogTaskExecutionRole;
	}

	private createFargateService(
		taskDefinition: FargateTaskDefinition,
		webServerTierSecurityGroup: ISecurityGroup,
		vpc: IVpc,
		cluster: Cluster
	) : FargateService {
		return new FargateService(
			this,
			'RzCdkFargateService',
			{
				taskDefinition,
				cluster: cluster,
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
		databaseEndpoint: string,
		databasePort: string,
		databaseName: string,
		databaseUserId: string,
		databasePassword: string,
		appConfiguration: AppConfiguration,
		ecrRepository: IRepository
	): FargateTaskDefinition {
		const taskDefinition = new FargateTaskDefinition(
			this,
			'RzCdkFargateTaskDefinition',
			{
				cpu: 256,
				memoryLimitMiB: 512,
				taskRole: taskExecutionRole,
				executionRole: taskExecutionRole,
			},
		);

		const connectionString = `Server=${databaseEndpoint},${databasePort};Database=${databaseName};User ID=${databaseUserId};Password=${databasePassword};MultipleActiveResultSets=false;TrustServerCertificate=true;`

		const {
			Aws__CertificateArn,
			Aws__HostedZoneName,
			Aws__HostedZoneId,
			...relevantEnvVariables
		} = appConfiguration

		const containerEnvVariables = {
			...relevantEnvVariables,
		  	ConnectionStrings__DefaultConnection: connectionString
		};

		const logging = new AwsLogDriver({ streamPrefix: "razor-blog" });
		taskDefinition.addContainer(
			'RzCdkContainer',
			{
				containerName: 'razorblog-cdk-container',
				image: ContainerImage.fromEcrRepository(ecrRepository, "latest"),
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
					}
				]
			}
		);

		return taskDefinition;
	}
}
