using DigitalStore.Core.DTOs;
using DigitalStore.Core.Models;
using DigitalStore.Repository.Context;
using DigitalStore.Service.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.Include(c => c.ProductCategories)
                                                      .ThenInclude(pc => pc.Product)
                                                      .ToListAsync();
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.ProductCategories)
                                                    .ThenInclude(pc => pc.Product)
                                                    .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/Category
        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromQuery] string token, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                // Token'ı doğrula ve claimleri al
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    throw new InvalidTokenException();
                }

                // Token'dan email ve name claim'lerini al
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Eğer email veya name içinde "admin" geçiyorsa, kullanıcı admin olarak kabul edilir
                if ((emailClaim != null && emailClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)) ||
                    (nameClaim != null && nameClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)))
                {
                    // Admin olarak kabul edilir
                }
                else
                {
                    throw new UnauthorizedActionException();
                }

                // Gerekli alanları kontrol et
                if (string.IsNullOrEmpty(categoryDto.Name) || string.IsNullOrEmpty(categoryDto.Url) || string.IsNullOrEmpty(categoryDto.Tags))
                {
                    return BadRequest("Name, Url ve Tags alanları gereklidir.");
                }

                // Yeni kategori oluştur
                var category = new Category
                {
                    Name = categoryDto.Name,
                    Url = categoryDto.Url,
                    Tags = categoryDto.Tags
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (InvalidTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedActionException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InternalServerErrorException ex)
            {
                // InternalServerErrorException özel mesajını döndür
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                // Diğer beklenmedik hatalar için genel bir InternalServerErrorException fırlat
                throw new InternalServerErrorException();  // Sınıfın varsayılan mesajını kullan
            }
        }








        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(category.Name) || string.IsNullOrEmpty(category.Url) || string.IsNullOrEmpty(category.Tags))
            {
                return BadRequest("Name, Url and Tags are required.");
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Category/5
        // DELETE: api/Category/5
        // DELETE: api/Category
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory([FromQuery] int id, [FromQuery] string token)
        {
            try
            {
                // Token'ı doğrula ve claimleri al
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    throw new InvalidTokenException();
                }

                // Token'dan email ve name claim'lerini al
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Eğer email veya name içinde "admin" geçiyorsa, kullanıcı admin olarak kabul edilir
                if ((emailClaim != null && emailClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)) ||
                    (nameClaim != null && nameClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)))
                {
                    // Admin olarak kabul edilir
                }
                else
                {
                    throw new UnauthorizedActionException();
                }

                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (InvalidTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedActionException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InternalServerErrorException ex)
            {
                // InternalServerErrorException özel mesajını döndür
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                // Diğer beklenmedik hatalar için genel bir InternalServerErrorException fırlat
                throw new InternalServerErrorException("Kategori silinirken bir hata oluştu.", ex);
            }
        }


    }
}
