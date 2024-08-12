using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
