using Amazon.AppConfig;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime.CredentialManagement;
using Checkout.Configuration.AppConfig;
using Microsoft.Extensions.Configuration;
using System;

namespace Rnd.Core.AwsAppConfig
{
    class AppConfigConfigurationSource : IConfigurationSource
    {
        readonly AppConfigOptions _options;
        readonly IAmazonAppConfig _client;

        public AppConfigConfigurationSource(AppConfigOptions options, AWSOptions awsOptions)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            options.ValidateOrThrow();

            var creds = new SharedCredentialsFile();
            creds.TryGetProfile("default", out var profile);
            awsOptions.Credentials = profile.GetAWSCredentials(creds);

            _client = awsOptions.CreateServiceClient<IAmazonAppConfig>();
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RetryableConfigurationProvider(_client, new JsonConfigReader(), _options);
        }
    }
}
