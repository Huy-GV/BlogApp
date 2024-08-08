import { StackProps, Stack, aws_elasticloadbalancingv2, aws_certificatemanager, Duration, aws_ecs } from "aws-cdk-lib";
import { ISecurityGroup, IVpc, SubnetType } from "aws-cdk-lib/aws-ec2";
import { IRepository } from "aws-cdk-lib/aws-ecr";
import { FargateTaskDefinition, FargateService, Cluster, AwsLogDriver, ContainerImage, AppProtocol, Protocol } from "aws-cdk-lib/aws-ecs";
import { Effect, ManagedPolicy, PolicyStatement, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { Construct } from "constructs";
import { AppConfiguration } from "../config/appConfiguration";
import { IBucket } from "aws-cdk-lib/aws-s3";
import { ARecord, PublicHostedZone, RecordTarget } from "aws-cdk-lib/aws-route53";
import { LoadBalancerTarget } from "aws-cdk-lib/aws-route53-targets";
import { StringParameter } from "aws-cdk-lib/aws-ssm";

interface ContainerServiceStackProps extends StackProps {
	vpc: IVpc,
	dataBucket: IBucket,
	loadBalancerTierSecurityGroup: ISecurityGroup,
	webTierSecurityGroup: ISecurityGroup,
	databaseEndpoint: string,
	databaseName: string,
	ecrRepository: IRepository,
	appConfiguration: AppConfiguration
}

export class ContainerStack extends Stack {
	readonly fargateService: FargateService

	constructor(scope: Construct, id: string, props: ContainerServiceStackProps) {
		super(scope, id, props);

		const taskExecutionRole = this.createEcsExecutionRole(props.dataBucket);
		const cluster = new Cluster(this, 'SfoCdkCluster', {
			clusterName: "sfo-cdk-cluster",
			vpc: props.vpc
		})

		const taskDefinition = this.createTaskDefinition(
			taskExecutionRole,
			props.databaseName,
			props.databaseEndpoint,
			props.appConfiguration,
			props.ecrRepository
		);

		this.fargateService = this.createFargateService(
			taskDefinition,
			props.webTierSecurityGroup,
			props.vpc,
			cluster
		);

		const appLoadBalancer = new aws_elasticloadbalancingv2.ApplicationLoadBalancer(
			this,
			'SfoCdkAlb',
			{
				loadBalancerName: 'sfo-cdk-alb',
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
				'SfoCdkHttpsCertificateArn',
				props.appConfiguration.Aws__CertificateArn)]
		});

		const targetGroup = new aws_elasticloadbalancingv2.ApplicationTargetGroup(
			this,
			'SfAlbTargetGroup',
			{
				vpc: props.vpc,
				port: 80,
				targets: [this.fargateService],
				targetType: aws_elasticloadbalancingv2.TargetType.IP,
				healthCheck: {
					path: '/health',
					interval: Duration.seconds(40),
					timeout: Duration.seconds(15)
				}
			}
		);

		listener.addTargetGroups('DefaultTargetGroup', {
			targetGroups: [targetGroup]
		});

		const hostedZone = PublicHostedZone.fromLookup(
			this,
			'SfoCdkHostedZone',
			{
				domainName: props.appConfiguration.Aws__HostedZoneName
			}
		);

		new ARecord(this, 'SfoCdkAliasRecord', {
			zone: hostedZone,
			target: RecordTarget.fromAlias(new LoadBalancerTarget(appLoadBalancer)),
			recordName: 'sfo'
		});
	}

	private createEcsExecutionRole(dataBucket: IBucket): Role {
		const sfoTaskExecutionRole = new Role(this, 'SfFargateExeRole', {
		  	assumedBy: new ServicePrincipal('ecs-tasks.amazonaws.com'),
		});

		// required to pull images from the ECR repository
		sfoTaskExecutionRole.addManagedPolicy(
		  	ManagedPolicy.fromAwsManagedPolicyName('service-role/AmazonECSTaskExecutionRolePolicy')
		);

		sfoTaskExecutionRole.addToPolicy(
			new PolicyStatement({
				actions: ['s3:*'],
				resources: [dataBucket.bucketArn, `${dataBucket.bucketArn}/*`],
				effect: Effect.ALLOW
			})
		);

		return sfoTaskExecutionRole;
	}

	private createFargateService(
		taskDefinition: FargateTaskDefinition,
		webServerTierSecurityGroup: ISecurityGroup,
		vpc: IVpc,
		cluster: Cluster
	) : FargateService {
		return new FargateService(
			this,
			'SfoCdkFargateService',
			{
				taskDefinition,
				cluster: cluster,
				desiredCount: 0,
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
		databaseName: string,
		databaseEndpoint: string,
		appConfiguration: AppConfiguration,
		ecrRepository: IRepository
	): FargateTaskDefinition {
		const taskDefinition = new FargateTaskDefinition(
			this,
			'SfoCdkFargateTaskDefinition',
			{
				cpu: 256,
				memoryLimitMiB: 512,
				taskRole: taskExecutionRole,
				executionRole: taskExecutionRole,
			},
		);

		const containerEnvVariables = {
			ConnectionStrings__DatabaseName: databaseName,
			ConnectionStrings__Endpoint: databaseEndpoint,
			ConnectionStrings__UserId: appConfiguration.ConnectionStrings__UserId,
			ASPNETCORE_URLS: appConfiguration.ASPNETCORE_URLS,
			Aws__DataBucket: appConfiguration.Aws__DataBucket,
		}

		const databasePasswordParameter = StringParameter.fromSecureStringParameterAttributes(this, 'SfoCdkDbPassword', {
			parameterName: '/sfo/prod/db/password',
		});

		const seedUserPasswordParameter = StringParameter.fromSecureStringParameterAttributes(this, 'SfoCdkSeedUserPassword', {
			parameterName: '/sfo/prod/seeduser/password',
		});

		const dbPassword = aws_ecs.Secret.fromSsmParameter(databasePasswordParameter);
		const seedUserPassword = aws_ecs.Secret.fromSsmParameter(seedUserPasswordParameter);

		const logging = new AwsLogDriver({ streamPrefix: "simple-forum" });
		taskDefinition.addContainer(
			'SfoCdkContainer',
			{
				containerName: 'sf-cdk-container',
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
				],
				secrets: {
					SeedUser__Password: seedUserPassword,
					ConnectionStrings__Password: dbPassword,
				}
			}
		);

		return taskDefinition;
	}
}
