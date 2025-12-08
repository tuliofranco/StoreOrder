using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Order.Core.Application.Common;
using Order.Core.Application.UseCases.Orders.GetAllOrders;
using Order.IntegrationTests.Infrastructure;
using Order.Api.ViewModels;
using Xunit;

namespace Order.IntegrationTests.Api;

[Collection("ApiCollection")]
public class OrdersEndpointTests
{
    private readonly ApiFixture _fixture;

    public OrdersEndpointTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOk()
    {
        var client = _fixture.Client;

        var response = await client.GetAsync("/api/v1/orders?page=0&pageSize=25");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<
            ResultViewModel<PagedResult<GetAllOrdersResponse>>>();

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
    }
}
