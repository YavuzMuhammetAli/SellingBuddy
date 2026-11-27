using StackExchange.Redis;

namespace BasketService.Api.Extensions
{
    public static class RedisRegistration
    {
        public static IServiceCollection ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(sp =>
            {
                var redisConfig = ConfigurationOptions.Parse(configuration["RedisSettings:ConnectionString"], true);

                //app keeps starting when redis is not ready yet
                redisConfig.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisConfig);
            });

            return services;
        }
    }
}
