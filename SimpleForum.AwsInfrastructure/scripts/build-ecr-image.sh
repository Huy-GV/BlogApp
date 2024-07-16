#!/bin/bash

if [ -z "$1" ]; then
  echo "Invalid arguments: solution directory is required"
  exit 1
fi

ABS_SOLUTION_DIR=$(realpath "$1")
if [ ! -d "$ABS_SOLUTION_DIR" ]; then
  echo "Invalid arguments: solution directory not found"
  exit 1
fi

REPOSITORY_NAME='sfo-cdk-repository'
aws ecr describe-repositories --repository-names $REPOSITORY_NAME || { echo "ECR repository not found"; exit 1; }

# dotnet test $ABS_SOLUTION_DIR || { echo "Tests failed"; exit 1; }

# Log into the AWS CLI
AWS_REGION=$(aws configure get region --profile simple-forum --output text)
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query "Account" --output text)
ECR_URI=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com
REPOSITORY_URI=$ECR_URI/$REPOSITORY_NAME

aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI

docker build -f $ABS_SOLUTION_DIR/AWS.Dockerfile -t $REPOSITORY_URI:latest $ABS_SOLUTION_DIR
docker push $REPOSITORY_URI:latest
echo "Pushed image to $REPOSITORY_NAME"
