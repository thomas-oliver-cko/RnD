#!/bin/sh -e
echo Initialising table

tables=$(awslocal dynamodb list-tables --region ${DEFAULT_REGION})
tableExists=$(echo "$tables" | jq '.TableNames as $tables | "Entities" | IN($tables[])')
if [ "$tableExists" = true ]
then
    echo "Table exists, deleting table"
    awslocal dynamodb delete-table --table-name "Entities"
fi

echo "Creating table"
awslocal dynamodb create-table --cli-input-json file://${DATA_ROOT_FOLDER}/dynamodb-create-table.json --region ${DEFAULT_REGION}
echo "Writing initial entities"

# Add entities in batches of 25
jsonPath="${DATA_ROOT_FOLDER}/sample-entities.dynamo.json"
length=$(jq -c '. | length' "$jsonPath")
batches=$(((length+24)/ 25))
counter=0

while [ "$counter" -lt "$batches" ]
do
    start=$((counter*25))
    end=$((start+25))
    if [ "$end" -ge "$length" ]
    then
        end=$length
    fi    
    echo "writing batch $((counter+1)) of $batches, element $((start+1))-$end"

    # Note JQ array is start inclusive but not end inclusive, hence the end + 1. It is also 0 indexed
    batch=$(jq -c ".[$start:$end] | {Entities:[{PutRequest:{Item:.[]}}]}" "$jsonPath")
    awslocal dynamodb batch-write-item --request-item "$batch" --region ${DEFAULT_REGION}
    counter=$((counter+1))
done
