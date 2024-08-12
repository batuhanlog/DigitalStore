using DigitalStore.Core.Models;
using DigitalStore.Repository.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponDto couponDto, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupon = new Coupon
            {
                Code = couponDto.Code,
                DiscountAmount = couponDto.DiscountAmount,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return Ok(new { CouponId = coupon.Id, coupon.Code, coupon.DiscountAmount });
        }


        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetCoupons([FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupons = await _context.Coupons.Include(c => c.User).ToListAsync();
            return Ok(coupons);
        }

        [HttpGet("{id}")]
        //[Authorize]
        public async Task<IActionResult> GetCouponById(int id, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupon = await _context.Coupons.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return NotFound("Coupon not found.");
            }
            return Ok(coupon);
        }

        [HttpPut("{id}")]
        //[Authorize]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponDto updatedCoupon, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound("Coupon not found.");
            }

            coupon.DiscountAmount = updatedCoupon.DiscountAmount;
            coupon.ExpiryDate = DateTime.UtcNow.AddMonths(1); // ExpiryDate'i burada belirleyin

            await _context.SaveChangesAsync();

            return Ok(coupon);
        }

        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteCoupon(int id, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound("Coupon not found.");
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return Ok("Coupon deleted successfully.");
        }

        [HttpPost("apply")]
        //[Authorize]
        public async Task<IActionResult> ApplyCoupon(string code, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && !c.IsUsed && c.ExpiryDate > DateTime.UtcNow);
            if (coupon == null)
            {
                return BadRequest("Invalid or expired coupon.");
            }

            coupon.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(coupon);
        }

        private int? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return null;
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            return userId;
        }
    }

    public class CouponDto
    {
        public string Code { get; set; } = string.Empty;
        public int DiscountAmount { get; set; }
    }
}
