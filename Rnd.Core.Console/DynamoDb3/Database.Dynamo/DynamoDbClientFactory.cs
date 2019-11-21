﻿using System;
using Amazon;
using Amazon.DynamoDBv2;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.Configuration;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo
{
    static class DynamoDbClientFactory
    {
        public static IAmazonDynamoDB Create(DynamoDbSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = settings.LocalUrl?.ToString(),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
                UseHttp = true
            };

            if (!string.IsNullOrWhiteSpace(settings.Region))
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region);

            return new AmazonDynamoDBClient(config);
        }
    }
}
