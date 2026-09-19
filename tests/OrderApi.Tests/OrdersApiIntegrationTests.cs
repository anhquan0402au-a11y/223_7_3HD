using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderApi.Models;
using Xunit;

namespace OrderApi.Tests;

public class OrdersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static Order BuildValidOrder()
    {
        Order order = new Order();
        order.CustomerName = "Integration Test";
        order.TotalAmount = 10m;
        order.Items = new List<OrderItem>
        {
            new OrderItem { ProductName = "Mouse", Quantity = 1, UnitPrice = 10m }
        };
        return order;
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_Endpoint_Is_Exposed()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Without_ApiKey_Is_Unauthorized()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/orders", BuildValidOrder());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_With_ApiKey_Creates_Order()
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "dev-secret-key");
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/orders", BuildValidOrder());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_Invalid_Order_Returns_BadRequest()
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "dev-secret-key");
        Order bad = BuildValidOrder();
        bad.TotalAmount = 0m;
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/orders", bad);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
