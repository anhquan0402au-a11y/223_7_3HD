using OrderApi.Models;
using OrderApi.Validation;
using Xunit;

namespace OrderApi.Tests;

public class OrderValidatorTests
{
    private static Order BuildValidOrder()
    {
        Order order = new Order();
        order.CustomerName = "Quan Nguyen";
        order.TotalAmount = 25.5m;
        order.Items = new List<OrderItem>
        {
            new OrderItem { ProductName = "Keyboard", Quantity = 1, UnitPrice = 25.5m }
        };
        return order;
    }

    [Fact]
    public void Null_Order_Is_Invalid()
    {
        ValidationResult result = OrderValidator.Validate(null);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Empty_CustomerName_Is_Invalid()
    {
        Order order = BuildValidOrder();
        order.CustomerName = "   ";
        ValidationResult result = OrderValidator.Validate(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void No_Items_Is_Invalid()
    {
        Order order = BuildValidOrder();
        order.Items = new List<OrderItem>();
        ValidationResult result = OrderValidator.Validate(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Zero_Quantity_Item_Is_Invalid()
    {
        Order order = BuildValidOrder();
        order.Items[0].Quantity = 0;
        ValidationResult result = OrderValidator.Validate(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Zero_Total_Is_Invalid()
    {
        Order order = BuildValidOrder();
        order.TotalAmount = 0m;
        ValidationResult result = OrderValidator.Validate(order);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_Order_Passes()
    {
        Order order = BuildValidOrder();
        ValidationResult result = OrderValidator.Validate(order);
        Assert.True(result.IsValid);
    }
}
