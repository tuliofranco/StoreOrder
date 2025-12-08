using Order.Core.Domain.Orders.Enums;

namespace Order.Core.Domain.Orders.Rules;

public static class OrderStatusRule
{
    public static void EnsureOrderIsOpen(OrderStatus status, string orderNumber)
    {
        if (status != OrderStatus.Open)
        {
            throw new InvalidOperationException(
                $"Cannot modify a closed order. OrderNumber: {orderNumber}");
        }
    }
}
