using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;

namespace Rnd.Core.ConsoleApp
{
    public static class AppMetrics
    {
        public static async Task Execute()
        {
            var metrics = new MetricsBuilder()
                .Build();

            var counter = new CounterOptions {Name = "test_counter"};
            metrics.Measure.Counter.Increment(counter);

            var snapshot = metrics.Snapshot.Get();

            using (var stream = new MemoryStream())
            {
                await metrics.DefaultOutputMetricsFormatter.WriteAsync(stream, snapshot);
                var result = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine(result);
            }
        }
    }
}
