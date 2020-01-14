using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v1
{
    public class DynamoDbMetrics
    {
        public DynamoDbMetrics(string name, OperationType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public OperationType Type { get; set; }
        public long Duration { get; set; }
        public int Records { get; set; }
        public double ReadCapacityUnits { get; set; }
        public double WriteCapacityUnits { get; set; }
        public string Error { get; set; }
        public Exception Exception { get; set; }

        public void Write()
        {
            Console.WriteLine($"{Name} too {Duration}ms and used {ReadCapacityUnits} RCUs and {WriteCapacityUnits} WRUs");
        }

        public async Task<TResult> HandleAsync<TResult>(Task<TResult> loadAction)
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                return await loadAction;
            }
            catch (Exception e)
            {
                Exception = e;
                throw;
            }
            finally
            {
                Duration = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
            }
        }

        public async Task HandleAsync(Task loadAction)
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                await loadAction;
            }
            catch (Exception e)
            {
                Exception = e;
                throw;
            }
            finally
            {
                Duration = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
            }
        }
    }

    public enum OperationType
    {
        Load,
        Get
    }
}
