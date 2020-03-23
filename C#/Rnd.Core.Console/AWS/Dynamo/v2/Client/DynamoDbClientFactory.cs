using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Rnd.Core.ConsoleApp.AWS.Dynamo.v2.Client.Strategies;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v2.Client
{
    public interface IDynamoDbClientFactory
    {
        IAmazonDynamoDB Create(DynamoDbSettings settings);
    }

    class DynamoDbClientFactory : IDynamoDbClientFactory
    {
        readonly Dictionary<DynamoDbHost, IDynamoDbClientStrategy> strategies;

        public DynamoDbClientFactory(IEnumerable<IDynamoDbClientStrategy> strategies)
        {
            var strategiesArray = strategies?.ToArray();

            if (strategiesArray == null || !strategiesArray.Any())
                throw new ArgumentException("Must contain strategies", nameof(strategies));

            this.strategies = strategiesArray.ToDictionary(s => s.Host, s => s);
        }

        public IAmazonDynamoDB Create(DynamoDbSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (!strategies.TryGetValue(settings.DynamoDbHost, out var strategy))
                throw new KeyNotFoundException($"A strategy must be provided for a {settings.DynamoDbHost} host");
            
            return strategy.Create(settings);
        }
    }
}
