using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStore.Core.Models
{
    [Index("Email", IsUnique = true)]
    public class User : IdentityUser<int>
    {
        [MaxLength(100)]
        public string FirstName { get; set; } = "";

        [MaxLength(100)]
        public string LastName { get; set; } = "";

        [MaxLength(100)]
        public string Address { get; set; } = "";

       
        public int WalletBalance { get; set; } = 1000;

        public int Points { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Kullanıcı ile ilişkili kuponlar
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    }
}
