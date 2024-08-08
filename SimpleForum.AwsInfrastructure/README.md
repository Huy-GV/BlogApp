# AWS Deployment
## Overview
This document describes the AWS deployment process using the CDK with TypeScript.

### Table of Contents
- [Quick Start](#quick-start)
- [Infrastructure](#infrastructure)

## Quick Start
### Pre-requisites
- Required installations
    - [Node.js](https://nodejs.org/en/download/current)
    - [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html#getting-started-install-instructions)
    - TypeScript: `npm install -g typescript`
    - AWS CDK: `npm -g install typescript`
    - Node.js packages: `npm install`

### Set Up AWS Credentials
- Run the following command and specify the access key, secret key, and region
    ```bash
    aws configure --profile simple-forum
    set AWS_PROFILE=simple-forum
    ```
- Generate a GitHub Token with `admin:repo_hook`, `repo` scopes and add it to AWS Secret Manager with the key `GitHubToken`
- Upload secrets to AWS Secret Manager:
    ```bash
    ./scripts/set-aws-secrets.sh /sfo/prod/db/password "DbPassword123"
    ./scripts/set-aws-secrets.sh /sfo/prod/seeduser/password "SecurePassword123@@"
    ```

### Bootstrap CDK
- Attach 2 IAM policies to the current user:
    - [./iam/cdkBootstrapPolicy.json](iam/cdkBootstrapPolicy.json)
    - [./iam/cdkDeployPolicy.json](iam/cdkDeployPolicy.json)
- Run the bootstrap command:
    ```bash
    cdk bootstrap --profile simple-forum
    ```

### Deploy Application
- Create an `aws.env` file in [./config](./config/) containing properties of [AppConfiguration](./config/appConfiguration.ts)
- Deploy the stacks to AWS in the following order:
    1. Deploy the VPC stack:
        ```bash
        cdk deploy SfoVpcStack
        ```
    2. Deploy the data store stack:
        ```bash
        cdk deploy SfoDataStoreStack
        ```
    3. Deploy the container stack:
        ```bash
        cdk deploy SfoContainerStack
        ```
    4. Deploy the CI/CD pipeline stack:
        ```bash
        cdk deploy SfoCodePipelineStack
        ```
