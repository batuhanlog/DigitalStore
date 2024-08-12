using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.DTOs
{
    public class ProductDto
    {
        [Required, MaxLength(25)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(25)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; } 

        [Required]
        public int Price { get; set; }

        [MaxLength(50)]
        public string? Description { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}
