using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v1
{
    public class DynamoDbTableCreator
    {
        readonly IAmazonDynamoDB client;

        public DynamoDbTableCreator(IAmazonDynamoDB client, string tableName)
        {
            this.client = client;
            TableRequest = GetEntityPkRequest(tableName);
        }

        public static CreateTableRequest TableRequest { get; set; }

        public Task CreateTableAsync(bool resetTable = false, CancellationToken token = default)
            => CreateTableAsync(TableRequest, resetTable, token);

        public async Task CreateTableAsync(CreateTableRequest request, bool resetTable = false, CancellationToken token = default)
        {
            try
            {
                await client.DescribeTableAsync(new DescribeTableRequest(request.TableName), token);

                if (!resetTable)
                    return;

                await client.DeleteTableAsync(new DeleteTableRequest(request.TableName), token);
            }
            catch
            {
                // If table doesn't exist, exception is thrown
            }

            await client.CreateTableAsync(request, token);
        }

        public static CreateTableRequest GetEntityPkRequest(string tableName)
        {
            var schema = new List<KeySchemaElement>
            {
                new KeySchemaElement(nameof(Entity.EntityId), KeyType.HASH),
                //new KeySchemaElement("VersionId", KeyType.RANGE)
            };

            var globalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "ClientId-EntityId-Index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement(nameof(Entity.ClientId), KeyType.HASH),
                        new KeySchemaElement(nameof(Entity.EntityId), KeyType.RANGE),
                    },
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL,
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                },
            };

            var attributes = new List<AttributeDefinition>
            {
                new AttributeDefinition(nameof(Entity.ClientId), ScalarAttributeType.S),
                new AttributeDefinition(nameof(Entity.EntityId), ScalarAttributeType.S)
            };

            var tags = new List<Tag>
            {
                new Tag {Key = "Product", Value = "Marketplace"},
                new Tag {Key = "Application", Value = "Marketplace-Entities-Api"},
                new Tag {Key = "Department", Value = "Engineering"},
            };

            return new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = attributes,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                GlobalSecondaryIndexes = globalSecondaryIndexes,
                KeySchema = schema,
                ProvisionedThroughput = new ProvisionedThroughput(5, 5),
                Tags = tags,
            };
        }
    }
}
