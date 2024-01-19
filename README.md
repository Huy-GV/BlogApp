# Razor Blog

## Overview
A Blog application where users can write blogs or comment on others.
All blogs can be monitored by Moderators and Administrators.

Technologies used: C#, JavaScript, HTML, CSS, .NET 8 Razor Pages, .NET Blazor, .NET Identity, Entity Framework Core, Hangfire, SASS, SQL Server, AWS S3, IAM, Docker

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
![Screenshot (1085)](https://user-images.githubusercontent.com/78300296/145921039-838cb3af-6adc-41d9-b154-6be44df7d827.png)

### Profile Page
![Screenshot (1062)](https://user-images.githubusercontent.com/78300296/142516988-522a6d22-2af0-41a2-9b28-bf19ad9adab0.png)

### Post Hidden By Moderators
![image](https://github.com/Huy-GV/RazorBlog/assets/78300296/7ce30232-4659-457d-8d96-8c145faf0827)

### Admin User Reviewing Reported Post
![image](https://github.com/Huy-GV/RazorBlog/assets/78300296/228498ec-7293-4b64-92be-1ff76fc7e965)

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
- Create a fully private S3 bucket and add a policy with the following the statement
	```json
	{
		"Effect": "Deny",
		"Principal": "*",
		"Action": "s3:*",
		"Resource": [
			"arn:aws:s3:::your-bucket-name",
			"arn:aws:s3:::your-bucket-name/*"
		],
		"Condition": {
			"NotIpAddress": {
				"aws:SourceIp": "your-ip-address"
			}
		}
	}
	```
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

## Run Inside Docker Container:
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
		ASPNETCORE_Kestrel__Certificates__Default__Password=cert@password
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
