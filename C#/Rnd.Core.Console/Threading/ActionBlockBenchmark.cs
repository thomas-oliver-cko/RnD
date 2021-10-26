using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Rnd.Core.ConsoleApp.Threading
{
    [SimpleJob(launchCount: 1, warmupCount: 5, targetCount: 10)]
    public class ActionBlockBenchmark
    {
        readonly Client client;
        readonly ActionBlock<object> block;

        public ActionBlockBenchmark()
        {
            client = new Client(100);
            block = new ActionBlock<object>(client.PostMessage);
        }

        [Benchmark]
        public async Task RunActionBlock()
        {
            await foreach (var value in client.Query())
                block.Post(value);

            block.Complete();
            await block.Completion;
        }

        [Benchmark]
        public async Task RunForeachBenchmark()
        {
            await foreach (var value in client.Query())
                await client.PostMessage(value);
        }

        class Client
        {
            readonly int count;

            public Client(int count)
            {
                this.count = count;
            }

            ConcurrentBag<object> ReceivedData { get; } = new();

            public async IAsyncEnumerable<object> Query()
            {
                for (var i = 0; i < count; i++)
                {
                    await Task.Delay(1);
                    yield return new object();
                }
            }

            public Task PostMessage(object data)
            {
                ReceivedData.Add(data);
                return Task.Delay(1);
            }
        }
    }
}