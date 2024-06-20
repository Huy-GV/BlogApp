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
    - [iam/cdkBootstrapPolicy.json](iam/cdkBootstrapPolicy.json)
    - [iam/cdkDeployPolicy.json](iam/cdkDeployPolicy.json)
- Run the boostrap command:
    ```bash
    cdk bootstrap --profile razor-blog
    ```

### Upload Docker Image
1. Ensure the ECR repository already exists by deploying the `DataStoreStack`:
    ```bash
    cdk deploy DataStoreStack
    ```
2. Build a Docker image locally and upload it to ECR:
    ```bash
    ./scripts/build-ecr-image.sh YOUR_HTTPS_CERT_PASSWORD YOUR_DOCKERFILE_DIR
    ```

### Deploy Stack
- Create an `aws.env` file in [./config](./config/):
    ```env
    SeedUser__Password=SecurePassword123@@
    Database__UserId=YOUR_RDS_DB_USER_ID
    Database__Name=YOUR_RDS_INSTANCE_NAME
    SqlServer__Password=YOUR_RDS_DB_PASSWORD
    ASPNETCORE_URLS=https://+:443
    ASPNETCORE_HTTPS_PORT=443
    ASPNETCORE_Kestrel__Certificates__Default__Password=YOUR_CERT_PASSWORD
    ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
    Aws__SecretKey=YOUR_AWS_SECRET_KEY
    Aws__S3__BucketName=YOUR_S3_BUCKET_NAME
    Aws__AccessKey=YOUR_AWS_ACCESS_KEY
    ```
- Deploy the stack to AWS using the command:
    ```bash
    cdk deploy --all
    ```
