using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using Xunit;
using Order.IntegrationTests.Infrastructure.E2E;
using Order.Api.ViewModels;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using FluentAssertions;
using Order.Api.ViewModels.Order;

namespace Order.IntegrationTests.Infrastructure.Golden;

[Collection("Golden")]
public class OrderGoldenTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderGoldenTests(GoldenApiFixture fixture)
    {
        _client = fixture.Client;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    [Fact]
    public async Task Full_Order_Lifecycle_Should_Match_Golden_Snapshots()
    {
        // 1. Cria a ordem
        var orderCreatedRequest = new CreateOrderRequest
        {
            ClientName = "Tulio Franco"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", orderCreatedRequest, _jsonOptions);
        createResponse.EnsureSuccessStatusCode();

        var createResult =
            await createResponse.Content.ReadFromJsonAsync<
                ResultViewModel<CreateOrderResponse>>();

        createResult.Should().NotBeNull();
        createResult!.Data.Should().NotBeNull();

        var created = createResult.Data!;
        created.OrderNumber.Should().NotBeNullOrWhiteSpace();
        created.clientName.Should().Be(orderCreatedRequest.ClientName);

        var orderNumber = created.OrderNumber;

        // 2. Lista todas as ordens
        var listResponse = await _client.GetAsync("/api/v1/orders?page=0&pageSize=25");
        listResponse.EnsureSuccessStatusCode();

        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listNode = JsonNode.Parse(listJson)!;

        var totalItems = listNode["data"]?["totalItems"]!.GetValue<int>();
        totalItems.Should().Be(1);

        // 3. Add item Iphone qtd 3
        await PostAddItemAsync(orderNumber, "Iphone 30", 100000.00m, 3);

        // 4. Adiciona + 1 Iphone
        await PostAddItemAsync(orderNumber, "Iphone 30", 100000.00m, 1);

        // 5. Remove 1 Iphone
        await PostAddItemAsync(orderNumber, "Iphone 30", 100000.00m, -1);

        // 6. Adiciona "Notebook" com quantity = 1
        await PostAddItemAsync(orderNumber, "Notebook", 2500.00m, 1);

        // 7. Dá o get com o Number Id e bate os itens
        var get0Response = await _client.GetAsync($"/api/v1/orders/{orderNumber}");
        get0Response.EnsureSuccessStatusCode();
        var get0Json = await get0Response.Content.ReadAsStringAsync();

        var get0Node = JsonNode.Parse(get0Json)!;
        var items0 = get0Node["data"]?["items"]!.AsArray();
        var iphoneProductId = items0!
            .First(i => i!["description"]!.GetValue<string>() == "Iphone 30")!["productId"]!
            .GetValue<string>();

        iphoneProductId.Should().NotBeNullOrWhiteSpace();

        await AssertMatchesGoldenAsync("GetByOrderNumberResponse0.json", get0Json);

        // 8. Deleta o Iphone
        var deleteResponse = await _client.DeleteAsync($"/api/v1/orders/{orderNumber}/{iphoneProductId}");
        deleteResponse.EnsureSuccessStatusCode();

        // 9. Bate valor
        var get1Response = await _client.GetAsync($"/api/v1/orders/{orderNumber}");
        get1Response.EnsureSuccessStatusCode();
        var get1Json = await get1Response.Content.ReadAsStringAsync();

        await AssertMatchesGoldenAsync("GetByOrderNumberResponse1.json", get1Json);

        // 10. Fecha a ordem
        var patchResponse = await _client.PatchAsync($"/api/v1/orders/{orderNumber}", content: null);
        patchResponse.EnsureSuccessStatusCode();

        // 11. Dá o get e bate os itens
        var get2Response = await _client.GetAsync($"/api/v1/orders/{orderNumber}");
        get2Response.EnsureSuccessStatusCode();
        var get2Json = await get2Response.Content.ReadAsStringAsync();

        await AssertMatchesGoldenAsync("GetByOrderNumberResponse2.json", get2Json);
    }

    private async Task PostAddItemAsync(string orderNumber, string description, decimal unitPrice, int quantity)
    {
        var body = new
        {
            description,
            unitPrice,
            quantity
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/orders/{orderNumber}/addItem",
            body,
            _jsonOptions);

        response.EnsureSuccessStatusCode();
    }

    private async Task AssertMatchesGoldenAsync(string goldenFileName, string actualJson)
    {
        var actualNode = JsonNode.Parse(actualJson)!;

        var goldenPath = GetGoldenFilePath(goldenFileName);
        var goldenJson = await File.ReadAllTextAsync(goldenPath);
        var expectedNode = JsonNode.Parse(goldenJson)!;

        NormalizeApiResponse(actualNode);
        NormalizeApiResponse(expectedNode);

        AssertJsonEquals(expectedNode, actualNode);
    }

    private static void NormalizeApiResponse(JsonNode node)
    {
        if (node is not JsonObject root)
            return;

        if (root.TryGetPropertyValue("data", out var dataNode) && dataNode is JsonObject data)
        {
            data.Remove("orderNumber");
            data.Remove("createdAt");
            data.Remove("updatedAt");
            data.Remove("closedAt");

            if (data.TryGetPropertyValue("items", out var itemsNode) && itemsNode is JsonArray itemsArray)
            {
                foreach (var item in itemsArray)
                {
                    if (item is JsonObject itemObj)
                    {
                        itemObj.Remove("productId");
                    }
                }
            }
        }
    }

    private static string GetGoldenFilePath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        return Path.Combine(projectDir, "Order.IntegrationTests", "GoldenTest", "Data", fileName);
    }

    private static void AssertJsonEquals(JsonNode expected, JsonNode actual)
    {
        var expData = expected["data"]!;
        var actData = actual["data"]!;

        Assert.Equal(
            expData["status"]!.GetValue<string>(),
            actData["status"]!.GetValue<string>());

        Assert.Equal(
            expData["total"]!.GetValue<decimal>(),
            actData["total"]!.GetValue<decimal>());

        var expItems = expData["items"]!.AsArray();
        var actItems = actData["items"]!.AsArray();

        Assert.Equal(expItems.Count, actItems.Count);

        for (int i = 0; i < expItems.Count; i++)
        {
            var e = expItems[i]!;
            var a = actItems[i]!;

            Assert.Equal(e["description"]!.GetValue<string>(), a["description"]!.GetValue<string>());
            Assert.Equal(e["quantity"]!.GetValue<int>(), a["quantity"]!.GetValue<int>());
            Assert.Equal(e["unitPrice"]!.GetValue<decimal>(), a["unitPrice"]!.GetValue<decimal>());
            Assert.Equal(e["subtotal"]!.GetValue<decimal>(), a["subtotal"]!.GetValue<decimal>());
        }
    }
}
