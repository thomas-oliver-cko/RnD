using System;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v2
{
    public class DynamoDbSettings
    {
        public string Region { get; set; }
        public int TimeoutSeconds { get; set; } = 10;
        public Uri LocalUrl { get; set; }
        public DynamoDbHost DynamoDbHost { get; set; }
        public string TableName { get; set; }
    }

    public enum DynamoDbHost
    {
        LocalStack,
        Docker,
        Aws
    }
}
