set -e

apt-get install awscli jq -y

localstackUrl=$1

dynamoResult = false
snsResult = false
sqsResult = false

while [ $count -eq 0 ] do
    if [ ! $dynamoResult ] then
        result=$(aws dynamodb list-tables --cli-connect-timeout 2 --endpoint-url $localstackUrl | jq '.["Count"]')
        dynamoResult = -z "$result"
    fi

    if [ ! $snsResult ] then
        result=$(aws sns list-topics --cli-connect-timeout 2 --endpoint-url $localstackUrl | jq '.["Count"]')
        snsResult = -z "$result"
    fi

    if [ ! $sqsResult ] then
        result=$(aws sqs list-queues --cli-connect-timeout 2 --endpoint-url $localstackUrl | jq '.["Count"]')
        sqsResult = -z "$result"
    fi
done
