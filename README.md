# Razor Blog

## Overview
A Blog application where users can write blogs or comment on others.
All blogs can be monitored by Moderators and Administrators.

### Technologies
- Languages: C#, JavaScript, HTML, CSS,
- Frameworks: .NET 8 Razor Pages, .NET Blazor, .NET Identity, Entity Framework Core, Hangfire, SASS, SQL Server,
- Development Tools: Docker, AWS S3, IAM, ECS Fargate, ECR, RDS, VPC

### Table of Contents
- [Overview](#overview)
  - [Features](#features)
    - [Blog Posting](#blog-posting)
    - [Moderating Users and Posts](#moderating-users-and-posts)
    - [Different Image Stores](#different-image-stores)
- [Images](#images)
  - [Home Page](#home-page)
  - [Profile Page](#profile-page)
  - [Post Hidden By Moderators](#post-hidden-by-moderators)
  - [Admin User Reviewing Reported Post](#admin-user-reviewing-reported-post)
- [Run Locally](#run-locally)
  - [Pre-requisites](#pre-requisites)
  - [Set Up Development Environment](#set-up-development-environment)
  - [Set Up AWS Image Storage](#set-up-aws-image-storage)
- [Run Inside Docker Container:](#run-inside-docker-container)
- [Full AWS Deployment](#full-aws-deployment)
  - [VPC Setup](#vpc-setup)
  - [RDS Setup](#rds-setup)
  - [ECS and ECR Setup](#ecs-and-ecr-setup)
    - [Image Repository](#image-repository)
    - [Task Definition](#task-definition)

### Features
#### Blog Posting
- Users can post blogs and write comments after creating an account
- Users can upload basic information and view basic usage stats on their profile page

#### Moderating Users and Posts
- Administrators can assign/ remove Moderator role to/ from any user
- Moderators can hide blogs and comments comments, their final status will be decided by Administrators (either un-hidden or deleted)
- Administrators can create and lift bans on offending users.

#### Different Image Stores
- Images uploaded by the user can be stored in an AWS S3 Bucket or in the local file system
	- To configure AWS S3, see the [Set Up AWS Image Storage](#set-up-aws-image-storage)
	- Even when S3 is used, the application logo and default profile image is still stored locally in `wwwroot\images\readonly\`
	- Enabling S3 to store images will not affect locally-stored ones

## Images
### Home Page
<img src="https://user-images.githubusercontent.com/78300296/145921039-838cb3af-6adc-41d9-b154-6be44df7d827.png" width=60%>

### Profile Page

<img src="https://user-images.githubusercontent.com/78300296/142516988-522a6d22-2af0-41a2-9b28-bf19ad9adab0.png" width=60%>

### Post Hidden By Moderators
<img src="https://github.com/Huy-GV/RazorBlog/assets/78300296/7ce30232-4659-457d-8d96-8c145faf0827" width=60%>

### Admin User Reviewing Reported Post
<img src="https://github.com/Huy-GV/RazorBlog/assets/78300296/228498ec-7293-4b64-92be-1ff76fc7e965" width=60%>

##  Run Locally
### Pre-requisites
- Required installations:
	- [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
	- [Docker Community](https://www.docker.com/get-started/)
	- [Microsoft SQL Server](https://www.microsoft.com/en-au/sql-server/sql-server-downloads)
- Install required Nuget packages:
	``` bash
	cd /directory/containing/RazorBlog.sln/
	dotnet restore
	```

### Set Up Development Environment
- Initialize user secret storage:
	```bash
	dotnet user-secrets init
	```
- Set up passwords for seeded user account:
	```bash
	cd /directory/containing/RazorBlog.csproj/
	dotnet user-secrets set "SeedUser:Password" "YourTestPassword"
	```
- Set up database connection
	``` bash
	cd /directory/containing/RazorBlog.csproj/

	# Optionally set custom database location
	# If this directory does not exist, it will automatically be created
	dotnet user-secrets set "ConnectionStrings:DefaultLocation" "\\Path\\To\\Database\\Directory\\DatabaseName.mdf"

	# Set up MS SQL server connection string
	# Example using a local server: "Server=(localdb)\mssqllocaldb;Database=RazorBlog;Trusted_Connection=True;MultipleActiveResultSets=true;"
	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your;Database;Connection;String;"
	```
- Start the web server in `Release` mode:
	```bash
	cd /directory/containing/RazorBlog.csproj/
	dotnet run --configuration Release
	```

### Set Up AWS Image Storage
- Create an IAM user, generate AWS credentials, and store them:
	```bash
	cd /directory/containing/RazorBlog.csproj/
	dotnet user-secrets set "Aws:AccessKey" "YourAwsAccessKeyPassword"
	dotnet user-secrets set "Aws:SecretKey" "YourAwsSecretKeyPassword"
	dotnet user-secrets set "Aws:S3:BucketName" "your-bucket-name"
	```
- To use the AWS S3 as an image store, set the flag `UseAwsS3` to `true` in your `appsettings.{env}.json` file:
	```json
	"UseAwsS3": true
	```

## Run Inside Local Docker Container:
- Start the Docker engine and ensure it is targeting *Linux*
- Generate a certificate and store it in `~/.aspnet/https` on the host machine
- Create an environment file named `docker.env` and specify the following fields:
	- `SeedUser__Password`: equivalent to `SeedData:Password`
	- `ConnectionStrings__DefaultConnection`: equivalent to `ConnectionStrings:DefaultConnection`
	- `SqlServer__Password`: password of MS SQL Server database
	- `ASPNETCORE_Kestrel__Certificates__Default__Password`: password of HTTPS certificate
	- `ASPNETCORE_Kestrel__Certificates__Default__Path`: path to certificate file
	- Example:
		```env
		# docker.env
		SeedUser__Password=SecurePassword123@@

		# ensure the server name is the same as the container name
		ConnectionStrings__DefaultConnection=Server=razorblogdb;Database=RazorBlog;User ID=SA;Password=YourDbPassword;MultipleActiveResultSets=false;

		# must be the same as password in connection string
		SqlServer__Password=YourDbPassword

		# ensure certificate password and name is correct
		ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertPassword
		ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx

		# define AWS user credentials here
		Aws__SecretKey=YourAwsSecretKeyPassword
		Aws__S3__BucketName=your-bucket-name
		Aws__AccessKey=YourAwsAccessKeyPassword
		```
- Run the below command in admin mode:
	```bash
	cd /directory/containing/docker-compose.yaml/
	docker compose --env-file docker.env up --build
	```

## Full AWS Deployment
### VPC Setup
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

### RDS Setup
- Create an RDS database with the following configurations:
	- Engine: SQL Server Express Edition
	- Set VPC to `razor-blog-vpc`
	- Set VPC Security Group to `DatabaseTier`

### ECS and ECR Setup
#### Image Repository
- Use `aws configure` to configure AWS credentials on the local machine
- Create a private repository and upload an image using the push command templates:
	- Log into the AWS CLI
		```bash
		aws ecr get-login-password --region your.aws.region | docker login --username AWS --password-stdin your.private.repository.uri
		```
	- Build the image and set the HTTPS certificate password as an argument
		```bash
		docker build -f ./aws.Dockerfile --build-arg CERT_PASSWORD=YourCertPassword -t razor-blog .
		```
	- Tag the image and upload it
		```bash
		docker tag razor-blog:latest your.docker.image:latest
		docker push your.docker.image:latest
		```

#### Task Definition
- Create a task definition with the following configurations:
	- Launch type: AWS Fargate
	- Task role and Task execution role: `ecsTaskExecutionRole`
		- In AWS IAM, ensure the role has the policy `AmazonECSTaskExecutionRolePolicy`
	- Enter the container name and use the URI of the recently uploaded image
	- Set the environment variables either manually or via an `.env` file stored in a S3 bucket
		- If S3 is used, ensure the Task execution role has the policy `AmazonS3ReadonlyAccess`
		- Example `.env` file:
			```env
			# aws.env
			SeedUser__Password=SecurePassword123@@

			# ensure the server id is set to the RDS database endpoint and database credentials are correct
			ConnectionStrings__DefaultConnection=Server=RdsEndpoint,1433;Database=RazorBlog;User ID=RdsUsername;Password=YourDbPassword;MultipleActiveResultSets=false;

			# must be the same as password in connection string
			SqlServer__Password=YourDbPassword

			# ensure certificate password and name is correct
			ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertPassword
			ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx

			# configure ports
			ASPNETCORE_URLS=https://+:443
			ASPNETCORE_HTTPS_PORT=443

			# define AWS user credentials here
			Aws__SecretKey=YourAwsSecretKeyPassword
			Aws__S3__BucketName=your-bucket-name
			Aws__AccessKey=YourAwsAccessKeyPassword
			```
	- Set the port mappings for HTTP and HTTPS traffic:
		```json
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
		},
		```
- Deploy a service using the created task definition:
	- Set Compute options to Launch type and select Fargate
- Configure networking options:
	- Set VPC to `razor-blog-vpc`
	- Set subnets to public subnets in `razor-blog-vpc`
	- Set VPC Security Group to `WebServerTier`
