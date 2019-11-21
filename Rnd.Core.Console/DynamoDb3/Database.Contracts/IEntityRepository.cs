using System.Threading;
using System.Threading.Tasks;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.DynamoDb3.Database.Contracts
{
    public interface IEntityCommandRepository
    {
        Task SaveAsync(Entity entity, CancellationToken token = default);
    }

    public interface IEntityQueryRepository
    {
        Task<Entity> ReadAsync(string id, CancellationToken token = default);
    }
}
