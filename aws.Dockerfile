FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Copy project the container and publish it
COPY ./RazorBlog ./RazorBlog
RUN dotnet restore ./RazorBlog/RazorBlog.csproj
RUN dotnet publish ./RazorBlog/RazorBlog.csproj -c Release -o out

# generate a HTTPS certificate via a cmd arg
ARG CERT_PASSWORD
RUN dotnet dev-certs https -ep /app/aspnetapp.pfx -p ${CERT_PASSWORD}

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .
COPY --from=build-env /app/aspnetapp.pfx .

ENTRYPOINT ["dotnet", "RazorBlog.dll"]
