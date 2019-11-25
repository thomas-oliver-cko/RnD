using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.Configuration;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.DataObject;

namespace Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo
{
    class DynamoDbService 
    {
        readonly IDynamoDbClient client;
        readonly IMapper mapper;
        readonly DynamoDbSettings settings;

        public DynamoDbService(IDynamoDbClient client, IMapper mapper, DynamoDbSettings settings)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Task CreateEntityAsync(Entity entity, CancellationToken token = default)
        {
            var dbEntity = mapper.Map<DynamoDbEntity>(entity);
            var item = dbEntity.ToAttributeValues();

            var request = new PutItemRequest
            {
                TableName = settings.TableName,
                Item = item,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            return client.PutAsync(request, token);
        }

        public async Task<Entity> ReadEntityAsync(string id, CancellationToken token = default)
        {
            var request = new GetItemRequest
            {
                TableName = settings.TableName,
                Key = new Dictionary<string, AttributeValue>
                    { [nameof(DynamoDbEntity.EntityId)] = new AttributeValue(id) },
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                ConsistentRead = false
            };

            var response = await client.GetAsync(request, token);

            var dynamoEntity = DynamoDbEntity.FromAttributeValues(response);
            return mapper.Map<Entity>(dynamoEntity);
        }

        public async Task<List<Entity>> ReadEntitiesByClientAsync(string clientId, CancellationToken token = default)
        {
            var request = new QueryRequest
            {
                TableName = settings.TableName,
                IndexName = "ClientId-EntityId-Index",
                KeyConditionExpression = "ClientId = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    { [":id"] = new AttributeValue(clientId) },
                Limit = 100,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            var response = await client.QueryAsync(request, token);

            return response.Select(DynamoDbEntity.FromAttributeValues)
                .Select(mapper.Map<Entity>)
                .ToList();
        }
    }
}
