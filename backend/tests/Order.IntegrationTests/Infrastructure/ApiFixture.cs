using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Order.IntegrationTests.Infrastructure;

public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithDatabase("orders_db_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16-alpine")
            .Build();

    private OrderApiFactory _factory = default!;

    public HttpClient Client { get; private set; } = default!;
    public string ConnectionString => _container.GetConnectionString();

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

[CollectionDefinition("ApiCollection")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}
