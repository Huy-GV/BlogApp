#!/bin/bash

CERT_PASSWORD=$1
SOLUTION_DIR=$2

if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Invalid arguments. HTTPS cert password and Solution directory are required."
  exit 1
fi

REPOSITORY_NAME='razorblog-cdk-repository'
aws ecr describe-repositories --repository-names $REPOSITORY_NAME || { echo "ECR repository not found"; exit 1; }

cd $SOLUTION_DIR
dotnet test || { echo "Tests failed"; exit 1; }

# Log into the AWS CLI
AWS_REGION=$(aws configure get region --profile razor-blog --output text)
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query "Account" --output text)
ECR_URI=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com
REPOSITORY_URI=$ECR_URI/$REPOSITORY_NAME

aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI

docker build -f $SOLUTION_DIR/AWS.Dockerfile --build-arg CERT_PASSWORD=$CERT_PASSWORD -t $REPOSITORY_URI:latest $SOLUTION_DIR
docker push $REPOSITORY_URI:latest
echo "Pushed image to $REPOSITORY_NAME"
