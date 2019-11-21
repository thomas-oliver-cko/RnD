using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using BenchmarkDotNet.Loggers;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Contracts;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.Configuration;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo.DataObject;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Dynamo
{
    class DynamoDbRepository : IEntityCommandRepository, IEntityQueryRepository
    {
        readonly IDynamoDbClient client;
        readonly IMapper mapper;
        readonly DynamoDbSettings settings;

        public DynamoDbRepository(IDynamoDbClient client, IMapper mapper, DynamoDbSettings settings)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public Task SaveAsync(Entity entity, CancellationToken token = default)
        {
            var dbEntity = mapper.Map<DynamoDbEntity>(entity);
            var attributes = dbEntity.ToAttributeValues();
            return client.SaveAsync(attributes, settings.TableName, token);
        }

        public async Task<Entity> ReadAsync(string id, CancellationToken token = default)
        {
            var result = await client.ReadAsync(id, settings.TableName, token);
            var dbEntity = DynamoDbEntity.FromAttributeValues(result);
            return mapper.Map<Entity>(dbEntity);
        }
    }
}
