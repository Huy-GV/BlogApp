FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
EXPOSE 80

# Copy project the container and publish it
COPY ./SimpleForum.Web ./SimpleForum.Web
COPY ./SimpleForum.Core ./SimpleForum.Core

RUN dotnet restore ./SimpleForum.Web/SimpleForum.Web.csproj
RUN dotnet publish ./SimpleForum.Web/SimpleForum.Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "SimpleForum.Web.dll"]
