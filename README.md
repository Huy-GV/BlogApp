# Razor Blog

## Overview
Simple blog application where users can write blogs or comment on blogs written by others.
All blogs can be monitored, hidden, and removed by Moderators and Administrators.

### Technologies
- Languages: C#, JavaScript, TypeScript, HTML, CSS,
- Frameworks: .NET 8 Razor Pages, .NET Blazor, .NET Identity, Entity Framework Core, Hangfire, SASS, SQL Server,
- Development Tools: Docker, AWS CDK, CloudFormation, Route 53, ALB, S3, IAM, ECS Fargate, ECR, RDS, VPC

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
	dotnet user-secrets set "ConnectionStrings:DefaultLocation" "\\PATH\\TO\\DB\\FILE\\DATABASE_NAME.mdf"

	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR;DB;CONNECTION;STRING;"
	```

### Set Up AWS Image Storage
- Create an IAM user and configure their profile locally:
	```bash
	aws configure --profile razor-blog
	export AWS_PROFILE=razor-blog
	```
- To use the AWS S3 as an image store, set the flag `UseAwsS3` to `true` in `appsettings.{env}.json`:
	```json
	{
		"UseAwsS3": true
	}
	```

## Run With Docker
- Start the Docker engine and ensure it is targeting *Linux*
- Create a `.env` files with fields as shown in the below example:
	```env
	SeedUser__Password=SecurePassword123@@
	ConnectionStrings__DefaultConnection=Server=razorblogdb;Database=RazorBlog;User ID=SA;Password=YOUR_DB_PASSWORD;MultipleActiveResultSets=false;TrustServerCertificate=True
	SqlServer__Password=YOUR_DB_PASSWORD
	```
- Run the below command in admin mode:
	```bash
	docker compose --env-file .env up --build
	```

## AWS Deployment
See [AWS Deployment](./RazorBlog.AwsInfrastructure//README.md)
