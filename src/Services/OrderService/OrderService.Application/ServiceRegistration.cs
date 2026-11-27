using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.IntegrationEvents.EventHandlers;
using System.Reflection;

namespace OrderService.Application
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationRegistration(this IServiceCollection services)
        {
            //mediatr handlers scanned from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            //integration event handlers resolved by eventbus
            services.AddTransient<OrderCreatedIntegrationEventHandler>();
            services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();
            services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();

            return services;
        }
    }
}
