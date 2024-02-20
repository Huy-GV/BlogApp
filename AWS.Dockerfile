FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
EXPOSE 443
EXPOSE 80

# Copy project the container and publish it
COPY ./RazorBlog.Web ./RazorBlog.Web
COPY ./RazorBlog.Core ./RazorBlog.Core

RUN dotnet restore ./RazorBlog.Web/RazorBlog.Web.csproj
RUN dotnet publish ./RazorBlog.Web/RazorBlog.Web.csproj -c Release -o out

# generate a self-signed HTTPS certificate via a commandline argument
ARG CERT_PASSWORD
RUN dotnet dev-certs https -ep /app/aspnetapp.pfx -p ${CERT_PASSWORD}

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .
COPY --from=build-env /app/aspnetapp.pfx .

ENTRYPOINT ["dotnet", "RazorBlog.Web.dll"]
