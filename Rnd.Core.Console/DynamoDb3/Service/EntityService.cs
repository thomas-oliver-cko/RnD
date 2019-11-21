using System;
using System.Threading;
using System.Threading.Tasks;
using Rnd.Core.ConsoleApp.DataGenerator;
using Rnd.Core.ConsoleApp.DynamoDb3.Database.Contracts;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Service
{
    public class EntityService
    {
        readonly IEntityCommandRepository repository;

        public EntityService(IEntityCommandRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task CreateAsync(Entity entity, CancellationToken token = default)
        {
            return repository.SaveAsync(entity, token);
        }
    }
}
