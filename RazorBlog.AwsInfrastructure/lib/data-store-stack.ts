import { RemovalPolicy, SecretValue, Stack, StackProps } from "aws-cdk-lib";
import { InstanceType, InstanceClass, InstanceSize, SubnetType, IVpc, ISecurityGroup } from "aws-cdk-lib/aws-ec2";
import { IRepository, Repository } from "aws-cdk-lib/aws-ecr";
import { DatabaseInstance, DatabaseInstanceEngine, IDatabaseInstance, SqlServerEngineVersion, SubnetGroup } from "aws-cdk-lib/aws-rds";
import { Bucket, BlockPublicAccess } from "aws-cdk-lib/aws-s3";
import { Construct } from "constructs";

export interface DataStoreStackProps extends StackProps {
    vpc: IVpc,
    databaseTierSecurityGroup: ISecurityGroup,
    userId: string,
    password: string
}

export class DataStoreStack extends Stack {
    readonly databaseInstance: IDatabaseInstance
    readonly repository: IRepository

    constructor(scope: Construct, id: string, props: DataStoreStackProps) {
        super(scope, id);

        this.createS3Bucket();
        this.databaseInstance = this.createRdsDatabase(
            props.vpc, 
            props.databaseTierSecurityGroup, 
            props.userId, 
            props.password);

        this.repository = this.createEcrRepository();
    }

    private createEcrRepository() {
        return new Repository(this, 'RazorBlogCdkRepository', {
            repositoryName: "razorblog-cdk-repository",
            emptyOnDelete: true,
            removalPolicy: RemovalPolicy.DESTROY
        })  
    }

    private createS3Bucket() {
        return new Bucket(
            this,
            'RazorBlogDataBucket',
            {
                bucketName: 'razorblog-cdk-data',
                blockPublicAccess: BlockPublicAccess.BLOCK_ALL,
                versioned: false,
                removalPolicy: RemovalPolicy.DESTROY,
            }
        );
    }
  
    private createRdsDatabase(
        vpc: IVpc,
        securityGroup: ISecurityGroup,
        userId: string,
        password: string
    ) {
        return new DatabaseInstance(
            this,
            'RazorBlogCdkDb',
            {
                databaseName: 'razorblog-cdk-db',
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
                subnetGroup: new SubnetGroup(this, 'RazorBlogCdkDbSubnetGroup', {
                    vpc: vpc,
                    subnetGroupName: 'razorblog-cdk-db-subnet-group',
                    vpcSubnets: vpc.selectSubnets({ subnetType: SubnetType.PRIVATE_ISOLATED }),
                    description: 'private-subnet-group-for-db'
                })
            }
        );
    }
}