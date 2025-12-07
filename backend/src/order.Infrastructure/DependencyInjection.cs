using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Core.Application.Abstractions.Repositories;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Repositories;
using Order.Core.Application.Abstractions;


namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? dbConnection = null)
    {
        dbConnection ??= Environment.GetEnvironmentVariable("STRING_CONNECTION");
        if (string.IsNullOrWhiteSpace(dbConnection))
            throw new InvalidOperationException("Connection string do Postgres não configurada (defina STRING_CONNECTION ou passe via AddInfrastructure).");

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
