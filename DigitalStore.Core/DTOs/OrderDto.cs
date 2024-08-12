using System.ComponentModel.DataAnnotations;

namespace DigitalStore.Core.DTOs
{
    public class OrderDto
    {
        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
