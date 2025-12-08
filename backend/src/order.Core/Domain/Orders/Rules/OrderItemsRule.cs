using System.Collections.Generic;

namespace Order.Core.Domain.Orders.Rules;

public static class OrderItemsRule
{
    public static void EnsureOrderHasItems<T>(IReadOnlyCollection<T> items, string orderNumber)
    {
        if (items is null || items.Count == 0)
        {
            throw new InvalidOperationException(
                $"Id do pedido: {orderNumber}; O pedido não pode ser fechado pois não possui itens.");
        }
    }
}
