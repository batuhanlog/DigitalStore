using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStore.Core.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = Guid.NewGuid().ToString().Substring(0, 10);

        [Required]
        public decimal DiscountAmount { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }

        // Yeni eklenen özellik
        
    }

}
