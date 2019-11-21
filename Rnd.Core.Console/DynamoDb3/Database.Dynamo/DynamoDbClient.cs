using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Checkout.Marketplace.Core.Logging.Dependencies;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.DataObject;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo
{
    interface IDynamoDbClient
    {
        Task SaveAsync(Dictionary<string, AttributeValue> item, string tableName, CancellationToken token = default);
        Task<Dictionary<string, AttributeValue>> ReadAsync(string id, string tableName, CancellationToken token = default);
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

        public async Task SaveAsync(Dictionary<string, AttributeValue> item, string tableName, CancellationToken token = default)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                ReturnValues = ReturnValue.UPDATED_NEW
            };
            
            var response = await coordinator.CallDependency(
                () => client.PutItemAsync(request, token), nameof(client.PutItemAsync),
                (result, metadata) => metadata.Add("WCUs", result.ConsumedCapacity.WriteCapacityUnits));

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new DynamoDbCallException(response);
        }

        public async Task<Dictionary<string, AttributeValue>> ReadAsync(string id, string tableName, CancellationToken token = default)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                    { [nameof(DynamoDbEntity.EntityId)] = new AttributeValue(id) },
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            var response = await coordinator.CallDependency(
                () => client.GetItemAsync(request, token), nameof(client.GetItemAsync),
                (result, metadata) => metadata.Add("RCUs", result.ConsumedCapacity.ReadCapacityUnits));

            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new DynamoDbCallException(response);

            return response.Item;
        }
    }
}
