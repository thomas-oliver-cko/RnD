using System.Threading;
using System.Threading.Tasks;
using Rnd.Core.ConsoleApp.DataGenerator;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Contracts
{
    public interface IEntityCommandRepository
    {
        Task SaveAsync(Entity entity, CancellationToken token = default);
    }

    public interface IEntityQueryRepository
    {
        Task<Entity> ReadAsync(string id, CancellationToken token = default);
    }

    public interface IClientQueryRepository
    {
        Task<Entity> ReadAsync(string clientId, CancellationToken token = default);
    }
}
