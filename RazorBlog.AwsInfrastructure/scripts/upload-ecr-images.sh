#!/bin/bash
# Log into the AWS CLI
aws ecr get-login-password --region YOUR.AWS.REGION | docker login --username AWS --password-stdin YOUR.PRIVATE.REPOSITORY.URI
```
# Build the image and set the HTTPS certificate password as an argument
docker build -f ./aws.Dockerfile --build-arg CERT_PASSWORD=YOUR_CERT_PASSWORD -t razor-blog .
```
# Tag the image and upload it
docker tag razor-blog:latest YOUR.DOCKER.IMAGE:latest
docker push YOUR.DOCKER.IMAGE:latest
