using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo
{
    interface IDynamoDbClient
    {
        Task PutAsync(PutItemRequest request, CancellationToken token = default);
        Task<Dictionary<string, AttributeValue>> GetAsync(GetItemRequest request, CancellationToken token = default);
        Task<List<Dictionary<string, AttributeValue>>> QueryAsync(QueryRequest request, CancellationToken token = default);
    }

    class DynamoDbClient : IDynamoDbClient
    {
        readonly IAmazonDynamoDB client;
        readonly IDependencyCoordinator coordinator;

        public DynamoDbClient(IAmazonDynamoDB client, IDependencyCoordinator coordinator)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public async Task PutAsync(PutItemRequest request, CancellationToken token = default)
        {
            var response = await coordinator.CallDependency(
                () => client.PutItemAsync(request, token), nameof(client.PutItemAsync),
                (result, metadata) => metadata.Add("CapacityUnits", result.ConsumedCapacity.CapacityUnits));

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new DynamoDbCallException(response);
        }

        public async Task<Dictionary<string, AttributeValue>> GetAsync(GetItemRequest request, CancellationToken token = default)
        {
            var response = await coordinator.CallDependency(
                () => client.GetItemAsync(request, token), nameof(client.GetItemAsync),
                (result, metadata) => metadata.Add("CapacityUnits", result.ConsumedCapacity.CapacityUnits));

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new DynamoDbCallException(response);

            return response.Item;
        }

        public async Task<List<Dictionary<string, AttributeValue>>> QueryAsync(QueryRequest request, CancellationToken token = default)
        {
            var response = await coordinator.CallDependency(
                () => client.QueryAsync(request, token), nameof(client.GetItemAsync),
                (result, metadata) => metadata.Add("CapacityUnits", result.ConsumedCapacity.CapacityUnits));

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new DynamoDbCallException(response);

            return response.Items;
        }
    }
}
