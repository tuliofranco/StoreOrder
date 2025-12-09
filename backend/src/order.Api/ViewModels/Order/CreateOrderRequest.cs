using System.ComponentModel.DataAnnotations;

namespace Order.Api.ViewModels.Order;

public class CreateOrderRequest
{
    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;
}
