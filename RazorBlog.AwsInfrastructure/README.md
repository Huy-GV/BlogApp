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
    ```
- Create a file named `cdk.context.json` containing availabilities zones within your region:
    ```json
    {
        "availability-zones:account=YOUR_AWS_ACCOUNT_ID:region=YOUR_AWS_REGION": [
            "YOUR_AWS_REGION_AZ_1",
            "YOUR_AWS_REGION_AZ_2",
            "YOUR_AWS_REGION_AZ_3"
        ]
    }
    ```

### Boostrap CDK
- Create an IAM policy named `CdkBoostrapPolicy` (see [iam/cdkBootstrapPolicy.json](iam/cdkBootstrapPolicy.json)) and attach it to the current user
- Create an IAM policy named `CdkDeployPolicy` (see [iam/cdkDeployPolicy.json](iam/cdkDeployPolicy.json)) and attach it to the user
- Run the boostrap command:
    ```bash
    cdk boostrap
    ```

### Upload Docker Image
- Build a Docker image locally and upload it to ECR:
    ```bash
    ./scripts/build-ecr-image.sh <repository name> <HTTPS certificate password> <Dockerfile directory>?
    ```

### Deploy Stack
- Create an `.env` file in `./lib/`:
    ```env
    # Example .env file
    SeedUser__Password=SecurePassword123@@
    Database__UserId=YOUR_RDS_DB_USER_ID
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
    cdk deploy
    ```

## Infrastructure
### VPC
- VPC naamed `razor-blog-vpc` with 2 public subnets and 2 private subnets
- Security groups:
	- `DatabaseTier`:
		- Inbound Rules: MSSQL TCP traffic from `WebServerTier`
		- Outbound Rules: None
	- `WebServerTier`:
		- Inbound Rules: HTTP & HTTPS traffic from any IPv4 & IPv6 addresses
		- Outbound Rules:
			- MSSQL TCP traffic to `DatabaseTier`
			- HTTPS traffic to any IPv4 & IPv6 addresses

### RDS
- Database Subnet Groups: `razor-blog-db-subnet-group`
    - Contains private subnets within `razor-blog-vpc`
- RDS instance with the following configurations:
	- Engine: `SQL Server Express Edition`
	- VPC: `razor-blog-vpc`
	- VPC Security Group: `DatabaseTier`
	- Subnet Group: `razor-blog-db-subnet-group`
	- Public access: disallowed

### ECR
- Repository name `razor-blog-repository`

### Fargate Task
- Task definition: `RazorBlogTaskDefinition`
	- Launch type: `AWS Fargate`
	- Task role and Task execution role: `RazorBlogTaskExecutionRole` with policies:
		- `AmazonECSTaskExecutionRolePolicy`
		- `AmazonS3ReadOnlyAccess` (required if env variables are loaded from a `.env` file in S3)
	- Docker image URI: recently uploaded image in `razor-blog-repository`
	- Environment variables:
        ```env
        # Example .env file
        SeedUser__Password=SecurePassword123@@
        ConnectionStrings__DefaultConnection=Server=YOUR.RDS.ENDPOINT,1433;Database=RazorBlog;User ID=YOUR_RDS_USERNAME;Password=YOUR_RDS_DB_PASSWORD;MultipleActiveResultSets=false;TrustServerCertificate=true;
        SqlServer__Password=YOUR_RDS_DB_PASSWORD
        ASPNETCORE_Kestrel__Certificates__Default__Password=YOUR_CERT_PASSWORD
        ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
        ASPNETCORE_URLS=https://+:443
        ASPNETCORE_HTTPS_PORT=443
        Aws__SecretKey=YOUR_AWS_SECRET_KEY
        Aws__S3__BucketName=YOUR_S3_BUCKET_NAME
        Aws__AccessKey=YOUR_AWS_ACCESS_KEY
        ```
	- Port mappings:
        - HTTP traffic on 80:80
        - HTTPS traffic on 443:443
- ECS Service: `RazorBlogService`
	- Compute options: `Fargate`
- Configure networking options:
	- VPC: `razor-blog-vpc`
	- Subnets: public subnets in `razor-blog-vpc`
	- VPC Security Group: `WebServerTier`
