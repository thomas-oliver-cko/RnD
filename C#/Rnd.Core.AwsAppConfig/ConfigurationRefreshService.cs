using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Checkout.Configuration.Core
{
    internal sealed class ConfigurationRefreshService : BackgroundService
    {
        internal static readonly List<IRefreshableConfigurationProvider> KnownConfigurationSources = new List<IRefreshableConfigurationProvider>();

        private readonly IOptionsMonitor<ConfigurationOptions> _configurationOptions;
        private readonly ILogger<ConfigurationRefreshService> _logger;

        public ConfigurationRefreshService(IOptionsMonitor<ConfigurationOptions> configurationOptions, ILogger<ConfigurationRefreshService> logger)
        {
            _configurationOptions = configurationOptions;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<IRefreshableConfigurationProvider> sources;

                    lock (KnownConfigurationSources)
                        sources = KnownConfigurationSources.ToList();

                    await Task.WhenAll(sources.Select(async x =>
                    {
                        try
                        {
                            await x.RefreshAsync(true, stoppingToken);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }));
                }
                catch (Exception e)
                {
                    if (!stoppingToken.IsCancellationRequested)
                        _logger?.LogError(e, e.Message);
                }
                finally
                {
                    try
                    {
                        await Task.Delay(_configurationOptions.CurrentValue.RefreshInterval, stoppingToken);
                    }
                    catch (OperationCanceledException) { }
                }
            }
        }

        internal static void Register(IRefreshableConfigurationProvider configurationProvider)
        {
            lock(KnownConfigurationSources)
                KnownConfigurationSources.Add(configurationProvider);
        }

        public static void Deregister(IRefreshableConfigurationProvider configurationProvider)
        {
            lock (KnownConfigurationSources)
                KnownConfigurationSources.Remove(configurationProvider);
        }
    }
}
