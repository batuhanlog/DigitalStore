using DigitalStore.Repository.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("my-wallet")]
        //[Authorize]
        public IActionResult MyWallet([FromForm] string cardNumber, [FromForm] string cvv, [FromForm] string expiryDate, [FromQuery] string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return Unauthorized("Invalid token.");
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not found from token.");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (cardNumber.Length == 16 && cvv.Length == 3 && !string.IsNullOrEmpty(expiryDate))
            {
                // WalletBalance'ı artır
                user.WalletBalance += 500;
                _context.SaveChanges();
                return Ok($"Welcome to your wallet. The developer (he's awesome) gave you 500$ for coming after a long time 🎉 :{user.WalletBalance:C}");
            }

            return BadRequest("Invalid credit card information.");
        }

        [HttpGet("my-budget")]
        //[Authorize]
        public IActionResult MyBudget([FromQuery] string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return Unauthorized("Invalid token.");
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not found from token.");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var budgetInfo = new
            {
                FullName = $"{user.FirstName} {user.LastName}",
                WalletBalance = $"{user.WalletBalance:C}",
                Points = user.Points
            };

            return Ok(budgetInfo);
        }
    }
}
