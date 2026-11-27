using Consul;

namespace PaymentService.Api.Extensions
{
    public static class ConsulRegistration
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(configuration["ConsulConfig:Address"]);
            }));

            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, IConfiguration configuration)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ConsulRegistration");

            var serviceName = configuration["ConsulConfig:ServiceName"];
            var serviceUri = new Uri(configuration["ConsulConfig:ServiceAddress"]);

            var registration = new AgentServiceRegistration
            {
                ID = $"{serviceName}-{serviceUri.Host}-{serviceUri.Port}",
                Name = serviceName,
                Address = serviceUri.Host,
                Port = serviceUri.Port,
                Tags = new[] { serviceName }
            };

            //http check only when the address is reachable by consul (docker)
            var healthCheckAddress = configuration["ConsulConfig:HealthCheckAddress"];

            if (!string.IsNullOrEmpty(healthCheckAddress))
            {
                registration.Check = new AgentServiceCheck
                {
                    HTTP = $"{healthCheckAddress.TrimEnd('/')}/health",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5)
                };
            }

            //old registration removed before register
            lifetime.ApplicationStarted.Register(() =>
            {
                try
                {
                    logger.LogInformation("Registering {ServiceName} with Consul", serviceName);
                    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                    consulClient.Agent.ServiceRegister(registration).Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Consul registration failed for {ServiceName}", serviceName);
                }
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    logger.LogInformation("Deregistering {ServiceName} from Consul", serviceName);
                    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Consul deregistration failed for {ServiceName}", serviceName);
                }
            });

            return app;
        }
    }
}
