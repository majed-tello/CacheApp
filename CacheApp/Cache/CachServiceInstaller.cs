
namespace CacheApp.Cache;

public static class CachServiceInstaller
{
    public static IServiceCollection InstallCachService(this IServiceCollection services, IConfiguration configuration)
    {
        CacheOptions cacheOptions = new();
        configuration.GetSection("CacheSettings").Bind(cacheOptions);

        services.AddSingleton(cacheOptions);

        if (cacheOptions.Enabled)
        {
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options => options.Configuration = cacheOptions.RedisConnection);

            services.AddSingleton<InMemoryCacheService>();
            services.AddSingleton<RedisCacheService>();

            services.AddSingleton<CacheServiceResolver>(serviceProvider => cacheTechnology =>
            {
                switch (cacheTechnology)
                {
                    case CacheTechnologies.InMemory:
                        return serviceProvider.GetService<InMemoryCacheService>()!;
                    case CacheTechnologies.Redis:
                        return serviceProvider.GetService<RedisCacheService>()!;

                    default:
                        return serviceProvider.GetService<InMemoryCacheService>()!;
                }
            });
        }

        return services;
    }
}