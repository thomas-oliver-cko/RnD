using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Checkout.Configuration.Core
{
    /// <summary>
    ///     Defines a configuration source that is refreshable
    /// </summary>
    public interface IRefreshableConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        ///    Asks the configuration source to check for the latest changes and refresh if needed
        /// </summary>
        /// <param name="canReload">Whether or not the configuration should be reloaded (typically false on startup)</param>
        /// <param name="cancellationToken">The cancellation token that will abort the refresh</param>
        Task RefreshAsync(bool canReload = true, CancellationToken cancellationToken = default);
    }
}
