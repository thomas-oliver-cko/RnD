using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.Dynamo.v1;
using Rnd.Core.ConsoleApp.Dynamo.v3;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.Configuration;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.DataObject;
using DynamoDbClientFactory = Rnd.Core.ConsoleApp.Dynamo.v1.DynamoDbClientFactory;
using DynamoDbService = Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.DynamoDbService;

namespace Rnd.Core.ConsoleApp
{
    class Program
    {
        const string Path = @"C:\Users\Thomas.Oliver\Documents\Test\Untitled-1.json";
        public const string TableName = "Marketplace-Entities";

        static readonly object lockObj = new object();

        static void Main(string[] args)
        {
            File.WriteAllText(Path, "");

            Execute().GetAwaiter().GetResult();
        }

        public class Client
        {
            public string ClientId { get; set; }
            public string EntityId { get; set; }
        }

        static async Task Execute()
        {
            var settings = new DynamoDbSettings
            {
                TableName = TableName,
                LocalUrl = new Uri("http://localhost:8000")
            };

            using var client = DynamoDbClientFactory.Create(settings);
            await new DynamoDbTableCreator(client, TableName).CreateTableAsync(true);

            var mapper = new Mapper(new MapperConfiguration(ctx => { ctx.AddProfile(new MappingProfile()); }));
            var dynamoClient = new DynamoDbClient(client, new DependencyCoordinator());
            var repo = new DynamoDbService(dynamoClient, mapper, settings);

            try
            {
                var entities = CreateEntities(100);
                foreach (var entity in entities)
                    await repo.CreateEntityAsync(entity);

                var test = await repo.ReadEntitiesByClientAsync(entities[0].ClientId);

                var table = (await client.DescribeTableAsync(new DescribeTableRequest(TableName))).Table;
                var update = new UpdateTableRequest
                {
                    TableName = table.TableName,
                    GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                    {
                        new GlobalSecondaryIndexUpdate
                        {
                            Create = new CreateGlobalSecondaryIndexAction
                            {
                                IndexName = "Test Index",
                                ProvisionedThroughput = new ProvisionedThroughput(5,5),
                                KeySchema = new List<KeySchemaElement>
                                {
                                    new KeySchemaElement("EntityId", KeyType.HASH),
                                    new KeySchemaElement("ClientId", KeyType.RANGE)
                                },
                                Projection = new Projection(){ProjectionType = ProjectionType.ALL}
                            },
                        }
                    }
                };
            }
            catch
            {
                var table = await client.DescribeTableAsync(new DescribeTableRequest(TableName));
                Console.WriteLine(table.Table.ItemCount);
            }
        }

        static List<Entity> CreateEntities(int count, bool save = false)
        {
            var generator = new Generator();
            var entities = generator.CreateEntities(1, count, 2);

            if (!save)
                return entities;

            lock (lockObj)
            {
                using var file = File.OpenWrite(Path);
                using var writer = new StreamWriter(file);

                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };

                //serialize object directly into file stream
                serializer.Serialize(writer, entities);
            }

            return entities;
        }

        static List<Entity> ReadEntities()
        {
            using (var reader = new StreamReader(@"C:\Users\Thomas.Oliver\Documents\Test\Untitled-1.json"))
            {
                var jsonReader = new JsonTextReader(reader);
                var serialiser = new JsonSerializer();
                return serialiser.Deserialize<List<Entity>>(jsonReader);
            }
        }
    }
}