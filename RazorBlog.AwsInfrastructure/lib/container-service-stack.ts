import { StackProps, Stack } from "aws-cdk-lib";
import { ISecurityGroup, IVpc } from "aws-cdk-lib/aws-ec2";
import { IRepository } from "aws-cdk-lib/aws-ecr";
import { FargateTaskDefinition, FargateService, Cluster, AwsLogDriver, ContainerImage, AppProtocol, Protocol } from "aws-cdk-lib/aws-ecs";
import { ManagedPolicy, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { Construct } from "constructs";
import { DotenvParseOutput } from "dotenv";

interface ContainerServiceStackProps extends StackProps {
	vpc: IVpc,
	webTierSecurityGroup: ISecurityGroup
	databaseUserId: string
	databasePassword: string
	databaseName: string
	databaseEndpoint: string
	databasePort: string
	ecrRepository: IRepository,
	envProps: DotenvParseOutput
}

export class ContainerServiceStack extends Stack {
	constructor(scope: Construct, id: string, props: ContainerServiceStackProps) {
		super(scope, id);

		const taskExecutionRole = this.createEcsExecutionRole();
		const cluster = new Cluster(this, 'RazorBlogCdkCluster', {
		  clusterName: "razorblog-cdk-cluster"  
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
	
		this.createFargateService(
			taskDefinition,
			props.webTierSecurityGroup,
			props.vpc,
			cluster
		);
	}

	private createEcsExecutionRole(): Role {
		const razorBlogTaskExecutionRole = new Role(this, 'RazorBlogFargateExeRole', {
		  assumedBy: new ServicePrincipal('ecs-tasks.amazonaws.com'),
		});
	
		// required to pull images from the ECR repository
		razorBlogTaskExecutionRole.addManagedPolicy(
		  ManagedPolicy.fromAwsManagedPolicyName('service-role/AmazonECSTaskExecutionRolePolicy')
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
			'RazorBlogCdkFargateService',
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
		databaseEndpoint: string,
		databasePort: string,
		databaseName: string,
		databaseUserId: string,
		databasePassword: string,
		envVariables: DotenvParseOutput,
		ecrRepository: IRepository
	): FargateTaskDefinition {
		const taskDefinition = new FargateTaskDefinition(
			this,
			'RazorBlogCdkFargateTaskDefinition',
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
			'RazorBlogCdkContainer',
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
}