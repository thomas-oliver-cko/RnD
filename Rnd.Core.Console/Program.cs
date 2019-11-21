using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.DynamoDb1;

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

        static async Task Execute()
        {
            var entities = CreateAndSaveEntities(1000);

            using var client = new DynamoDbClientFactory().Get();
            var config = new DynamoDBOperationConfig()
            {
                OverrideTableName = TableName
            };

            using var context = new DynamoDBContext(client, config);

            await new DynamoDbTableCreator(client, TableName).CreateTableAsync(true);
            var table = await client.DescribeTableAsync(new DescribeTableRequest(TableName));
            Console.WriteLine(table.Table.ItemCount);

            await context.SaveAsync(entities[0], config);

            try
            {
                var service1 = new DynamoDbLoader(client, context, TableName);
                await service1.LoadEntitiesAsync(entities);
                table = await client.DescribeTableAsync(new DescribeTableRequest(TableName));
                Console.WriteLine(table.Table.ItemCount);
                await service1.LoadEntitiesAsync(entities);
                table = await client.DescribeTableAsync(new DescribeTableRequest(TableName));
                Console.WriteLine(table.Table.ItemCount);


                var x = await client.GetItemAsync(null, null, true, CancellationToken.None);
            }
            catch 
            {
                table = await client.DescribeTableAsync(new DescribeTableRequest(TableName));
                Console.WriteLine(table.Table.ItemCount);
            }
        }

        static List<Entity> CreateAndSaveEntities(int count)
        {
            var generator = new Generator();
            var entities = generator.CreateEntities(1, count, 2);

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