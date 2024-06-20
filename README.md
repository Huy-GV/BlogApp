# Razor Blog

## Overview
Simple blog application where users can write blogs or comment on blogs written by others.
All blogs can be monitored by Moderators and Administrators.

### Technologies
- Languages: C#, JavaScript, TypeScript, HTML, CSS,
- Frameworks: .NET 8 Razor Pages, .NET Blazor, .NET Identity, Entity Framework Core, Hangfire, SASS, SQL Server,
- Development Tools: Docker, AWS CDK, CloudFormation, S3, IAM, ECS Fargate, ECR, RDS, VPC

### Table of Contents
- [Overview](#overview)
- [Images](#images)
- [Quick Start](#quick-start)
- [Run With Docker](#run-with-docker)
- [AWS Deployment](./RazorBlog.AwsInfrastructure/README.md)

### Features
#### Blog Posting
- Users can post blogs and write comments after creating an account
- Users can upload basic information and view basic usage stats on their profile page

#### Moderating Users and Posts
- Administrators can assign/ remove Moderator role to/ from any user
- Moderators can hide blogs and comments, the final status of which will be decided by Administrators (either un-hidden or deleted)
	- Posts deleted by Administrators will have their content changed to `Deleted by administrators` temporarily before being removed completely
- Administrators can create and lift bans on offending users.
	- Temporary bans will be automatically lifted by a background service.

#### Different Image Stores
- Images uploaded by the user can be stored in an AWS S3 Bucket or in the local file system
	- To configure AWS S3, see the [Set Up AWS Image Storage](#set-up-aws-image-storage)
	- Even when S3 is used, the application logo and default profile image is still stored locally in `wwwroot\images\readonly\`
	- Enabling S3 to store images will not affect locally-stored ones
    - Mixing both image stores may cause the deletion of user-uploaded images to fail

## Images
### Home Page
<img src="https://user-images.githubusercontent.com/78300296/145921039-838cb3af-6adc-41d9-b154-6be44df7d827.png" width=60% alt="home-page-image">

### Post Hidden By Moderators
<img src="https://github.com/Huy-GV/RazorBlog/assets/78300296/7ce30232-4659-457d-8d96-8c145faf0827" width=60% alt="hidden-post-image">

### Admin User Reviewing Reported Post
<img src="https://github.com/Huy-GV/RazorBlog/assets/78300296/228498ec-7293-4b64-92be-1ff76fc7e965" width=60% alt="reported-post-image">

##  Quick Start
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
- Initialize user secret storage in the `Razor.Web` project:
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
	dotnet user-secrets set "ConnectionStrings:DefaultLocation" "\\PATH\\TO\\DB\\FILE\\DATABASE_NAME.mdf"

	# Set up MS SQL server connection string
	# Example using a local server: "Server=(localdb)\mssqllocaldb;Database=RazorBlog;Trusted_Connection=True;MultipleActiveResultSets=true;"
	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR;DB;CONNECTION;STRING;"
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
	dotnet user-secrets set "Aws:AccessKey" "YOUR_AWS_ACCESS_KEY"
	dotnet user-secrets set "Aws:SecretKey" "YOUR_AWS_SECRET_KEY"
	dotnet user-secrets set "Aws:S3:BucketName" "YOUR_S3_BUCKET_NAME"
	```
- To use the AWS S3 as an image store, set the flag `UseAwsS3` to `true` in your `appsettings.{env}.json` file:
	```json
		{
			"UseAwsS3": true
		}
	```

## Run With Docker
- Start the Docker engine and ensure it is targeting *Linux*
- Generate a `.pfx` certificate and store it in `~/.aspnet/https` on the host machine
- Create a `.env` files with fields as shown in the below example:
	```env
	# Example .env
	SeedUser__Password=SecurePassword123@@
	ConnectionStrings__DefaultConnection=Server=razorblogdb;Database=RazorBlog;User ID=SA;Password=YOUR_DB_PASSWORD;MultipleActiveResultSets=false;TrustServerCertificate=True
	SqlServer__Password=YOUR_DB_PASSWORD

	ASPNETCORE_Kestrel__Certificates__Default__Password=YOUR_CERT_PASSWORD
	ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx

	Aws__SecretKey=YOUR_AWS_SECRET_KEY
	Aws__S3__BucketName=YOUR_S3_BUCKET_NAME
	Aws__AccessKey=YOUR_AWS_ACCESS_KEY
	```
- Run the below command in admin mode:
	```bash
	cd /directory/containing/docker-compose.yaml/
	docker compose --env-file .env up --build
	```

## AWS Deployment
See [AWS Deployment](./RazorBlog.AwsInfrastructure//README.md)
