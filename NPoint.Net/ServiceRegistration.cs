using Microsoft.Extensions.DependencyInjection;
using NPoint.Net.Config;

namespace NPoint.Net;

public static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNPoint() => services.AddNPoint(new NPointConfig());

        public IServiceCollection AddNPoint(Action<NPointConfig> configure)
        {
            var config = new NPointConfig();
            configure(new NPointConfig());

            return services.AddNPoint(config);
        }

        private IServiceCollection AddNPoint(NPointConfig config) => services
            .AddSingleton(config)
            .AddScoped<INPointClient, NPointClient>();
    }
}