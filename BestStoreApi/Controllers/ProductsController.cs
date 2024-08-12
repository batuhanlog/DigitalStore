using DigitalStore.Core.DTOs;
using DigitalStore.Core.Models;
using DigitalStore.Repository.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }

        [HttpGet]
        public IActionResult GetProducts(string? search, string? category,
                                         int? minPrice, int? maxPrice,
                                         string? sort, string? order,
                                         int? page, bool? isAvailable)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.ProductCategories)
                                                         .ThenInclude(pc => pc.Category);

            // Arama işlevselliği
            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            // Kategoriye göre filtreleme
            if (category != null)
            {
                query = query.Where(p => p.ProductCategories.Any(pc => pc.Category.Name == category));
            }

            // Fiyat aralığı filtreleme
            if (minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // Stok durumu filtreleme
            if (isAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == isAvailable.Value);
            }

            // Sıralama işlevselliği
            if (sort == null) sort = "id";
            if (order == null || order != "asc") order = "desc";

            switch (sort.ToLower())
            {
                case "name":
                    query = order == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
                case "brand":
                    query = order == "asc" ? query.OrderBy(p => p.Brand) : query.OrderByDescending(p => p.Brand);
                    break;
                case "category":
                    query = order == "asc" ? query.OrderBy(p => p.ProductCategories.FirstOrDefault().Category.Name) : query.OrderByDescending(p => p.ProductCategories.FirstOrDefault().Category.Name);
                    break;
                case "price":
                    query = order == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "date":
                    query = order == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    query = order == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                    break;
            }

            // Sayfalama işlevselliği
            if (page == null || page < 1) page = 1;

            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize);

            query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            var response = new
            {
                Products = products,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "admin")]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.Include(p => p.ProductCategories)
                                           .ThenInclude(pc => pc.Category)
                                           .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Price = productDto.Price,
                Description = productDto.Description,
                CreatedAt = DateTime.UtcNow,
                StockQuantity = productDto.StockQuantity, // Stok miktarı
                IsAvailable = productDto.IsAvailable // Ürün durumu
            };

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Geçersiz Kategori ID'si.");
            }

            product.ProductCategories.Add(new ProductCategory
            {
                Product = product,
                Category = category
            });

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        //[Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto productDto)
        {
            var product = await _context.Products.Include(p => p.ProductCategories)
                                                 .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Price = productDto.Price;
            product.Description = productDto.Description ?? "";
            product.StockQuantity = productDto.StockQuantity; // Stok miktarını güncelle
            product.IsAvailable = productDto.IsAvailable; // Ürün durumunu güncelle

            product.ProductCategories.Clear();

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Geçersiz Kategori ID'si.");
            }

            product.ProductCategories.Add(new ProductCategory
            {
                Product = product,
                Category = category
            });

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        //[Authorize(Roles = "admin")]
        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, int stockQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.StockQuantity = stockQuantity;
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        //[Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok();
        }
    }
}
