using System;

namespace Checkout.Configuration.Core
{
    /// <summary>
    ///     General configuration for configuration
    /// </summary>
    public class ConfigurationOptions
    {
        /// <summary>
        ///     The interval between checking for changes
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(15);
    }
}
