using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Checkout.Configuration.Core;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Configuration.AppConfig
{
    class AppConfigConfigurationProvider : RefreshableConfigurationProvider
    {
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        readonly IAmazonAppConfig _client;
        readonly IConfigReader _reader;
        readonly GetConfigurationRequest _configRequest;

        public AppConfigConfigurationProvider(IAmazonAppConfig client, IConfigReader reader, AppConfigOptions options)
        {
            _client = client;
            _reader = reader;

            _configRequest = new GetConfigurationRequest
            {
                Application = options.Application,
                ClientId = options.ClientId,
                Environment = options.Environment,
                Configuration = options.Configuration
            };
        }

        public override async Task RefreshAsync(bool canReload, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);

            try
            {
                var response = await _client.GetConfigurationAsync(_configRequest, token);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new HttpRequestException($"Request to AWS AppConfig failed with status {response.HttpStatusCode}");

                if (response.ConfigurationVersion == _configRequest.ClientConfigurationVersion || response.Content.Length == 0)
                    return;

                var config = await _reader.Read(response.Content, token);

                Set(config, canReload);

                _configRequest.ClientConfigurationVersion = response.ConfigurationVersion;
            }
            finally 
            {
                _semaphore.Release();
            }
        }
    }
}
