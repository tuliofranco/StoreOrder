using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Order.IntegrationTests.Infrastructure;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithName($"postgres-tests-{Guid.NewGuid()}")
            .WithDatabase("orders_db_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16-alpine")
            .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public StoreOrderDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StoreOrderDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new StoreOrderDbContext(options);
    }
}

[CollectionDefinition("PostgresCollection")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
