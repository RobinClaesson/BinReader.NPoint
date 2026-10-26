using BinReader.NPoint.Config;
using Microsoft.Extensions.DependencyInjection;

namespace BinReader.NPoint;

/// <summary>
/// Provides extension methods for registering npoint clients with dependency injection.
/// </summary>
public static class NPointServiceRegistration
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers <see cref="INPointClient"/> and its configuration using the default settings.
        /// </summary>
        /// <returns>The service collection so that additional registrations can be chained.</returns>
        /// <remarks>
        /// The client uses a scoped lifetime and a file system cache with a five-minute time to live.
        /// The service collection must also register <see cref="IHttpClientFactory"/>, for example
        /// by calling <c>AddHttpClient()</c>.
        /// </remarks>
        public IServiceCollection AddNPoint() => services.AddNPoint(new NPointConfig());

        /// <summary>
        /// Registers <see cref="INPointClient"/> and its configuration using the supplied configuration action.
        /// </summary>
        /// <param name="configure">An action that customizes the default client configuration before registration.</param>
        /// <returns>The service collection so that additional registrations can be chained.</returns>
        /// <remarks>
        /// The configuration is registered as a singleton, and the client uses the configured service lifetime.
        /// The service collection must also register <see cref="IHttpClientFactory"/>, for example
        /// by calling <c>AddHttpClient()</c>.
        /// </remarks>
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