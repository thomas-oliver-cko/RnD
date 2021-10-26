using Amazon.AppConfig;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Configuration.AppConfig
{
    class RetryableConfigurationProvider : AppConfigConfigurationProvider
    {
        readonly AppConfigOptions _options;

        public RetryableConfigurationProvider(IAmazonAppConfig client, IConfigReader reader, AppConfigOptions options)
            : base(client, reader, options)
        {
            _options = options;
        }

        public override async Task RefreshAsync(bool canReload, CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();

            for (var i = 0; i < _options.Retries; i++)
            {
                try
                {
                    await base.RefreshAsync(canReload, cancellationToken);
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    Console.WriteLine(e.Message);
                }

                await Task.Delay(_options.TimeBetweenRetries, cancellationToken);
            }

            if (_options.ThrowErrors && exceptions.Count > 0)
                throw new AwsAppConfigLoadException($"Could not read secrets. Retry attempts: {_options.Retries}.", exceptions);
        }
    }
}
