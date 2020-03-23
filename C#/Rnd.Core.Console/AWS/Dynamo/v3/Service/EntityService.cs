using System;
using System.Threading;
using System.Threading.Tasks;
using Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Contracts;
using Rnd.Core.ConsoleApp.DataGenerator.Entities;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Service
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
