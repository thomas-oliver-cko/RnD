using Amazon.AppConfig;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Checkout.Configuration.AppConfig;
using Checkout.Configuration.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Rnd.Core.AwsAppConfig
{
    /// <summary>
    ///     Extension methods to install AWS AppConfig into IConfigurationBuilder
    /// </summary>
    public static class InstallerExtensions
    {
        /// <summary>
        ///     Standard configuration for secrets manager with the specified containers
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        /// <param name="application">The application being configured, as named in AWS AppConfig</param>
        /// <param name="environment">A group of configuration targets, such as QA or Production</param>
        /// <param name="includeRefreshService">Whether to include the refresh service. Default: true</param>
        /// <param name="configEditor">Settings for the AWS AppConfig client</param>
        public static IHostBuilder ConfigureUsingAwsAppConfig(this IHostBuilder hostBuilder,
            string application,
            string environment,
            Action<IConfiguration, AWSOptions>? configEditor = null,
            bool includeRefreshService = true)
        {
            return hostBuilder.ConfigureUsingAwsAppConfig((c, o, a) =>
            {
                o.Application = application;
                o.Environment = environment;

                configEditor?.Invoke(c, a);
            }, includeRefreshService);
        }

        /// <summary>
        ///     Standard configuration for secrets manager with the specified containers
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        public static IHostBuilder ConfigureUsingAwsAppConfig(this IHostBuilder hostBuilder,
            Action<AppConfigOptions, AWSOptions> optionsEditor,
            bool includeRefreshService = true)
        {
            return hostBuilder.ConfigureUsingAwsAppConfig(
                (_, o, a) => optionsEditor?.Invoke(o, a),
                includeRefreshService);
        }

        /// <summary>
        ///     Standard configuration for secrets manager with the specified containers
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        /// <param name="includeRefreshService">Whether to include the refresh service. Default: true</param>
        /// <param name="configEditor">Settings for the AWS AppConfig client</param>
        public static IHostBuilder ConfigureUsingAwsAppConfig(this IHostBuilder hostBuilder,
            Action<IConfiguration, AppConfigOptions, AWSOptions>? configEditor = null,
            bool includeRefreshService = true)
        {
            if (hostBuilder is null)
                throw new ArgumentNullException(nameof(hostBuilder));

            hostBuilder.ConfigureAppConfiguration((context, builder) => 
                builder.AddConfigurationFromAwsAppConfig(configEditor));

            if (includeRefreshService)
                hostBuilder.ConfigureServices((context, services) => services.AddRefreshService());

            return hostBuilder;
        }
    }

    /// <summary></summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     Extension Method to add config from AWS Secrets Manager to the IConfigurationBuilder
        /// </summary>
        /// <param name="builder">The builder to add the secrets to</param>
        /// <param name="application">The application being configured, as named in AWS AppConfig</param>
        /// <param name="environment">A group of configuration targets, such as QA or Production</param>
        /// <param name="throwErrors">Indicates if exceptions should be thrown</param>
        /// <param name="retries">Number of times to retry in case of an Exception</param>
        /// <param name="configEditor">Edit the AWS Config object prior to usage</param>
        public static IConfigurationBuilder AddConfigurationFromAwsAppConfig(this IConfigurationBuilder builder,
            string application,
            string environment,
            bool throwErrors = false,
            int retries = 10,
            Action<AWSOptions>? configEditor = null)
        {
            return builder.AddConfigurationFromAwsAppConfig((_, o, a) =>
            {
                o.Application = application;
                o.Environment = environment;
                o.ThrowErrors = throwErrors;
                o.Retries = retries;

                configEditor?.Invoke(a);
            });
        }

        /// <summary>
        ///     Extension Method to add config from AWS Secrets Manager to the IConfigurationBuilder
        /// </summary>
        /// <param name="builder">The builder to add the secrets to</param>
        /// <param name="configEditor">Edit the AWS Config object prior to usage</param>
        public static IConfigurationBuilder AddConfigurationFromAwsAppConfig(this IConfigurationBuilder builder,
            Action<AppConfigOptions, AWSOptions> configEditor) =>
            builder.AddConfigurationFromAwsAppConfig((_, o, a) => configEditor(o, a));

        /// <summary>
        ///     Extension Method to add config from AWS Secrets Manager to the IConfigurationBuilder
        /// </summary>
        /// <param name="builder">The builder to add the secrets to</param>
        /// <param name="configEditor">Edit the AWS Config object prior to usage</param>
        public static IConfigurationBuilder AddConfigurationFromAwsAppConfig(this IConfigurationBuilder builder,
            Action<IConfiguration, AppConfigOptions, AWSOptions>? configEditor = null)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var appConfig = builder.Build();
            var options = new AppConfigOptions();
            var awsOptions = appConfig.GetAWSOptions("AppConfig");

            appConfig.Bind("AppConfig", options);
            configEditor?.Invoke(appConfig, options, awsOptions);

            return builder.AddConfigurationFromAwsAppConfig(options, awsOptions);
        }

        /// <summary>
        ///     Extension Method to add config from AWS Secrets Manager to the IConfigurationBuilder
        /// </summary>
        /// <param name="builder">The builder to add the secrets to</param>
        /// <param name="options">Options for how</param>
        public static IConfigurationBuilder AddConfigurationFromAwsAppConfig(this IConfigurationBuilder builder,
            AppConfigOptions options,
            AWSOptions awsOptions)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (options is null) throw new ArgumentNullException(nameof(options));

            options.ValidateOrThrow();

            return builder.Add(new AppConfigConfigurationSource(options, awsOptions));
        }
    }
}
