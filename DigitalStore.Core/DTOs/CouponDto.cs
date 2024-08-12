using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.DTOs
{
    public class CouponDto
    {
        public string Code { get; set; } = string.Empty;
        public int DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; } // ExpiryDate alanını ekleyin
    }

}
