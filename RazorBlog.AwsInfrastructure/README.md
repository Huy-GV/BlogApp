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
- Install AWS CDK, TypeScript, and Node.js packages
    ```bash
    ./scripts/install-packages.sh
    ```

### Set Up AWS Credentials
- Run the following command and specify the access key, secret key, and region
    ```bash
    aws configure --profile razor-blog
    set AWS_PROFILE=razor-blog
    ```
- Create a file named `cdk.context.json` containing availabilities zones within your region:
    ```json
    {
        "availability-zones:account=YOUR_AWS_ACCOUNT_ID:region=YOUR_AWS_REGION": [
            "YOUR_AWS_REGION_AZ_1",
            "YOUR_AWS_REGION_AZ_2",
        ]
    }
    ```

### Bootstrap CDK
- Attach 2 IAM policies to the current user:
    - [./iam/cdkBootstrapPolicy.json](iam/cdkBootstrapPolicy.json)
    - [./iam/cdkDeployPolicy.json](iam/cdkDeployPolicy.json)
- Run the bootstrap command:
    ```bash
    cdk bootstrap --profile razor-blog
    ```

### Deploy Application
- Create an `aws.env` file in [./config](./config/):
    ```env
    SeedUser__Password=SecurePassword123@@
    Database__UserId=YOUR_RDS_DB_USER_ID
    Database__Name=YOUR_RDS_INSTANCE_NAME
    SqlServer__Password=YOUR_RDS_DB_PASSWORD
    ASPNETCORE_URLS=http://+:80
    Aws__DataBucket=YOUR_S3_BUCKET_NAME
    ```
- Deploy the stacks to AWS using the command `cdk deploy STACK_NAME` in the following order:
    1. Deploy `VpcStack`
    2. Deploy `DataStoreStack`
    3. Push Docker image to ECR via [./scripts/build-ecr-image.sh](./scripts/build-ecr-image.sh)
    4. Deploy `ContainerServiceStack`
