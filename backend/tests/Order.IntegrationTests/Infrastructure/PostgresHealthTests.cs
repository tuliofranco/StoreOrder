using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Order.IntegrationTests.Infrastructure;

[Collection("PostgresCollection")]
public class PostgresHealthTests
{
    private readonly PostgresFixture _fixture;

    public PostgresHealthTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShouldConnectToDatabase()
    {
        await using var context = _fixture.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }
}
