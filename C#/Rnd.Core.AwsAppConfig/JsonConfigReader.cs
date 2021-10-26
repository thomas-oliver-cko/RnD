using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Configuration.AppConfig
{
    class JsonConfigReader : IConfigReader
    {
        readonly ISystemClock _clock;

        public JsonConfigReader(ISystemClock? clock = null)
        {
            _clock = clock ?? new SystemClock();
        }

        public async Task<IReadOnlyDictionary<string, (string value, DateTime lastUpdated)>> Read(Stream stream,
            CancellationToken token = default)
        {
            var updateTime = _clock.UtcNow;

            var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            var dict = new Dictionary<string, string>();

            dict.ParseAndAdd(document.RootElement);

            return dict.ToDictionary(s => s.Key, s => (s.Value, updateTime));
        }
    }
}
