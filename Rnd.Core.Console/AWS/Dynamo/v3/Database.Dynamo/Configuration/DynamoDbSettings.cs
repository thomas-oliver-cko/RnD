using System;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Dynamo.Configuration
{
    public class DynamoDbSettings
    {
        public string Region { get; set; }
        public int TimeoutSeconds { get; set; } = 10;
        public Uri LocalUrl { get; set; }
        public string TableName { get; set; }
    }
}
