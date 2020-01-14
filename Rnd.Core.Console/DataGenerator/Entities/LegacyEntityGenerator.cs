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
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities
{
    static class LegacyEntityGenerator
    {
        static void Generate()
        {
            var gen = new EntityGenerator();
            var entities = gen.GenerateWithInverseExponentialBinarySequence(1000, 1000000, 5);

            const string file = @"C:\Code\Marketplace\checkout-entities-api\performance\data\data.csv";
            File.WriteAllText(file, "");
            using var sWriter = new StreamWriter(file);
            using var writer = new CsvWriter(sWriter);
            writer.Configuration.HasHeaderRecord = false;

            var client = new AmazonDynamoDBClient(
                new BasicAWSCredentials("XX", "XX"),
                new AmazonDynamoDBConfig
                {
                    AuthenticationRegion = "eu-west-1",
                    ServiceURL = "http://localhost:8000",
                    EndpointDiscoveryEnabled = false
                });

            const string idFile = @"C:\Code\Marketplace\checkout-entities-api\performance\data\ids.csv";
            File.WriteAllText(idFile, "");
            using var idSWriter = new StreamWriter(idFile);
            using var idWriter = new CsvWriter(idSWriter);
            idWriter.Configuration.RegisterClassMap<EntityIdMap>();

            var data = new Entity[25];
            var i = 0;
            var tasks = new List<Task>(10);
            var entitiesToRetry = new List<Dictionary<string, List<WriteRequest>>>();
            foreach (var entity in entities)
            {
                Write(entity, idWriter);
                Write(entity, writer);

                data[i] = entity;
                i++;
                if (i != 25) continue;

                i = 0;

                tasks.Add(Task.Run(() => SendToDynamo(data, client, entitiesToRetry)));
                if (tasks.Count != 10) continue;

                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
            }

            using var toRetry = File.CreateText(@"C:\Code\Marketplace\checkout-entities-api\performance\data\ToRetry.csv");
            var serializer = new JsonSerializer();
            serializer.Serialize(toRetry, entitiesToRetry);
        }

        static void Write(Entity entity, IWriter writer)
        {
            writer.WriteRecord(entity);
            writer.NextRecord();
        }

        static async Task SendToDynamo(IEnumerable<Entity> entities, IAmazonDynamoDB client, ICollection<Dictionary<string, List<WriteRequest>>> entitiesToRetry)
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

    sealed class EntityIdMap : ClassMap<Entity>
    {
        public EntityIdMap()
        {
            Map(m => m.EntityId);
        }
    }
}