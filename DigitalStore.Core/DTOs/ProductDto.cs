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
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; } 

        [Required]
        public decimal Price { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }


    }
}
