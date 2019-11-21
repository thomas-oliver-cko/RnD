using System;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Rnd.Core.ConsoleApp.DynamoDb2.Client.Strategies
{
    class DynamoDbDockerClientStrategy : IDynamoDbClientStrategy
    {
        public DynamoDbHost Host { get; } = DynamoDbHost.Docker;

        public IAmazonDynamoDB Create(DynamoDbSettings settings)
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = settings.LocalUrl.ToString(),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
            };

            var credentials = new BasicAWSCredentials("123", "123");
            return new AmazonDynamoDBClient(credentials, config);
        }
    }
}
