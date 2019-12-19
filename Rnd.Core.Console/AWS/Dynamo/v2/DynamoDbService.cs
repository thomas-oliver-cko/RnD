using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Rnd.Core.ConsoleApp.Dynamo.v2
{
    interface IDynamoDbService
    {
        Task<bool> CreateAsync<T>(IReadOnlyList<T> request, CancellationToken token = default);
        Task<T> ReadAsync<T>(Guid id, CancellationToken token = default);
    }

    class DynamoDbService : IDynamoDbService
    {
        const int MaxBatchSize = 25;

        readonly IAmazonDynamoDB client;
        readonly IDynamoDBContext context;
        readonly string tableName;

        public DynamoDbService(IAmazonDynamoDB client, IDynamoDBContext context, DynamoDbSettings settings)
            : this(client, context, settings?.TableName)
        {
        }

        public DynamoDbService(IAmazonDynamoDB client, IDynamoDBContext context, string tableName)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException(nameof(tableName));
            this.tableName = tableName;
        }

        public async Task<bool> CreateAsync<T>(IReadOnlyList<T> request, CancellationToken token = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var batches = Batch(request);

            foreach (var batch in batches)
            {
                var response = await client.BatchWriteItemAsync(batch, token);

                // TODO TO: Make this transactional across batches i.e. if one fails we need to handle it.
                // TODO TO: Add logging/codes which describe failures
                if (response.HttpStatusCode != HttpStatusCode.OK)
                    return false;
            }

            new PutItemRequest()
            {
                TableName = tableName,
                Item = Document.FromJson("").ToAttributeMap(),
            };

            return true;
        }

        public Task<T> ReadAsync<T>(Guid id, CancellationToken token = default)
        {
            return context.LoadAsync<T>(id, token);
        }

        IEnumerable<BatchWriteItemRequest> Batch<T>(IReadOnlyList<T> request)
        {
            var batches = new List<BatchWriteItemRequest>(request.Count / MaxBatchSize + 1);
            var batch = new List<WriteRequest>(MaxBatchSize);

            for (var i = 0; i < request.Count; i++)
            {
                batch.Add(new WriteRequest
                {
                    PutRequest = new PutRequest
                    {
                        Item = context.ToDocument(request[i]).ToAttributeMap(),
                    }
                });

                if ((i + 1) % MaxBatchSize != 0) continue;

                batches.Add(new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>> { [tableName] = batch.ToList() },
                    ReturnConsumedCapacity = "TOTAL",
                    ReturnItemCollectionMetrics = ReturnItemCollectionMetrics.SIZE
                });
                batch.Clear();
            }

            if (batch.Any())
                batches.Add(new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>> { [tableName] = batch.ToList() },
                    ReturnConsumedCapacity = "TOTAL",
                    ReturnItemCollectionMetrics = ReturnItemCollectionMetrics.SIZE
                });

            return batches;
        }
    }
}
