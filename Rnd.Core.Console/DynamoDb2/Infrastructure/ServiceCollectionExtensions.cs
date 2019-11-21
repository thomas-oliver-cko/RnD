using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Rnd.Core.ConsoleApp.DynamoDb2.Client;
using Rnd.Core.ConsoleApp.DynamoDb2.Client.Strategies;

namespace Rnd.Core.ConsoleApp.DynamoDb2.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServicesDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IDynamoDbClientStrategy, DynamoDbDockerClientStrategy>();
            services.AddSingleton<IDynamoDbClientStrategy, DynamoDbAwsClientStrategy>();
            services.AddSingleton<IDynamoDbClientStrategy, DynamoDbLocalStackClientStrategy>();

            services.AddSingleton<IDynamoDbClientFactory, DynamoDbClientFactory>();

            services.AddSingleton(ctx =>
            {
                var factory = ctx.GetRequiredService<IDynamoDbClientFactory>();
                var settings = ctx.GetRequiredService<DynamoDbSettings>();
                return factory.Create(settings);
            });

            services.AddSingleton<IDynamoDBContext>(ctx =>
            {
                var client = ctx.GetRequiredService<IAmazonDynamoDB>();
                return new DynamoDBContext(client);
            });

            services.AddSingleton<IDynamoDbService, DynamoDbService>();
        }
    }
}
