#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { parseEnvFile } from '../config/appConfiguration';
import { VpcStack } from '../lib/vpc-stack';
import { DataStoreStack } from '../lib/data-store-stack';
import { ContainerStack } from '../lib/container-service-stack';
import { exit } from 'process';

const app = new cdk.App();
const awsEnv = { account: process.env.CDK_DEFAULT_ACCOUNT, region: process.env.CDK_DEFAULT_REGION };

const appConfiguration = parseEnvFile() || exit(1);
const vpcStack = new VpcStack(app, 'RzbVpcStack', { env: awsEnv });

const dataStoreStack = new DataStoreStack(app, 'RzbDataStoreStack', {
	env: awsEnv,
	vpc: vpcStack.vpc,
	databaseTierSecurityGroup: vpcStack.databaseTierSecurityGroup,
	databaseName: appConfiguration.Database__Name,
	dataBucketName: appConfiguration.Aws__DataBucket,
	databaseUserId: appConfiguration.Database__UserId,
	databasePassword: appConfiguration.SqlServer__Password
});

const containerServiceStack = new ContainerStack(app, 'RzbContainerStack', {
	env: awsEnv,
	dataBucket: dataStoreStack.dataBucket,
	vpc: vpcStack.vpc,
	loadBalancerTierSecurityGroup: vpcStack.loadBalancerTierSecurityGroup,
	webTierSecurityGroup: vpcStack.webTierSecurityGroup,
	databaseUserId: appConfiguration.Database__UserId,
	databasePassword: appConfiguration.SqlServer__Password,
	databaseName: appConfiguration.Database__Name,
	databaseEndpoint: dataStoreStack.databaseInstance.dbInstanceEndpointAddress,
	databasePort: dataStoreStack.databaseInstance.dbInstanceEndpointPort,
	ecrRepository: dataStoreStack.repository,
	appConfiguration: appConfiguration
})

app.synth();
