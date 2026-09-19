using OrderApi.Models;
using OrderApi.Exceptions;

namespace OrderApi.Validation
{

    public static class OrderValidator
    {
        public static void Validate(Order? order)
        {
            if (order == null)
            {
                throw new OrderValidationException("Order is required.");
            }

            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                throw new OrderValidationException("Customer name is required.");
            }

            if (order.Items == null)
            {
                throw new OrderValidationException("At least one order item is required.");
            }

            if (order.Items.Count == 0)
            {
                throw new OrderValidationException("At least one order item is required.");
            }

            foreach (OrderItem item in order.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ProductName))
                {
                    throw new OrderValidationException("Each item must have a product name.");
                }

                if (item.Quantity <= 0)
                {
                    throw new OrderValidationException("Each item quantity must be greater than zero.");
                }

                if (item.UnitPrice < 0)
                {
                    throw new OrderValidationException("Item unit price cannot be negative.");
                }
            }

            if (order.TotalAmount <= 0)
            {
                throw new OrderValidationException("Total amount must be greater than zero.");
            }
        }
    }
}