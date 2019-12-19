using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.Configuration;

namespace Rnd.Core.ConsoleApp.Dynamo.v3.Database.Dynamo.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDatabaseDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IDynamoDBContext>(ctx =>
            {
                var settings = ctx.GetRequiredService<DynamoDbSettings>();
                var dynamoDbClient = DynamoDbClientFactory.Create(settings);

                return new DynamoDBContext(dynamoDbClient,
                    new DynamoDBOperationConfig {OverrideTableName = settings.TableName});
            });

        }
    }
}
