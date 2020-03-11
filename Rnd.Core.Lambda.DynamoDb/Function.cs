using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using CsvHelper;
using Newtonsoft.Json;
using Rnd.Core.Lambda.DynamoDb.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Rnd.Core.Lambda.DynamoDb
{
    public class Function
    {
        readonly IAmazonS3 s3Client;
        readonly IAmazonDynamoDB dynamoClient;

        public Function() : this(new AmazonS3Client(RegionEndpoint.EUWest1),
            new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
        {
        }

        public Function(IAmazonS3 s3Client, IAmazonDynamoDB dynamoClient)
        {
            this.dynamoClient = dynamoClient ?? new AmazonDynamoDBClient();
            this.s3Client = s3Client ?? new AmazonS3Client();
        }

        public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if(s3Event == null)
                return;
            
            try
            {
                var response = await s3Client.GetObjectStreamAsync(s3Event.Bucket.Name, s3Event.Object.Key, null);

                // Read csv
                var reader = new CsvReader(new StreamReader(response), CultureInfo.CurrentCulture);
                reader.Configuration.Delimiter = "|";
                reader.ReadHeader();

                var dynamoBatch = Task.CompletedTask;
                var entities = new List<Entity>(25);
                foreach (var csvEntity in reader.GetRecords<CsvEntity>())
                {
                    entities.Add(csvEntity.ToEntity());

                    if (entities.Count < 25)
                        continue;

                    var requests = entities.Select(e =>
                    {
                        var json = JsonConvert.SerializeObject(e);
                        return new WriteRequest
                        {
                            PutRequest = new PutRequest(Document.FromJson(json).ToAttributeMap())
                        };
                    }).ToList();

                    var batch = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>>
                    {
                        ["mp-dyndb-entities-qa"] = requests
                    });

                    await dynamoBatch;
                    dynamoBatch = dynamoClient.BatchWriteItemAsync(batch);
                }
            }
            catch(Exception e)
            {
                context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. " +
                                       "Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
