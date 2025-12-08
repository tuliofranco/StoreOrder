using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Infrastructure.Persistence;

namespace Order.IntegrationTests.Infrastructure;

public class OrderApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public OrderApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StoreOrderDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<StoreOrderDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });
        });

        return base.CreateHost(builder);
    }
}
