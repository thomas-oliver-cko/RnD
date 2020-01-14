using Amazon.DynamoDBv2;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v2.Client.Strategies
{
    interface IDynamoDbClientStrategy
    {
        DynamoDbHost Host { get; }
        IAmazonDynamoDB Create(DynamoDbSettings settings);
    }
}