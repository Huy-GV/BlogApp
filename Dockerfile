FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Copy project the container and publish it
COPY ./RazorBlog.Web ./RazorBlog.Web
COPY ./RazorBlog.Core ./RazorBlog.Core

RUN dotnet restore ./RazorBlog.Web/RazorBlog.Web.csproj
RUN dotnet publish ./RazorBlog.Web/RazorBlog.Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .
