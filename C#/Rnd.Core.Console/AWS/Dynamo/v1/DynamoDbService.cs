using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Rnd.Core.ConsoleApp.DataGenerator.Entities;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v1
{
    public class DynamoDbService
    {
        readonly IAmazonDynamoDB client;
        readonly string tableName;

        public DynamoDbService(IAmazonDynamoDB client, string tableName)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public async Task GetEntityDetails(Guid entityId)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                ReturnConsumedCapacity = "TOTAL",
                Key = new Dictionary<string, AttributeValue>
                {
                    [nameof(Entity.EntityId)] = new AttributeValue(entityId.ToString()),
                }
            };

            var metrics = new DynamoDbMetrics("Get Entity", OperationType.Get);

            var response = await metrics.HandleAsync(client.GetItemAsync(request));

            metrics.ReadCapacityUnits = response.ConsumedCapacity.ReadCapacityUnits;
            metrics.WriteCapacityUnits = response.ConsumedCapacity.WriteCapacityUnits;
        }

        public void GetEntitiesInClient(Guid clientId)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                ReturnConsumedCapacity = "TOTAL",
                Key = new Dictionary<string, AttributeValue>
                {
                    [nameof(Entity.ClientId)] = new AttributeValue(clientId.ToString()),
                },
                
            };

            var response = client.GetItemAsync(request).GetAwaiter().GetResult();

            WriteMetrics($"Getting entities for client returned {response} took {response.ConsumedCapacity.CapacityUnits} units.");
        }

        public void Scan()
        {
            var table = client.DescribeTableAsync(new DescribeTableRequest(tableName)).GetAwaiter().GetResult();
            WriteMetrics($"There are {table.Table.ItemCount} entities in the db");

            var request = new ScanRequest
            {
                TableName = tableName,
                ReturnConsumedCapacity = "TOTAL",
                Limit = 10000
            };
            var start = Stopwatch.GetTimestamp();

            var response = client.ScanAsync(request).GetAwaiter().GetResult();

            var time = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
            WriteMetrics($"Scanning took {response.ConsumedCapacity.CapacityUnits} units, {time} ms, and returned {response.Count} elements.");
            
            request = new ScanRequest
            {
                TableName = tableName,
                ReturnConsumedCapacity = "TOTAL",
                Limit = 100,
                ExclusiveStartKey = response.LastEvaluatedKey
            };
            start = Stopwatch.GetTimestamp();

            response = client.ScanAsync(request).GetAwaiter().GetResult();

            time = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
            WriteMetrics($"Scanning took {response.ConsumedCapacity.CapacityUnits} units, {time} ms, and returned {response.Count} elements.");
        }

        readonly object lockObj = new object();
        void WriteMetrics(string metrics)
        {
            lock (lockObj)
            {
                File.AppendAllText(@"C:\Users\Thomas.Oliver\Documents\Test\Untitled-1.txt",
                    metrics + Environment.NewLine);
            }
        }
    }
}
