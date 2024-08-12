using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStore.Core.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = Guid.NewGuid().ToString().Substring(0, 10);

        
        public int DiscountAmount { get; set; }

        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;


        public int UserId { get; set; }
        public User User { get; set; } 
    }
}
