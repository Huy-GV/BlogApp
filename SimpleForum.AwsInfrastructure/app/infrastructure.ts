#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { parseEnvFile } from '../config/appConfiguration';
import { VpcStack } from '../lib/vpc-stack';
import { DataStoreStack } from '../lib/data-store-stack';
import { ContainerStack } from '../lib/container-service-stack';
import { exit } from 'process';
import { CodePipelineStack } from '../lib/codepipeline-stack';

const app = new cdk.App();
const awsEnv = { account: process.env.CDK_DEFAULT_ACCOUNT, region: process.env.CDK_DEFAULT_REGION };

const appConfiguration = parseEnvFile() || exit(1);
const vpcStack = new VpcStack(app, 'SfoVpcStack', { env: awsEnv });

const dataStoreStack = new DataStoreStack(app, 'SfoDataStoreStack', {
	env: awsEnv,
	vpc: vpcStack.vpc,
	databaseTierSecurityGroup: vpcStack.databaseTierSecurityGroup,
	dataBucketName: appConfiguration.Aws__DataBucket,
	databaseUserId: appConfiguration.ConnectionStrings__UserId,
});

const containerServiceStack = new ContainerStack(app, 'SfoContainerStack', {
	env: awsEnv,
	dataBucket: dataStoreStack.dataBucket,
	vpc: vpcStack.vpc,
	loadBalancerTierSecurityGroup: vpcStack.loadBalancerTierSecurityGroup,
	webTierSecurityGroup: vpcStack.webTierSecurityGroup,
	databaseEndpoint: dataStoreStack.databaseInstance.dbInstanceEndpointAddress,
	databaseName: dataStoreStack.databaseInstance.instanceIdentifier,
	ecrRepository: dataStoreStack.repository,
	appConfiguration: appConfiguration
});

const codePipelineStack = new CodePipelineStack(app, 'SfoCodePipelineStack', {
	env: awsEnv,
	gitHubOwner: appConfiguration.GitHub__OwnerName,
	gitHubRepositoryName: appConfiguration.GitHub__RepositoryName,
	gitHubSecretName: appConfiguration.GitHub__SecretName,
	fargateService: containerServiceStack.fargateService,
	ecrRepository: dataStoreStack.repository,
	awsAccountId: appConfiguration.Aws__AccountId,
	awsContainerName: appConfiguration.Aws__ContainerName,
	awsRegion: appConfiguration.Aws__Region,
	awsRepositoryName: appConfiguration.Aws__RepositoryName
});

app.synth();
