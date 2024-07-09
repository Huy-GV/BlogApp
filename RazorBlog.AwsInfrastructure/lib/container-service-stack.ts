import { StackProps, Stack, aws_elasticloadbalancingv2, aws_certificatemanager, Duration } from "aws-cdk-lib";
import { ISecurityGroup, IVpc } from "aws-cdk-lib/aws-ec2";
import { IRepository } from "aws-cdk-lib/aws-ecr";
import { FargateTaskDefinition, FargateService, Cluster, AwsLogDriver, ContainerImage, AppProtocol, Protocol } from "aws-cdk-lib/aws-ecs";
import { Effect, ManagedPolicy, PolicyStatement, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { Construct } from "constructs";
import { AppConfiguration } from "../config/appConnfiguration";
import { IBucket } from "aws-cdk-lib/aws-s3";
import { ARecord, HostedZone, RecordTarget } from "aws-cdk-lib/aws-route53";
import { LoadBalancerTarget } from "aws-cdk-lib/aws-route53-targets";

interface ContainerServiceStackProps extends StackProps {
	vpc: IVpc,
	dataBucket: IBucket,
	webTierSecurityGroup: ISecurityGroup
	databaseUserId: string
	databasePassword: string
	databaseName: string
	databaseEndpoint: string
	databasePort: string
	ecrRepository: IRepository,
	envProps: AppConfiguration
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
			props.envProps,
			props.ecrRepository
		);

		const fargateService = this.createFargateService(
			taskDefinition,
			props.webTierSecurityGroup,
			props.vpc,
			cluster
		);

		const alb = new aws_elasticloadbalancingv2.ApplicationLoadBalancer(
			this,
			'RzCdkAlb',
			{
				vpc: props.vpc,
				internetFacing: true
			}
		);

		// const listener = alb.addListener('HttpsListener', {
		// 	port: 443,
		// 	open: true,
		// 	certificates: [aws_certificatemanager.Certificate.fromCertificateArn(
		// 		this,
		// 		'RzCdkHttpsCertificateArn',
		// 		props.envProps.Aws__HttpsCertificateArn)]
		// });

		// const targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(this, 'RzAlbTargetGroup', {
		// 	vpc: props.vpc,
		// 	port: 80,
		// 	targets: [ fargateService ],
		// 	targetType: aws_elasticloadbalancingv2.TargetType.IP,
		// 	healthCheck: {
		// 		path: '/',
		// 		interval: Duration.seconds(30),
		// 		timeout: Duration.seconds(5)
		// 	}
		// });

		// listener.addTargetGroups('DefaultTargetGroup', {
		// 	targetGroups: [targetGroup]
		// });

		// const hostedZone = HostedZone.fromLookup(stack, 'MyHostedZone', {
		// 	domainName: 'example.com' // Replace with your domain
		// });

		// new ARecord(stack, 'AliasRecord', {
		// 	zone: hostedZone,
		// 	target: RecordTarget.fromAlias(new LoadBalancerTarget(alb)),
		// 	recordName: 'razor-blog'
		// });
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
		envVariables: AppConfiguration,
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

		const containerEnvVariables = {
		  	...envVariables,
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
