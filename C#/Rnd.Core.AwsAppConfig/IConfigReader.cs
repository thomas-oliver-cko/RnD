using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Configuration.AppConfig
{
    interface IConfigReader
    {
        Task<IReadOnlyDictionary<string, (string value, DateTime lastUpdated)>> Read(Stream stream,
            CancellationToken token = default);
    }
}
