using Nest;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnd.Core.ConsoleApp.ElasticSearch
{
    public class NestReconciler
    {
        const int CountPerPage = 100;

        readonly IElasticClient client;
        readonly AsyncRetryPolicy<ISearchResponse<object>> policy;

        public NestReconciler()
        {
            policy = Policy<ISearchResponse<object>>
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(100 * i));

           var connection = new ConnectionSettings(new Uri(
                    ""));
            client = new ElasticClient(connection);
        }

        public async Task<List<string>> Reconcile(string sourceIndex, string targetIndex)
        {
            var missedKeys = new List<string>();
            var page = 0;

            var searchTask = GetEntitiesFrom(sourceIndex, page);
            page++;

            while (true)
            {
                var response = await searchTask;

                if (response.Hits.Count == 0)
                    break;

                searchTask = GetEntitiesFrom(sourceIndex, page);

                var requests = response.Hits
                    .Select(h => (h.Id, client.GetAsync<object>(h.Id, g => g.Index(targetIndex))))
                    .ToList();
                
                foreach (var (id, task) in requests)
                {
                    var found = await CheckEntityExistsInTarget(task);

                    if (!found)
                        missedKeys.Add(id);
                }

                page++;
                Console.WriteLine(page * CountPerPage);
            }

            return missedKeys;
        }

        public async Task<List<string>> Reconcile(string index, string[] ids)
        {
            var missedKeys = new List<string>();

            for (var i = 0; i < ids.Length; i++)
            {
                var hitTask = client.GetAsync<object>(ids[i], g => g.Index(index));

                var found = await CheckEntityExistsInTarget(hitTask);

                if (!found)
                    missedKeys.Add(ids[i]);

                if (i % 100 == 0)
                    Console.WriteLine($"{i}/{ids.Length}");
            }

            return missedKeys;
        }

        async Task<ISearchResponse<object>> GetEntitiesFrom(string index, int page)
        {
            try
            {
                return await policy.ExecuteAsync(() => client.SearchAsync<object>(s => s.Index(index)
                    .Query(q => q.MatchAll())
                    .Skip(page)
                    .Take(CountPerPage)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static async Task<bool> CheckEntityExistsInTarget(Task<GetResponse<object>> task)
        {
            try
            {
                var entity = await task;

                return entity.Found;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }
    }
}
