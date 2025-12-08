using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Order.Api.ViewModels;
using Order.Api.ViewModels.OrderItem;
using Order.Core.Application.Common;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using Order.Core.Application.UseCases.Orders.GetAllOrders;
using Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;
using Order.Core.Application.UseCases.OrderItem.AddItem;
using Order.Core.Application.UseCases.Orders.CloseOrder;
using Order.Core.Domain.Orders.Enums;
using Order.IntegrationTests.Infrastructure;
using Xunit;

namespace Order.IntegrationTests.Api;

[Collection("E2E")]
public class OrdersEndpointTests
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;

    public OrdersEndpointTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/orders?page=0&pageSize=25");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<
            ResultViewModel<PagedResult<GetAllOrdersResponse>>>();

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task FullFlow_ShouldCreate_AddItem_Close_AndReturnOrder()
    {
        // 1. Cria o pedido
        var createResponse = await _client.PostAsync("/api/v1/orders", content: null);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult =
            await createResponse.Content.ReadFromJsonAsync<
                ResultViewModel<CreateOrderResponse>>();

        createResult.Should().NotBeNull();
        createResult!.Data.Should().NotBeNull();

        var orderNumber = createResult.Data!.OrderNumber;
        orderNumber.Should().NotBeNullOrWhiteSpace();

        // 2.Adicionar um item ao pedido
        var addItemRequest = new AddOrderItemRequest
        {
            Description = "Product A",
            UnitPrice = 10m,
            Quantity = 2
        };

        var addItemResponse = await _client.PostAsJsonAsync(
            $"/api/v1/orders/{orderNumber}/addItem",
            addItemRequest);

        addItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addItemResult =
            await addItemResponse.Content.ReadFromJsonAsync<
                ResultViewModel<AddOrderItemResponse?>>();

        addItemResult.Should().NotBeNull();
        addItemResult!.Data.Should().NotBeNull();

        // 3.Fecha o pedido
        var closeResponse = await _client.PatchAsync(
            $"/api/v1/orders/{orderNumber}",
            content: null);

        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var closeResult =
            await closeResponse.Content.ReadFromJsonAsync<
                ResultViewModel<CloseOrderResponse>>();

        closeResult.Should().NotBeNull();
        closeResult!.Data.Should().NotBeNull();

        // 4. Buscar o pedido e validar o estado final
        var getResponse = await _client.GetAsync($"/api/v1/orders/{orderNumber}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResult =
            await getResponse.Content.ReadFromJsonAsync<
                ResultViewModel<GetOrderByOrderNumberResponse>>();

        getResult.Should().NotBeNull();
        getResult!.Data.Should().NotBeNull();

        var order = getResult.Data!;

        order.OrderNumber.Should().Be(orderNumber);

        // Pedido fechado
        order.Status.Should().Be(OrderStatus.Closed.ToString());

        // Deve ter 1 item
        order.Items.Should().NotBeNull();
        order.Items.Should().HaveCount(1);
    }
}
