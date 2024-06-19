#!/bin/bash

REPOSITORY_NAME=$1
CERT_PASSWORD=$2
DOCKERFILE_DIR=${3:-"."}

# Ensure repository exists
aws ecr describe-repositories --repository-names $REPOSITORY_NAME || aws ecr create-repository --repository-name $REPOSITORY_NAME

# Log into the AWS CLI
AWS_REGION=$(aws configure get region --profile razor-blog --output text)
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query "Account" --output text)
ECR_URI=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com
REPOSITORY_URI=$ECR_URI/$REPOSITORY_NAME

aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI

docker build -f $DOCKERFILE_DIR/AWS.Dockerfile --build-arg CERT_PASSWORD=$CERT_PASSWORD -t $REPOSITORY_URI:latest $DOCKERFILE_DIR
docker push $REPOSITORY_URI:latest
