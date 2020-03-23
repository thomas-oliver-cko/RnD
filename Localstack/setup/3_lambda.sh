# !/bin/bash -e

ROLENAME=lambda-role

awslocal iam create-role --role-name $ROLENAME --assume-role-policy-document file://${DATA_ROOT_FOLDER}/lambda-assume-role.json
awslocal iam attach-role-policy --role-name $ROLENAME --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
awslocal iam attach-role-policy --role-name $ROLENAME --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess
awslocal iam attach-role-policy --role-name $ROLENAME --policy-arn arn:aws:iam::aws:policy/AmazonDynamoDBFullAccess

awslocal lambda create-function --function-name dynamo-db-load --zip-file fileb://${DATA_ROOT_FOLDER}/publish.zip \
    --handler Rnd.Core.Lambda.DynamoDb::Rnd.Core.Lambda.DynamoDb.Function::FunctionHandler \
    --runtime dotnetcore2.1 --role arn:aws:iam::123456789012:role/$ROLENAME