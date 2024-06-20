#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { parseEnvFile } from '../config/appConnfiguration';
import { VpcStack } from '../lib/vpc-stack';
import { DataStoreStack } from '../lib/data-store-stack';
import { exit } from 'process';
import { ContainerServiceStack } from '../lib/container-service-stack';

const app = new cdk.App();
const awsEnv = { account: process.env.CDK_DEFAULT_ACCOUNT, region: process.env.CDK_DEFAULT_REGION };

const appConfiguration = parseEnvFile() || exit(-1);

const vpcStack = new VpcStack(app, 'VpcStack');

const dataStoreStack = new DataStoreStack(app, 'DataStoreStack', {
	env: awsEnv,
	vpc: vpcStack.vpc,
	databaseTierSecurityGroup: vpcStack.databaseTierSecurityGroup,
	databaseName: appConfiguration.DatabaseName,
	dataBucketName: appConfiguration.AwsS3BucketName,
	databaseUserId: appConfiguration.DatabaseUserId,
	databasePassword: appConfiguration.SqlServerPassword
});

const containerServiceStack = new ContainerServiceStack(app, 'ContainerServiceStack', {
	env: awsEnv,
	vpc: vpcStack.vpc,
	webTierSecurityGroup: vpcStack.webTierSecurityGroup,
	databaseUserId: appConfiguration.DatabaseUserId,
	databasePassword: appConfiguration.SqlServerPassword,
	databaseName: appConfiguration.DatabaseName,
	databaseEndpoint: dataStoreStack.databaseInstance.dbInstanceEndpointAddress,
	databasePort: dataStoreStack.databaseInstance.dbInstanceEndpointPort,
	ecrRepository: dataStoreStack.repository,
	envProps: appConfiguration.RawConfig
})

app.synth();
