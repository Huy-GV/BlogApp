import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as codepipeline from 'aws-cdk-lib/aws-codepipeline';
import * as codepipeline_actions from 'aws-cdk-lib/aws-codepipeline-actions';
import * as codebuild from 'aws-cdk-lib/aws-codebuild';
import { FargateService } from 'aws-cdk-lib/aws-ecs';
import { PolicyStatement } from 'aws-cdk-lib/aws-iam';
import { IRepository } from 'aws-cdk-lib/aws-ecr';

interface CodePipelineStackProps extends cdk.StackProps {
	gitHubOwner: string;
	gitHubRepositoryName: string;
	gitHubSecretName: string;
	fargateService: FargateService;
	ecrRepository: IRepository;
	awsRegion: string;
	awsAccountId: string;
	awsRepositoryName: string;
	awsContainerName: string;
}

export class CodePipelineStack extends cdk.Stack {
  	constructor(scope: Construct, id: string, props: CodePipelineStackProps) {
		super(scope, id, props);

		const buildProject = new codebuild.Project(this, 'SfoCdkCodeBuildProject', {
			source: codebuild.Source.gitHub({
				owner: props.gitHubOwner,
				repo: props.gitHubRepositoryName,
				webhook: true,
			}),
			buildSpec: codebuild.BuildSpec.fromSourceFilename('buildspec.yml'),
			environment: {
				buildImage: codebuild.LinuxBuildImage.AMAZON_LINUX_2_5,
				privileged: false,
			},
			environmentVariables: {
				AWS_REGION: { value: props.awsRegion },
				AWS_ACCOUNT_ID: { value: props.awsAccountId },
				REPOSITORY_NAME: { value: props.awsRepositoryName },
				CONTAINER_NAME: { value: props.awsContainerName },
			}
		});

		buildProject.addToRolePolicy(new PolicyStatement({
			actions: [
			  	"ecr:GetDownloadUrlForLayer",
			  	"ecr:BatchGetImage",
			  	"ecr:BatchCheckLayerAvailability",
			  	"ecr:PutImage",
			  	"ecr:InitiateLayerUpload",
			  	"ecr:UploadLayerPart",
			  	"ecr:CompleteLayerUpload",
			  	"ecr:GetAuthorizationToken",
			  	"logs:CreateLogGroup",
			  	"logs:CreateLogStream",
				"logs:PutLogEvents",
				"ssm:GetParameters"
			],
			resources: [
				props.ecrRepository.repositoryArn,
				"*"
			]
		}));

		const pipeline = new codepipeline.Pipeline(this, 'SfoCdkCodePipeline', {
			pipelineName: 'SfoCdkCodePipeline',
		});

		const sourceOutput = new codepipeline.Artifact('SourceArtifact');

		const sourceStage = pipeline.addStage({
			stageName: 'Source',
			actions: [
				new codepipeline_actions.GitHubSourceAction({
					actionName: 'GitHub_Source',
					output: sourceOutput,
					oauthToken: cdk.SecretValue.secretsManager(props.gitHubSecretName, { jsonField: "GitHubToken" }),
					owner: props.gitHubOwner,
					repo: props.gitHubRepositoryName,
					branch: 'main',
				}),
			],
		});

		const buildOutput = new codepipeline.Artifact('BuildArtifact');
		const buildStage = pipeline.addStage({
			stageName: 'Build',
			actions: [
				new codepipeline_actions.CodeBuildAction({
					actionName: 'CodeBuild',
					project: buildProject,
					input: sourceOutput,
					outputs: [buildOutput]
				}),
			],
		});

		const deployStage = pipeline.addStage({
			stageName: 'Deploy',
			actions: [
				new codepipeline_actions.EcsDeployAction(
					{
						actionName: 'DeployAction',
						service: props.fargateService,
						input: buildOutput,
					}),
				],
		});
  	}
}
