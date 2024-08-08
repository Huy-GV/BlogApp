#!/bin/bash

if [ "$#" -ne 2 ]; then
  echo "Usage: $0 <secret-name> <secret-key> <secret-value>"
  exit 1
fi

SECRET_NAME=$1
SECRET_VALUE=$2

# space before secret name is needed for some reason
aws ssm put-parameter --name " $SECRET_NAME" --value "$SECRET_VALUE" --type "SecureString" --overwrite
