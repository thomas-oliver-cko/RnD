using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Rnd.Core.Lambda.DynamoDb
{
    public class Function : IDisposable
    {
        readonly JsonSerializer serializer = new JsonSerializer();
        readonly List<JObject> objects = new List<JObject>(25);
        readonly string tableName = Environment.GetEnvironmentVariable("TableName");
        readonly IAmazonS3 s3Client;
        readonly IAmazonDynamoDB dynamoClient;

        public Function() 
        {
            s3Client = new AmazonS3Client(new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.EUWest1,
                EndpointDiscoveryEnabled = false
            });
            dynamoClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.EUWest1,
                EndpointDiscoveryEnabled = false
            });
        }

        public Function(IAmazonS3 s3Client, IAmazonDynamoDB dynamoClient)
        {
            this.dynamoClient = dynamoClient ?? new AmazonDynamoDBClient();
            this.s3Client = s3Client ?? new AmazonS3Client();
        }

        public async Task FunctionHandler(S3Event evnt, ILambdaContext? context)
        {
            if(evnt?.Records == null)
                return;
            
            try
            {
                foreach (var s3Event in evnt.Records.Select(record => record.S3))
                {
                    context?.Logger.LogLine($"Received event from S3, reading {s3Event.Object.Key} from {s3Event.Bucket.Name}");
                    using var response = await s3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key); 
                    using var sr = new StreamReader(response.ResponseStream);
                    using JsonReader reader = new JsonTextReader(sr);
                    var task = Task.CompletedTask;

                    context?.Logger.LogLine("Accessed object, deserialising and sending to dynamo db");
                    while (reader.Read())
                    {
                        if (reader.TokenType != JsonToken.StartObject) continue;
                        objects.Add(serializer.Deserialize<JObject>(reader));

                        if (objects.Count < 25) continue;

                        await task;
                        task = PostAsync(objects, context);

                        objects.Clear();
                    }
                }
            }
            catch(Exception e)
            {
                context?.Logger.LogLine(e.Message);
                context?.Logger.LogLine(e.StackTrace);
                throw;
            }
        }

        async Task PostAsync(IEnumerable<JObject> values, ILambdaContext? context)
        {
            context?.Logger.LogLine("Sending batch to dynamo");

            var requests = values.Select(e => new WriteRequest
            {
                PutRequest = new PutRequest(Document.FromJson(e.ToString(Formatting.None)).ToAttributeMap())
            });

            var batch = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>>
            {
                [tableName] = requests.ToList()
            });

            var result = await dynamoClient.BatchWriteItemAsync(batch);
            if (result.HttpStatusCode != HttpStatusCode.OK)
            {
                var metadata = JsonConvert.SerializeObject(result.ResponseMetadata.Metadata, new JsonSerializerSettings{MaxDepth = 3});
                context?.Logger.LogLine($"Dynamo Call failed with {result.HttpStatusCode} {metadata}");
                context?.Logger.LogLine($"Data: {string.Join(Environment.NewLine, values.Select(s => s.ToString()))}");
                throw new Exception(metadata);
            }
        }

        public void Dispose()
        {
            s3Client.Dispose();
            dynamoClient.Dispose();
        }
    }
}
