using System;
using Microsoft.Extensions.DependencyInjection;

namespace Checkout.Configuration.Core
{
    /// <summary>
    /// Extension methods for the configuration service
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        ///     Adds the refresh service for the secrets manager instances
        /// </summary>
        public static IServiceCollection AddRefreshService(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.AddHostedService<ConfigurationRefreshService>();

            return services;
        }
    }
}
