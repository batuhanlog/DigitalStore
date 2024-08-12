using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.DTOs
{
    public class WalletUpdateDto
    {
        public int UserId { get; set; }
        public CreditCardDto CreditCardInfo { get; set; }
    }
}
