# Simple Forum

## Overview
Simple forum application where users can write blogs or comment on blogs written by others.
All blogs can be monitored, hidden, and removed by Moderators and Administrators.

<p><em>Demo</em></p>
<img src="https://github.com/Huy-GV/RazorBlog/assets/78300296/51a252ea-0353-4e99-916e-779d93890db5" width=80% alt="demo-gif">

- Technologies
	- Frameworks: .NET Razor Pages, Blazor, Entity Framework Core, SASS, SQL Server
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

	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR;DB;CONNECTION;STRING;"
	# Optionally set custom database location
	dotnet user-secrets set "ConnectionStrings:DefaultLocation" "\\PATH\\TO\\DB\\FILE\\DATABASE_NAME.mdf"
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
	export ConnectionStrings="Server=(localdb)\mssqllocaldb;Database=SimpleForum;Trusted_Connection=True;MultipleActiveResultSets=true;"
	dotnet ef migrations add <name> -p ./SimpleForum.Core.csproj -s ./SimpleForum.Web.csproj
	dotnet ef database update -p ./SimpleForum.Core/SimpleForum.Core.csproj -s ./SimpleForum.Web/SimpleForum.Web.csproj 
	```
## Run With Docker
- Start the Docker engine and ensure it is targeting *Linux*
- Create a `.env` files with fields as shown in the below example:
	```env
	SeedUser__Password=SecurePassword123@@
	ConnectionStrings__DefaultConnection=Server=simpleforumdb;Database=SimpleForum;User ID=SA;Password=YOUR_DB_PASSWORD;MultipleActiveResultSets=false;TrustServerCertificate=True
	SqlServer__Password=YOUR_DB_PASSWORD
	```
- Start the application stack
	```bash
	docker compose --env-file .env up --build
	```

## AWS Deployment
See [AWS Deployment](./SimpleForum.AwsInfrastructure//README.md)
