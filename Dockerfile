FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Generate certificate
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p cert@password

# Copy project the container and publish it
COPY ./RazorBlog ./RazorBlog
RUN dotnet restore ./RazorBlog/RazorBlog.csproj
RUN dotnet publish ./RazorBlog/RazorBlog.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .
COPY --from=build-env /https/aspnetapp.pfx /https/aspnetapp.pfx

ENTRYPOINT ["dotnet", "RazorBlog.dll"]
