using System.Threading.Tasks;
using Rnd.Core.Aspnet.Domain;

namespace Rnd.Core.Aspnet.Repo
{
    public interface IBusinessRepository
    {
        Task SaveAsync(BusinessModel model);
    }

    public class BusinessRepository : IBusinessRepository
    {
        public async Task SaveAsync(BusinessModel model)
        {
            await Task.CompletedTask;
        }
    }
}
