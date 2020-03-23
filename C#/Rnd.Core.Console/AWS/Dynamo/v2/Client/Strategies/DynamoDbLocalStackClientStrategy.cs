using System;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v2.Client.Strategies
{
    class DynamoDbLocalStackClientStrategy : IDynamoDbClientStrategy
    {
        public DynamoDbHost Host { get; } = DynamoDbHost.LocalStack;

        public IAmazonDynamoDB Create(DynamoDbSettings settings)
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = settings.LocalUrl.ToString(),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
                UseHttp = true
            };

            var credentials = new BasicAWSCredentials("123", "123");
            return new AmazonDynamoDBClient(credentials, config);
        }
    }
}
