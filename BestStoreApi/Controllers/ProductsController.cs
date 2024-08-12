using DigitalStore.Core.DTOs;
using DigitalStore.Core.Models;
using DigitalStore.Repository.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string? token, string? search, string? category,
                                         int? minPrice, int? maxPrice,
                                         string? sort, string? order,
                                         int? page, bool? isAvailable)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            IQueryable<Product> query = _context.Products.Include(p => p.ProductCategories)
                                                         .ThenInclude(pc => pc.Category);

            // Filtreleme ve sıralama işlemleri
            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (category != null)
            {
                query = query.Where(p => p.ProductCategories.Any(pc => pc.Category.Name == category));
            }

            if (minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == isAvailable.Value);
            }

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

            // Sayfalama işlemi
            if (page == null || page < 1) page = 1;

            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize);

            query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            var result = new
            {
                Products = products,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(result);
        }

        [HttpGet("by-category/{categoryName}")]
        public IActionResult GetProductsByCategory([FromQuery] string token, string categoryName)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            var products = _context.Products.Include(p => p.ProductCategories)
                                            .ThenInclude(pc => pc.Category)
                                            .Where(p => p.ProductCategories.Any(pc => pc.Category.Name == categoryName))
                                            .ToList();

            if (!products.Any())
            {
                return NotFound($"Category '{categoryName}' does not exist or has no products.");
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct([FromQuery] string token, int id)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            var product = _context.Products.Include(p => p.ProductCategories)
                                           .ThenInclude(pc => pc.Category)
                                           .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromQuery] string token, [FromForm] ProductDto productDto)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            var product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Price = productDto.Price,
                Description = productDto.Description,
                CreatedAt = DateTime.UtcNow,
                StockQuantity = productDto.StockQuantity,
                IsAvailable = productDto.IsAvailable
            };

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid Category ID.");
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromQuery] string token, int id, [FromForm] ProductDto productDto)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

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
            product.StockQuantity = productDto.StockQuantity;
            product.IsAvailable = productDto.IsAvailable;

            product.ProductCategories.Clear();

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid Category ID.");
            }

            product.ProductCategories.Add(new ProductCategory
            {
                Product = product,
                Category = category
            });

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock([FromQuery] string token, int id, int stockQuantity)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.StockQuantity = stockQuantity;
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct([FromQuery] string token, int id)
        {
            if (!IsTokenValid(token, out var response))
            {
                return response;
            }

            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok();
        }

        private bool IsTokenValid(string? token, out IActionResult? response)
        {
            response = null;

            if (string.IsNullOrEmpty(token))
            {
                response = Unauthorized("Token is required.");
                return false;
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken;

            try
            {
                jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            }
            catch (Exception)
            {
                response = Unauthorized("Invalid token.");
                return false;
            }

            if (jwtToken == null)
            {
                response = Unauthorized("Invalid token.");
                return false;
            }

            return true;
        }
    }
}
