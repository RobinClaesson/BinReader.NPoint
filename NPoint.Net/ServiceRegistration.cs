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
            configure(config);

            return services.AddNPoint(config);
        }

        private IServiceCollection AddNPoint(NPointConfig config)
        {
            var service = new ServiceDescriptor(typeof(INPointClient), typeof(NPointClient), config.ServiceLifetime);
            services.AddSingleton(config).Add(service);
            return services;
        }
    }
}