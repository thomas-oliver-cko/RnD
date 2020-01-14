using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using CsvHelper;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.DataGenerator
{
    class GenerateAndWriter<T>
    {
        const int bufferSize = 25;
        readonly IAmazonDynamoDB client;

        public GenerateAndWriter()
        {
            client = new AmazonDynamoDBClient(
                new BasicAWSCredentials("XX", "XX"),
                new AmazonDynamoDBConfig
                {
                    AuthenticationRegion = "eu-west-1",
                    ServiceURL = "http://localhost:8000",
                    EndpointDiscoveryEnabled = false
                });
        }

        public async Task GenerateAndWriteToCsv(string destinationFile, Func<IEnumerable<T>> getDataFunc, bool clearFile = true)
        {
            if (clearFile)
                File.WriteAllText(destinationFile, "");

            await using var sWriter = new StreamWriter(destinationFile);
            using var writer = new CsvWriter(sWriter);
            writer.Configuration.HasHeaderRecord = false;

            var buffer = new T[bufferSize];
            var bufferIndex = 0;
            var tasks = new List<Task>(bufferSize);
            var entitiesToRetry = new List<Dictionary<string, List<WriteRequest>>>();

            foreach (var element in getDataFunc())
            {
                writer.WriteRecord(element);
                var next = writer.NextRecordAsync();

                buffer[bufferIndex] = element;
                bufferIndex++;

                await next;

                if (bufferIndex != bufferSize) continue;
                
                tasks.Add(SendToDynamoAsync(buffer, entitiesToRetry));
                bufferIndex = 0;

                if (tasks.Count != bufferSize) continue;

                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        async Task SendToDynamoAsync(IEnumerable<T> entities, ICollection<Dictionary<string, List<WriteRequest>>> entitiesToRetry)
        {
            var result = await client.BatchWriteItemAsync(new BatchWriteItemRequest
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
