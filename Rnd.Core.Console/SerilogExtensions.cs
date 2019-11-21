using System.Runtime.CompilerServices;
using Serilog;

namespace Rnd.Core.ConsoleApp
{
    static class SerilogExtensions
    {

        public static ILogger Here(this ILogger logger,
            [CallerMemberName] string methodName = "")
        {
            return logger
                .ForContext("MethodName", methodName);
        }
    }
}
