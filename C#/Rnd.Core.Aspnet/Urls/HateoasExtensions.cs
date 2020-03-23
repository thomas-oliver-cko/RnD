using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tapioca.HATEOAS;

namespace Rnd.Core.Aspnet.Urls
{
    public static class HateoasExtensions
    {
        public static IServiceCollection AddHateoas(this IServiceCollection services)
        {
            var filtertOptions = new HyperMediaFilterOptions();
            // add enrichers
            services.AddSingleton(filtertOptions);
            return services;
        }

        public static MvcOptions AddHateoas(this MvcOptions options)
        {
            options.Filters.Add<HyperMediaFilter>();
            return options;
        }
    }
}
