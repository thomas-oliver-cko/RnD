using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkout.Configuration.AppConfig
{
    /// <summary>
    /// Options for configuration the AWS AppConfig client
    /// </summary>
    public class AppConfigOptions
    {
        /// <summary>
        /// The application being configured, as named in AWS AppConfig
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// A unique, user-specified ID to identify the client. This enables deployment strategies, where config
        /// is rolled out to applications in intervals
        /// </summary>
        public string ClientId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The environment is a group of configuration targets, such as QA or Production, as set in AWS AppConfig
        /// </summary>
        public string Environment { get; set; }


        /// <summary>
        /// The configuration profile to be used, as set in AWS AppConfig
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Number of retries for getting the secret. This is on top of retries performed by the AWS clients
        /// </summary>
        public int Retries { get; set; } = 10;

        /// <summary>
        /// The number of milliseconds between each retry
        /// </summary>
        public int TimeBetweenRetries { get; set; } = 1000;

        /// <summary>
        /// If true, throw Exception after number of Retries attempts to get the secret.
        /// If false, continue silently after number of Retries
        /// </summary>
        public bool ThrowErrors { get; set; } = true;

        internal void ValidateOrThrow()
        {
            var reasons = new List<string>();

            if (string.IsNullOrEmpty(Application))
                reasons.Add($"{nameof(Application)} cannot be empty");

            if (string.IsNullOrEmpty(Environment))
                reasons.Add($"{nameof(Environment)} cannot be empty");

            if (string.IsNullOrEmpty(Configuration))
                reasons.Add($"{nameof(Configuration)} cannot be empty");

            if (Retries < 0)
                reasons.Add($"{nameof(Retries)} must be greater than or equal to 0");

            if (TimeBetweenRetries < 0)
                reasons.Add($"{nameof(TimeBetweenRetries)} must be greater than or equal to 0");

            if (reasons.Any())
                throw new InvalidOperationException(string.Join(", ", reasons));
        }
    }
}
