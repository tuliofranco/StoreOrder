namespace Order.Core.Application.Orders;

public static class OrderCacheKeys
{
    public static string ByOrderNumber(string orderNumber)
        => $"order:by-number:{orderNumber}";

    public static string ById(Guid id)
        => $"order:by-id:{id}";
}