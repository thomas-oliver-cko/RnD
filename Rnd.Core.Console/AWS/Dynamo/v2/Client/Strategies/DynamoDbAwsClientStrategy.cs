using System;
using Amazon;
using Amazon.DynamoDBv2;

namespace Rnd.Core.ConsoleApp.Dynamo.v2.Client.Strategies
{
    class DynamoDbAwsClientStrategy : IDynamoDbClientStrategy
    {
        public DynamoDbHost Host { get; } = DynamoDbHost.Aws;

        public IAmazonDynamoDB Create(DynamoDbSettings settings)
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
                UseHttp = true
            };

            return new AmazonDynamoDBClient(config);
        }
    }
}
