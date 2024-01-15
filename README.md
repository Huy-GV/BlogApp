# Razor Blog

## Overview
A Blog application where users can write blogs or comment on others.
All blogs can be monitored by Moderators and Administrators.

### Tools used
- Languages: C#, JavaScript, HTML, CSS
- Frameworks & Libraries: .NET 8 Razor Pages, .NET Blazor, .NET Identity, Entity Framework Core, Hangfire, SASS

### Features
- Users can post blogs and write comments after creating an account
- Users can upload basic information and view basic usage stats on their profile page
- Administrators can assign/ remove Moderator role to/ from any user
- Moderators can hide posts/ comments and their status will be decided by Administrators
- Administrators can create and lift bans on offending users.

## Images
### Home Page
![Screenshot (1085)](https://user-images.githubusercontent.com/78300296/145921039-838cb3af-6adc-41d9-b154-6be44df7d827.png)

### Profile Page
![Screenshot (1062)](https://user-images.githubusercontent.com/78300296/142516988-522a6d22-2af0-41a2-9b28-bf19ad9adab0.png)

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
- Initialize user secret storage:
    ```bash
    dotnet user-secrets init
    ```
- Set up AWS credentials:
    ```bash
    cd /directory/containing/RazorBlog.csproj/
    dotnet user-secrets set "Aws:AccessKey" "YourAwsAccessKeyPassword"
    dotnet user-secrets set "Aws:SecretKey" "YourAwsSecretKeyPassword"
    dotnet user-secrets set "Aws:S3:BucketName" "your-bucket-name"
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
        ConnectionStrings__DefaultConnection=Server=sqlserver;Database=RazorBlog;User ID=SA;Password=YourDbPassword;MultipleActiveResultSets=false;

        # must be the same as password in connection string
        SqlServer__Password=YourDbPassword

        # ensure certificate password and name is correct
        ASPNETCORE_Kestrel__Certificates__Default__Password=cert@password
        ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
        ```
- Run the below command in admin mode:
    ```bash
    cd /directory/containing/docker-compose.yaml/
    docker compose --env-file docker.env up --build
    ```
