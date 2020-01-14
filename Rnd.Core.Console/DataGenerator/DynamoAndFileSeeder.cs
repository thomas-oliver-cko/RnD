using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using CsvHelper;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.DataGenerator
{
    abstract class DynamoAndFileSeeder<T>
    {
        const int BufferSize = 25;
        protected abstract IAmazonDynamoDB Client { get; }
        protected abstract IWriter Writer { get; }

        protected abstract IEnumerable<T> GetData();

        public async Task SeedData()
        {
            var buffer = new T[BufferSize];
            var bufferIndex = 0;
            var tasks = new List<Task>(BufferSize);
            var entitiesToRetry = new List<Dictionary<string, List<WriteRequest>>>();

            foreach (var element in GetData())
            {
                Writer.WriteRecord(element);
                var next = Writer.NextRecordAsync();

                buffer[bufferIndex] = element;
                bufferIndex++;

                await next;

                if (bufferIndex != BufferSize) continue;
                
                tasks.Add(SendToDynamoAsync(buffer, entitiesToRetry));
                bufferIndex = 0;

                if (tasks.Count != BufferSize) continue;

                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        protected async Task SendToDynamoAsync(IEnumerable<T> entities, ICollection<Dictionary<string, List<WriteRequest>>> entitiesToRetry)
        {
            var result = await Client.BatchWriteItemAsync(new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {
                        "Entities", entities.Select(t => new WriteRequest
                        {
                            PutRequest = new PutRequest(Document.FromJson(JsonConvert.SerializeObject(t))
                                .ToAttributeMap())
                        }).ToList()
                    }
                }
            });

            entitiesToRetry.Add(result.UnprocessedItems);

            if (result.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception();
        }
    }
}
