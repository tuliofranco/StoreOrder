using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Core.Application.Abstractions.Repositories;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Repositories;
using Order.Core.Application.Abstractions;
using Order.Infrastructure.Caching;
using StackExchange.Redis;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? dbConnection = null, string? redisConnection = null)
    {
        dbConnection ??= Environment.GetEnvironmentVariable("STRING_CONNECTION");
        if (string.IsNullOrWhiteSpace(dbConnection))
            throw new InvalidOperationException("Connection string do Postgres não configurada (defina STRING_CONNECTION ou passe via AddInfrastructure).");


        redisConnection ??= Environment.GetEnvironmentVariable("REDIS_CONNECTION");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            var mux = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton<IConnectionMultiplexer>(mux);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "order-service:";
            });

            services.AddScoped<ICacheService, DistributedCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICacheService, NullCacheService>();
        }

        services.AddDbContext<StoreOrderDbContext>(opt =>
        {
            opt.UseNpgsql(
                dbConnection,
                npg => npg.MigrationsAssembly(typeof(StoreOrderDbContext).Assembly.FullName)
            );
        });


        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
