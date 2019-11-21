using System;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Rnd.Core.ConsoleApp.DynamoDb1
{
    public class DynamoDbClientFactory
    {
        public AmazonDynamoDBClient Get()
        {
            var credentials = new BasicAWSCredentials("XX", "XX");

            var clientConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000",
                CacheHttpClient = true,
                MaxConnectionsPerServer = 1,
                MaxErrorRetry = 5,
                ThrottleRetries = true,
                Timeout = TimeSpan.FromSeconds(5)
            };

            return new AmazonDynamoDBClient(credentials, clientConfig);
        }
    }
}
