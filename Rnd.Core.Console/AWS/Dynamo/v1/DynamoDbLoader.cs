using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v1
{
    public class DynamoDbLoader
    {
        readonly IAmazonDynamoDB client;
        readonly IDynamoDBContext context;
        readonly string tableName;

        public DynamoDbLoader(IAmazonDynamoDB client, IDynamoDBContext context, string tableName)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public async Task LoadEntitiesAsync(List<Entity> entities)
        {
            var batches = Batch(entities);

            var metrics = new DynamoDbMetrics("Batch Load", OperationType.Load);
            var batchMetrics = new ConcurrentBag<DynamoDbMetrics>();

            await metrics.HandleAsync(Load(batchMetrics, batches));
            metrics.Records = entities.Count;
            metrics.WriteCapacityUnits = batchMetrics.Sum(s => s.WriteCapacityUnits);
            metrics.Write();
        }

        async Task Load(ConcurrentBag<DynamoDbMetrics> batchMetrics, List<BatchWriteItemRequest> batches)
        {
            foreach (var batch in batches)
            {
                var batchMetric = new DynamoDbMetrics("Load", OperationType.Load);

                var response = await batchMetric.HandleAsync(client.BatchWriteItemAsync(batch));

                if (response.UnprocessedItems.Any() || response.HttpStatusCode != HttpStatusCode.OK ||
                    response.ItemCollectionMetrics.Any() || response.ResponseMetadata.Metadata.Any())
                    batchMetric.Error += JsonConvert.SerializeObject(response) + Environment.NewLine;

                batchMetric.WriteCapacityUnits = response.ConsumedCapacity.Capacity;
                batchMetrics.Add(batchMetric);
            }
        }

        List<BatchWriteItemRequest> Batch<T>(IReadOnlyCollection<T> objects)
        {

            const int batchSize = 25;

            var batches = new List<BatchWriteItemRequest>();
            for (var i = 0; i < objects.Count; i += batchSize)
            {
                var dataToTake = objects.Count - i < batchSize
                    ? i % objects.Count
                    : batchSize;

                if (dataToTake == 0)
                    continue;

                if (dataToTake > batchSize)
                    throw new Exception();

                var batch = objects.Skip(i).Take(dataToTake).Select(s => new WriteRequest
                {
                    PutRequest = new PutRequest
                    {
                        Item = context.ToDocument(s).ToAttributeMap()
                    }
                });

                batches.Add(new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        [tableName] = batch.ToList()
                    },
                    ReturnConsumedCapacity = "TOTAL",
                    ReturnItemCollectionMetrics = ReturnItemCollectionMetrics.SIZE,
                });
            }

            return batches;
        }
    }
}
