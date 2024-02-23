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
- Install AWS CDK and TypeScript globally
    ```bash
    npm install -g aws-cdk
    npm install -g TypeScript
    ```

### Set Up AWS Credentials
- Run the following command and specify the access key, secret key, and region
    ```bash
    aws configure
    ```
- Create a file named `cdk.context.json`:
    ```json
    {
        "availability-zones:account=YOUR_AWS_ACCOUNT_ID:region=ap-southeast-2": [
            "ap-southeast-2a",
            "ap-southeast-2b",
            "ap-southeast-2c"
        ]
    }
    ```

### Boostrap CDK
- Create an IAM policy named `CdkBoostrapPolicy` containing the below permissions and attach it to the IAM user:
    ```json
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Action": [
                    "cloudformation:CreateChangeSet",
                    "cloudformation:DeleteStack",
                    "cloudformation:DescribeChangeSet",
                    "cloudformation:DescribeStackEvents",
                    "cloudformation:DescribeStacks",
                    "cloudformation:ExecuteChangeSet",
                    "cloudformation:GetTemplate"
                ],
                "Resource": "arn:aws:cloudformation:ap-southeast-2:YOUR_AWS_ACCOUNT_ID:stack/CDKToolkit/*",
                "Effect": "Allow",
                "Sid": "CloudFormationPermissions"
            },
            {
                "Action": [
                    "iam:CreateRole",
                    "iam:DeleteRole",
                    "iam:GetRole",
                    "iam:TagRole",
                    "iam:AttachRolePolicy",
                    "iam:DetachRolePolicy",
                    "iam:DeleteRolePolicy",
                    "iam:PutRolePolicy"
                ],
                "Effect": "Allow",
                "Resource": [
                    "arn:aws:iam::*:policy/*",
                    "arn:aws:iam::*:role/cdk-*"
                ]
            },
            {
                "Action": [
                    "s3:CreateBucket",
                    "s3:DeleteBucket",
                    "s3:PutBucketPolicy",
                    "s3:DeleteBucketPolicy",
                    "s3:PutBucketPublicAccessBlock",
                    "s3:PutBucketVersioning",
                    "s3:PutEncryptionConfiguration",
                    "s3:PutLifecycleConfiguration"
                ],
                "Effect": "Allow",
                "Resource": [
                    "arn:aws:s3:::cdk-*"
                ]
            },
            {
                "Action": [
                    "ssm:DeleteParameter",
                    "ssm:GetParameter",
                    "ssm:GetParameters",
                    "ssm:PutParameter"
                ],
                "Effect": "Allow",
                "Resource": [
                    "arn:aws:ssm:ap-southeast-2:YOUR_AWS_ACCOUNT_ID:parameter/cdk-bootstrap/*"
                ]
            },
            {
                "Action": [
                    "ecr:CreateRepository",
                    "ecr:DeleteRepository",
                    "ecr:DescribeRepositories",
                    "ecr:SetRepositoryPolicy",
                    "ecr:PutLifecyclePolicy"
                ],
                "Effect": "Allow",
                "Resource": [
                    "arn:aws:ecr:ap-southeast-2:YOUR_AWS_ACCOUNT_ID:repository/cdk-*"
                ]
            }
        ]
    }
    ```
- Run the boostrap command:
    ```bash
    cdk boostrap
    ```
- Create an IAM policy named `CdkDeployPolicy` with the following permissions and attach it to the IAM user:
    ```json
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Sid": "VisualEditor0",
                "Effect": "Allow",
                "Action": [
                    "iam:PassRole",
                    "cloudformation:DescribeStacks"
                ],
                "Resource": [
                    "arn:aws:cloudformation:ap-southeast-2:YOUR_AWS_ACCOUNT_ID:stack/CdkStack/*",
                    "arn:aws:iam::YOUR_AWS_ACCOUNT_ID:role/cdk-*"
                ]
            },
            {
                "Sid": "VisualEditor1",
                "Effect": "Allow",
                "Action": "cloudformation:ListStacks",
                "Resource": "*"
            },
            {
                "Sid": "VisualEditor2",
                "Effect": "Allow",
                "Action": "cloudformation:DescribeStacks",
                "Resource": "arn:aws:cloudformation:ap-southeast-2:YOUR_AWS_ACCOUNT_ID:stack/CdkStack/*"
            },
            {
                "Effect": "Allow",
                "Action": [
                    "sts:AssumeRole"
                ],
                "Resource": [
                    "arn:aws:iam::*:role/cdk-*"
                ]
            }
        ]
    }
    ```

### Upload Docker Image
- The CDK stack requires a Docker image to be uploaded to a private ECR repository with the tag `latest`
    1. Create a private repository named `razor-blog-repository`
    2. Upload an image using the push command templates:
        - Log into the AWS CLI
            ```bash
            aws ecr get-login-password --region YOUR.AWS.REGION | docker login --username AWS --password-stdin YOUR.PRIVATE.REPOSITORY.URI
            ```
        - Build the image and set the HTTPS certificate password as an argument
            ```bash
            docker build -f ./aws.Dockerfile --build-arg CERT_PASSWORD=YOUR_CERT_PASSWORD -t razor-blog .
            ```
        - Tag the image and upload it
            ```bash
            docker tag razor-blog:latest YOUR.DOCKER.IMAGE:latest
            docker push YOUR.DOCKER.IMAGE:latest
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
- Compare deployed stack with current state
    ```bash
    cdk diff
    ```
- Generate CloudFormation template
    ```bash
    cdk synth
    ```

## Infrastructure
### VPC
- Create a VPC named `razor-blog-vpc` with 2 public subnets and 2 private subnets
- Create two security groups:
	- `DatabaseTier`:
		- Inbound Rules:
			- MSSQL TCP traffic from `WebServerTier`
		- Outbound Rules:
			- None
	- `WebServerTier`:
		- Inbound Rules:
			- HTTP traffic from any IPv4 addresses
			- HTTP traffic from any IPv6 addresses
			- HTTPS traffic from any IPv4 addresses
			- HTTPS traffic from any IPv6 addresses
		- Outbound Rules:
			- MSSQL TCP traffic to `DatabaseTier`
			- HTTPS traffic to any IPv4 addresses
			- HTTPS traffic to any IPv6 addresses

### RDS
- Create a Database Subnet Groups `razor-blog-db-subnet-group` that contains the private subnets within `razor-blog-vpc`
- Create an RDS database with the following configurations:
	- Engine: `SQL Server Express Edition`
	- Set VPC to `razor-blog-vpc`
	- Set VPC Security Group to `DatabaseTier`
	- Set Subnet Group to `razor-blog-db-subnet-group`
	- Disallow public access

### Fargate Task
- Create a task definition with the following configurations:
	- Launch type: `AWS Fargate`
	- Create Task role and Task execution role: `RazorBlogTaskExecutionRole` with the policies:
		- `AmazonECSTaskExecutionRolePolicy`
		- `AmazonS3ReadOnlyAccess` (required if env variables are loaded from a `.env` file in S3)
	- Enter the URI of the recently uploaded Docker image
	- Set the environment variables either manually or via an `.env` file stored in a S3 bucket
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
	- Set the port mappings for HTTP and HTTPS traffic:
		```json
		[
			{
				"containerPort": 80,
				"hostPort": 80,
				"protocol": "tcp",
				"appProtocol": "http"
			},
			{
				"containerPort": 443,
				"hostPort": 443,
				"protocol": "tcp",
				"appProtocol": "http"
			}
		]
		```
- Deploy a service using the created task definition:
	- Set Compute options to `Launch type` and select `Fargate`
- Configure networking options:
	- Set VPC to `razor-blog-vpc`
	- Set subnets to public subnets in `razor-blog-vpc`
	- Set VPC Security Group to `WebServerTier`
