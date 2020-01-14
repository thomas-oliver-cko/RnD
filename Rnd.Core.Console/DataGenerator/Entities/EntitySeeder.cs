using System;
using System.Collections.Generic;
using System.IO;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using CsvHelper;
using CsvHelper.Configuration;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities
{
    class EntitySeeder : DynamoAndFileSeeder<Entity>, IDisposable
    {
        readonly EntityGenerator generator;

        public EntitySeeder(string targetFile)
        {
            generator = new EntityGenerator();
            Client = new AmazonDynamoDBClient(new BasicAWSCredentials("XX", "XX"),
                new AmazonDynamoDBConfig
                {
                    AuthenticationRegion = "eu-west-1",
                    ServiceURL = "http://localhost:8000",
                    EndpointDiscoveryEnabled = false
                });


            File.WriteAllText(targetFile, "");
            var sWriter = new StreamWriter(targetFile);
            Writer = new CsvWriter(sWriter);
            Writer.Configuration.HasHeaderRecord = false;
        }

        protected sealed override IAmazonDynamoDB Client { get; }
        protected sealed override IWriter Writer { get; }

        protected override IEnumerable<Entity> GetData() =>
            generator.GenerateWithInverseExponentialBinarySequence(1000, 1000000, 5);

        public void Dispose()
        {
            Client.Dispose();
            Writer.Dispose();
        }
    }
}
