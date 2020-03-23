#!/bin/bash -e 

echo ${DATA_ROOT_FOLDER}/entities.csv
BUCKETNAME=dynamo-db-load

awslocal s3api create-bucket --bucket $BUCKETNAME
awslocal s3api put-bucket-notification-configuration --bucket $BUCKETNAME --notification-configuration file://${DATA_ROOT_FOLDER}/notification-config.json

awslocal s3api put-object --bucket $BUCKETNAME --key entities.csv --body ${DATA_ROOT_FOLDER}/entities.csv
