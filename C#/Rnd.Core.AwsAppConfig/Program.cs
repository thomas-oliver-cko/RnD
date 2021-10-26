using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Rnd.Core.AwsAppConfig
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureUsingAwsAppConfig()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Settings>(context.Configuration.GetSection("Settings"));
                });

            using var host = builder.Start();

            var settings = host.Services.GetService<IOptionsMonitor<Settings>>();
            settings.OnChange(s => Console.WriteLine(s.MySetting));

            while (true)
            {
                Console.WriteLine(settings.CurrentValue.MySetting);
                Thread.Sleep(5000);
            }
        }
    }

    class Settings
    {
        public string MySetting { get; set; }
    }
}
