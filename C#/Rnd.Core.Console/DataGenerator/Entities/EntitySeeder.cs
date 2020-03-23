using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities
{
    static class EntitySeeder
    {
        const int BufferSize = 25;

        public static async Task SeedCsvAndDynamoJson(string targetFile, string jsonFile, int clientCount,
            int totalCount)
        {
            var generator = new EntityGenerator();

            File.WriteAllText(jsonFile, "");
            await using var dWriter = new StreamWriter(jsonFile);
            using var dynamoWriter = new JsonTextWriter(dWriter);

            File.WriteAllText(targetFile, "");
            await using var sWriter = new StreamWriter(targetFile);
            using var writer = new CsvWriter(sWriter, CultureInfo.CurrentCulture);
            writer.Configuration.Delimiter = "|";
            writer.WriteHeader<Entity>();

            dynamoWriter.WriteStartArray();
            foreach (var element in generator.GenerateWithInverseExponentialBinarySequence(clientCount, 5,
                totalCount + clientCount))
            {
                writer.WriteRecord(element);
                var next = writer.NextRecordAsync();

                element.Write(dynamoWriter);
                await next;
            }

            dynamoWriter.WriteEndArray();
            dynamoWriter.Flush();
        }

        public static async Task SeedEntities()
        {
            var generator = new EntityGenerator();

            const string readEntity = @"C:\Temp\checkout-entities-api\data1\ReadEntityById.csv";
            const string readClient = @"C:\Temp\checkout-entities-api\data1\ReadEntitiesByClient.csv";
            const string create = @"C:\Temp\checkout-entities-api\data1\CreateEntity.csv";

            File.WriteAllText(readEntity, "");
            File.WriteAllText(create, "");
            File.WriteAllText(readClient, "");

            using var entityWriter = new CsvWriter(new StreamWriter(readEntity), CultureInfo.CurrentCulture);
            entityWriter.Configuration.Delimiter = "|";
            entityWriter.WriteHeader<Entity>();
            entityWriter.NextRecord();

            using var createWriter = new CsvWriter(new StreamWriter(create), CultureInfo.CurrentCulture);
            createWriter.Configuration.Delimiter = "|";
            createWriter.Configuration.RegisterClassMap<EntityMap>();
            createWriter.WriteHeader<Entity>();
            createWriter.NextRecord();

            using var writer = new CsvWriter(new StreamWriter(readClient), CultureInfo.CurrentCulture);
            writer.Configuration.ShouldQuote = (entry, context) => false;
            writer.Configuration.Delimiter = "|";
            writer.WriteHeader<Client>();
            writer.NextRecord();

            const int totalCount = 50;
            const int clientCount = totalCount / 20;
            const int total = totalCount + clientCount;
            var rnd = new Random();
            var horizontalDist = Enumerable.Range(0, clientCount).Select(s => rnd.NextDouble()).ToArray();

            var verticalDistribution = Enumerable.Range(0, 5)
                .Select(s => Math.Pow(2, s * -1))
                .ToArray();

            var lastClient = "";
            var entitiesInClient = new List<string>();
            foreach (var element in generator.Generate(clientCount, 5, total, verticalDistribution, horizontalDist,
                false))
            {
                createWriter.WriteRecord(element);
                createWriter.NextRecord();
                entityWriter.WriteRecord(element);
                entityWriter.NextRecord();

                if (lastClient == element.ClientId)
                {
                    entitiesInClient.Add(element.EntityId);
                    continue;
                }

                if (lastClient == "")
                {
                    entitiesInClient.Add(element.EntityId);
                    lastClient = element.ClientId;
                    continue;
                }

                var client = new Client
                {
                    ClientId = lastClient,
                    EntityCount = entitiesInClient.Count,
                    Entities = string.Join(",", entitiesInClient)
                };
                writer.WriteRecord(client);
                writer.NextRecord();

                entitiesInClient.Clear();
                entitiesInClient.Add(element.EntityId);
                lastClient = element.ClientId;
            }
        }

        public static async Task WriteToDynamo(string targetFile)
        {
            var prevCount = 1381470;
            var count = 0;
            try
            {
                using var reader = new CsvReader(new StreamReader(targetFile), CultureInfo.CurrentCulture);
                reader.Configuration.Delimiter = "|";

                reader.Read();
                reader.ReadHeader();

                using var stream = new MemoryStream();
                using var writer = new JsonTextWriter(new StreamWriter(stream));

                var client = new AmazonDynamoDBClient(new StoredProfileAWSCredentials("default"), new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1
                });

                var entities = new List<object>(25);
                var entitiesToRetry = new List<Dictionary<string, List<WriteRequest>>>();
                Task sendTask = Task.Run(() => {});
                while (reader.Read())
                {
                    if (count < prevCount)
                    {
                        count++;
                        continue;
                    }

                    entities.Add(reader.GetRecord<Entity>());

                    if (entities.Count == 25)
                    {
                        await sendTask;
                        sendTask = SendToDynamoAsync(entities, client, entitiesToRetry);
                        entities.Clear();
                        count += 25;
                    }
                }

                File.WriteAllText(@"C:\Temp\checkout-entities-api\data\Failed.json", JsonConvert.SerializeObject(entitiesToRetry));
            }
            catch
            {
                File.WriteAllText(@"C:\Temp\checkout-entities-api\data\Reached.txt", count.ToString());
            }
        }

        static async Task SendToDynamoAsync(IEnumerable<object> entities, IAmazonDynamoDB client,
            ICollection<Dictionary<string, List<WriteRequest>>> entitiesToRetry)
        {
            var item = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {
                        "mp-dyndb-entities-qa", entities.Select(t => new WriteRequest
                        {
                            PutRequest = new PutRequest(Document.FromJson(JsonConvert.SerializeObject(t))
                                .ToAttributeMap())
                        }).ToList()
                    }
                }
            };
            var result = await client.BatchWriteItemAsync(item);

            entitiesToRetry.Add(result.UnprocessedItems);

            if (result.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception();
        }
    }


    sealed class EntityMap : ClassMap<Entity>
    {
        public EntityMap()
        {
            AutoMap(CultureInfo.CurrentCulture);
            Map(s => s.EntityId).Ignore();
        }
    }
}
