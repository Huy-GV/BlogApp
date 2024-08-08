# Simple Forum

## Overview
Simple forum application where users can create threads or comment on threads written by others.
All posts can be monitored, hidden, and removed by Moderators and Administrators.

<p><em>Demo</em></p>
<img src="https://github.com/user-attachments/assets/a29741b1-b3f8-44c5-bf5e-74b6117be172" width=80% alt="demo-gif">

- Frameworks & Libraries: .NET Razor Pages, Blazor, Entity Framework Core, SASS, SQL Server
- Development Tools: Docker, AWS CDK, Route 53, ALB, S3, ECS Fargate, RDS, VPC

##  Quick Start
### Pre-requisites
- Required installations:
	- [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
	- [Docker Community](https://www.docker.com/get-started/)
	- [Microsoft SQL Server](https://www.microsoft.com/en-au/sql-server/sql-server-downloads)

### Set Up Development Environment
- Initialize user secret storage in the `SimpleForum.Web` project:
	```bash
	dotnet user-secrets init
	```
- Set up passwords for seeded admin user account and database connection:
	```bash
	cd /directory/containing/SimpleForum.Web.csproj/
	dotnet user-secrets set "SeedUser:Password" "YourTestPassword"
	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=SimpleForum;Trusted_Connection=True;MultipleActiveResultSets=true;"
	```
- Create an IAM user and configure their profile locally:
	```bash
	aws configure --profile simple-forum
	export AWS_PROFILE=simple-frum
	```
- To use the AWS S3 to store images, set `FeatureFlags:UseAwsS3` to `true` in `appsettings.{env}.json`:
- To use Hangfire background service, set `FeatureFlags:UseHangFire` to `true` in `appsettings.{env}.json`:
- Running EF migrations:
	```bash
	export ConnectionString="Server=(localdb)\mssqllocaldb;Database=SimpleForum;Trusted_Connection=True;MultipleActiveResultSets=true;"
	dotnet ef migrations add <name> -p ./SimpleForum.Core/SimpleForum.Core.csproj -s ./SimpleForum.Web/SimpleForum.Web.csproj
	dotnet ef database update -p ./SimpleForum.Core/SimpleForum.Core.csproj -s ./SimpleForum.Web/SimpleForum.Web.csproj
	```
## Run With Docker
- Start the Docker engine and ensure it is targeting *Linux*
- Create a `.env` files with fields as shown in the below example:
	```env
	SeedUser__Password=SecurePassword123@@
	ConnectionStrings__Endpoint=simpleforumdb
	ConnectionStrings__DatabaseName=SimpleForum
	ConnectionStrings__UserId=SA
	ConnectionStrings__Password=YourDbPassword
	```
- Start the application stack
	```bash
	docker compose --env-file .env up --build
	```

## AWS Deployment
See [AWS Deployment](./SimpleForum.AwsInfrastructure//README.md)
