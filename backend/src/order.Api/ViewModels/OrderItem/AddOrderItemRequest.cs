using System.ComponentModel.DataAnnotations;

namespace Order.Api.ViewModels.OrderItem;

public class AddOrderItemRequest
{
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(int.MinValue, int.MaxValue)]
    public int Quantity { get; set; }
}
