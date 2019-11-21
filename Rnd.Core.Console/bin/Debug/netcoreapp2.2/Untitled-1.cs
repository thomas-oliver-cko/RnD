using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Tests.Unit.Core.Logging
{
    class TestsLogSink : ILogEventSink
    {
        public ConcurrentBag<LogEvent> Events { get; set; }
        public LogEvent First => Events.First()

        public TestsLogSink()
        {
            Events = new ConcurrentBag<LogEvent>();
        }

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }

    public class DependencyExtensionsTests
    {
        readonly SomeInput someInput = new SomeInput { Property1 = "SomeValue" };
        readonly SomeResult someResult = new SomeResult { Property1 = "SomeValue" };
        readonly ITest component = Substitute.For<ITest>();

        readonly TestsLogSink logSink;

        public DependencyExtensionsTests()
        {
            logSink = new TestsLogSink();
            var log = new LoggerConfiguration()
                .WriteTo.Sink(logSink, LogEventLevel.Verbose)
                .CreateLogger();

            Log.Logger = log;
        }

        [Fact]
        public async Task CallDependency_WhenSuccess_SendsLogAsSuccess()
        {
            component.SomeTask(someInput).Returns(someResult);

            var result = await this.CallDependency(o => component.SomeTask(o), someInput, nameof(CallDependency_WhenSuccess_SendsLogAsSuccess));
            var logEvent = logSink.Events.First();

            result.Should().Be(someResult);
            logEvent.Level.Should().Be(LogEventLevel.Information);
        }

        [Fact]
        public void CallDependency_WhenError_SendsLogAsError()
        {
            var exception = new Exception("some exception");
            component.SomeTask(someInput).Throws(exception);

            var call = this.CallDependency(o => component.SomeTask(o), someInput, nameof(CallDependency_WhenError_SendsLogAsError));
            var logEvent = logSink.Events.First();

            call.Should().Throws<Exception>();
            logEvent.Level.Should().Be(LogEventLevel.Error);
        }

        internal interface ITest
        {
            Task<SomeResult> SomeTask(SomeInput o);
        }

        internal class SomeInput
        {
            public string Property1 { get; set; }
        }

        internal class SomeResult
        {
            public string Property1 { get; set; }
        }
    }

    static class DependencyExtensions
    {
        public static Task<TResult> CallDependency<TArg, TResult>(
            this object agent,
            Func<TArg, Task<TResult>> dependencyCall,
            TArg args,
            string name = null)
        {
            var dependencyData = new DependencyMetadata<TArg>
            {
                DependencyName = name ?? dependencyCall.GetMethodInfo().Name
            };

            return new CallDependency<TArg, TResult>(dependencyCall, dependencyData)
                .ExecuteAsync(args);
        }
    }

    sealed class DependencyInvocation<TArg, TResult>
    {
        readonly Stopwatch timer;

        public DependencyInvocation(DependencyMetadata<TArg> dependencyMetadata, TArg arg)
        {
            DependencyMetadata = dependencyMetadata;
            Argument = arg;
            timer = new Stopwatch();
        }

        public DependencyMetadata<TArg> DependencyMetadata { get; }

        public TArg Argument { get; }

        public TResult Result { get; private set; }

        internal void Start()
        {
            DependencyMetadata.StartTime = DateTime.UtcNow;
            timer.Start();
        }

        internal void Fail(Exception ex)
        {
            timer.Stop();
            DependencyMetadata.Error = ex;
            DependencyMetadata.Duration = timer.Elapsed;
            DependencyMetadata.Success = false;
        }

        internal void Pass(TResult result)
        {
            timer.Stop();
            Result = result;
            DependencyMetadata.Duration = timer.Elapsed;
            DependencyMetadata.Success = true;
        }

        internal void Send()
        {
            // TODO RM: pass in metadata and args into the actual properties also of the log event

            var ex = DependencyMetadata.Success ? null : DependencyMetadata.Error;
            if (ex == null)
            {
                Log.Logger.Information($"Invoke {DependencyMetadata.DependencyName} successfully.");

                return;
            }

            Log.Logger.Error(ex, $"Invoke {DependencyMetadata.DependencyName} with error.");
        }
    }

    class CallDependency<TArg, TResult>
    {
        readonly Func<TArg, Task<TResult>> dependencyCall;

        public DependencyMetadata<TArg> DependencyMetadata { get; }

        public CallDependency(Func<TArg, Task<TResult>> dependencyCall, DependencyMetadata<TArg> dependencyMetadata)
        {
            this.dependencyCall = dependencyCall ?? throw new ArgumentNullException(nameof(dependencyCall));

            DependencyMetadata = dependencyMetadata;
        }

        public async Task<TResult> ExecuteAsync(TArg arg)
        {
            var invocation = new DependencyInvocation<TArg, TResult>(DependencyMetadata, arg);
            try
            {
                if (DependencyMetadata.Data == null && arg != null) DependencyMetadata.Data = arg;
                invocation.Start();
                var result = await dependencyCall(arg);
                invocation.Pass(result);

                return result;
            }
            catch (Exception ex)
            {
                invocation.Fail(ex);
                throw;
            }
            finally
            {
                invocation.Send();
            }
        }
    }

    class DependencyMetadata<T>
    {
        public string DependencyName { get; set; }
        public bool Success { get; set; }
        public Exception Error { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public T Data { get; set; }
    }
}
