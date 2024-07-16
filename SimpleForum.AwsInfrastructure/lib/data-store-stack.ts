import { RemovalPolicy, SecretValue, Stack, StackProps } from "aws-cdk-lib";
import { InstanceType, InstanceClass, InstanceSize, SubnetType, IVpc, ISecurityGroup } from "aws-cdk-lib/aws-ec2";
import { IRepository, Repository } from "aws-cdk-lib/aws-ecr";
import { DatabaseInstance, DatabaseInstanceEngine, IDatabaseInstance, SqlServerEngineVersion, SubnetGroup } from "aws-cdk-lib/aws-rds";
import { Bucket, BlockPublicAccess, IBucket } from "aws-cdk-lib/aws-s3";
import { Construct } from "constructs";

export interface DataStoreStackProps extends StackProps {
    vpc: IVpc,
    databaseTierSecurityGroup: ISecurityGroup,
    dataBucketName: string,
    databaseName: string,
    databaseUserId: string,
    databasePassword: string
}

export class DataStoreStack extends Stack {
    readonly databaseInstance: IDatabaseInstance
    readonly repository: IRepository
    readonly dataBucket: IBucket

    constructor(scope: Construct, id: string, props: DataStoreStackProps) {
        super(scope, id, props);

        this.dataBucket =new Bucket(
            this,
            'SfDataBucket',
            {
                bucketName: props.dataBucketName,
                blockPublicAccess: BlockPublicAccess.BLOCK_ALL,
                versioned: false,
                removalPolicy: RemovalPolicy.DESTROY,
                autoDeleteObjects: true
            }
        );

        this.databaseInstance = new DatabaseInstance(
            this,
            'SfCdkDb',
            {
                removalPolicy: RemovalPolicy.SNAPSHOT,
                instanceIdentifier: props.databaseName,
                engine: DatabaseInstanceEngine.sqlServerEx({ version: SqlServerEngineVersion.VER_16 }),
                vpc: props.vpc,
                instanceType: InstanceType.of(
                    InstanceClass.T3,
                    InstanceSize.MICRO),
                credentials: {
                    username: props.databaseUserId,
                    password: SecretValue.unsafePlainText(props.databasePassword),
                },
                vpcSubnets: props.vpc.selectSubnets({ subnetType: SubnetType.PRIVATE_ISOLATED }),
                securityGroups: [
                    props.databaseTierSecurityGroup
                ],
                subnetGroup: new SubnetGroup(this, 'SfCdkDbSubnetGroup', {
                    vpc: props.vpc,
                    subnetGroupName: 'razorblog-cdk-db-subnet-group',
                    vpcSubnets: props.vpc.selectSubnets({ subnetType: SubnetType.PRIVATE_ISOLATED }),
                    description: 'private-subnet-group-for-db'
                })
            }
        );

        this.repository = new Repository(this, 'SfCdkRepository', {
            repositoryName: "razorblog-cdk-repository",
            emptyOnDelete: true,
            removalPolicy: RemovalPolicy.DESTROY
        });
    }
}
