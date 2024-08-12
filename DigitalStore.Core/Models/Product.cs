using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalStore.Core.Models
{
    public class Product 
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int StockQuantity { get; set; } = 0;  // Stok adedi
        public bool IsAvailable { get; set; } = true; // Satışta olup olmadığını belirten alan


        public decimal PointsPercentage { get; set; } = 12; // Default points percentage
        public decimal MaxPoints { get; set; } = 10; // Default max point

        // Many-to-Many 
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    }
}
