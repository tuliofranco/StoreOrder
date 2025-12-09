using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Api.ViewModels;
using Order.Api.ViewModels.OrderItem;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using Order.Core.Application.UseCases.OrderItem.AddItem;
using Order.Core.Application.UseCases.Orders.CloseOrder;
using Order.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;
using Order.IntegrationTests.Infrastructure.E2E;

namespace Order.IntegrationTests.Infrastructure.Golden;

public class GoldenApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithDatabase("orders_db_golden")
            .WithName("Golden-Postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16-alpine")
            .Build();

    public OrderApiFactory _factory = default!;

    public HttpClient Client { get; private set; } = default!;
    public string ConnectionString => _container.GetConnectionString();
    public OrderApiFactory Factory => _factory;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        Environment.SetEnvironmentVariable("STRING_CONNECTION", ConnectionString);

        var options = new DbContextOptionsBuilder<StoreOrderDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using (var ctx = new StoreOrderDbContext(options))
        {
            await ctx.Database.MigrateAsync();
        }

        _factory = new OrderApiFactory(ConnectionString);
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        await _container.DisposeAsync();
    }

}

[CollectionDefinition("Golden")]
public class GoldenCollection : ICollectionFixture<GoldenApiFixture>
{
}
