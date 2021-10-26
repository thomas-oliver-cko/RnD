using System;
using System.Collections.Generic;

namespace Checkout.Configuration.AppConfig
{
    /// <summary>
    /// Thrown when the service fails to acquire secrets from AWS App Config.
    /// </summary>
    [Serializable]
    public class AwsAppConfigLoadException : AggregateException
    {
        /// <inheritdoc />
        public AwsAppConfigLoadException()
        { }

        /// <inheritdoc />
        public AwsAppConfigLoadException(string message) : base(message)
        { }

        /// <inheritdoc />
        public AwsAppConfigLoadException(string message, Exception inner) : base(message, inner)
        { }

        /// <inheritdoc />
        public AwsAppConfigLoadException(string message, List<Exception> innerExceptions) : base(message, innerExceptions)
        { }
    }
}
