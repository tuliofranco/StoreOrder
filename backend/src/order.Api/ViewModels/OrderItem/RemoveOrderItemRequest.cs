using System.ComponentModel.DataAnnotations;

namespace Order.Api.ViewModels.OrderItem;

public class RemoveOrderItemRequest
{
    [Required]
    [MaxLength(60)]
    public string ProductId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}