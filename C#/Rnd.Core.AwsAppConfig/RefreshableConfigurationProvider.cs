using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Checkout.Configuration.Core
{
    // This code is pretty yucky, but there isn't much because of the way the configuration system is built
    // So the easiest way to get a background refresh of the data is to:
    //     - Register each instance of the secrets manager provider with a static internal collection
    //     - From the background service, access this collection and request the reload internally
    //     - Load synchronously

    /// <summary>
    /// Base class for implementing a configuration provider that only triggers a refresh when the data has actually changed in the configuration source provider.
    /// </summary>
    public abstract class RefreshableConfigurationProvider : IRefreshableConfigurationProvider, IDisposable
    {
        private Dictionary<string, (string value, DateTime lastUpdated)> _data = new Dictionary<string, (string value, DateTime lastUpdated)>();
        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        /// <summary>
        /// ctor
        /// </summary>
        protected RefreshableConfigurationProvider()
        {
            ConfigurationRefreshService.Register(this);
        }

        #region IConfigurationProvider Contract
        /// <inheritDoc />
        public bool TryGet(string key, out string value)
        {
            if (_data.TryGetValue(key, out var data))
            {
                value = data.value;
                return true;
            }

            value = null!; // public contract dictates this
            return false;
        }

        /// <inheritDoc />
        public void Set(string key, string value)
        {
            // This will probably always be true but we should always check to be safe
            if (TrySetKey(_data, key, value, DateTime.UtcNow))
                OnReload();
        }

        /// <inheritDoc />
        public IChangeToken GetReloadToken() => _reloadToken;

        /// <inheritDoc />
        public void Load() => RefreshAsync(true, CancellationToken.None);

        /// <inheritDoc />
        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            return _data
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(kv => Segment(kv.Key, prefix.Length))
                .Concat(earlierKeys)
                .OrderBy(k => k, ConfigurationKeyComparer.Instance);
        }
        #endregion

        /// <inheritDoc />
        public abstract Task RefreshAsync(bool canReload, CancellationToken cancellationToken);

        /// <summary>
        ///     Sets a series of keys at once controlling whether the source can trigger a refresh
        /// </summary>
        public void Set(IEnumerable<KeyValuePair<string, (string value, DateTime lastUpdated)>> entries, bool canReload)
        {
            // We need to be careful about how we do the reloads because of running consumers etc
            // Keeping a flag allows us to only change the options when required
            var requiresReload = false;

            var dictEntries = entries.ToDictionary(x => x.Key, x => x.Value);

            foreach (var entry in dictEntries)
                requiresReload |= TrySetKey(_data, entry.Key, entry.Value.value, entry.Value.lastUpdated);

            var keysThatNeedToBeRemoved = _data.Keys.Except(dictEntries.Keys).ToList();
            if (keysThatNeedToBeRemoved.Count > 0)
            {
                requiresReload = true;
                foreach (var key in keysThatNeedToBeRemoved)
                    _data.Remove(key);
            }

            if (canReload && requiresReload)
                OnReload();
        }

        /// <summary>
        ///     Tries to set the specified entry in the collection only when the key has been updated.
        /// </summary>
        /// <param name="data">The data collection to check</param>
        /// <param name="key">The key to check</param>
        /// <param name="value">The new value</param>
        /// <param name="lastUpdated">When the value was updated last</param>
        /// <returns>Returns true when the key has been updated, otherwise false</returns>
        public static bool TrySetKey(Dictionary<string, (string value, DateTime lastUpdated)> data, string key, string value, DateTime lastUpdated)
        {
            if (data.TryGetValue(key, out var entry) && entry.lastUpdated >= lastUpdated)
            {
                return false;
            }

            Debug.WriteLine($"Key '{key}' has been updated with value.");
            data[key] = (value, lastUpdated);
            return true;
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        private void OnReload()
        {
            // The change token needs to be changed otherwise we will not be able to reload multiple times.
            // See also: https://github.com/dotnet/AspNetCore.Docs/blob/master/aspnetcore/fundamentals/change-tokens.md
            Debug.WriteLine($"{this.GetType().Name} is being reloaded");
            var previousToken = Interlocked.Exchange(ref _reloadToken, new ConfigurationReloadToken());
            previousToken.OnReload();
        }

        /// <inheritDoc />
        public void Dispose()
        {
            ConfigurationRefreshService.Deregister(this);
            OnDispose();
        }

        /// <summary>
        ///     Custom dispose logic
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}
