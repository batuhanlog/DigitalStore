using DigitalStore.Core.DTOs;
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
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        //[Authorize]
        public async Task<IActionResult> CreateOrder(int productId, [FromQuery] string token)
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (!product.IsAvailable || product.StockQuantity <= 0)
            {
                return BadRequest("Product is not available or out of stock.");
            }

            decimal amountToPay = product.Price;

            if (user.WalletBalance < amountToPay)
            {
                return BadRequest("Insufficient wallet balance.");
            }

            // Deduct amount from wallet
            user.WalletBalance -= (int)amountToPay;

            // Calculate and add new points
            decimal pointsEarned = (amountToPay * product.PointsPercentage) / 100;
            pointsEarned = Math.Min(pointsEarned, product.MaxPoints);
            user.Points += (int)pointsEarned;

            // Update stock quantity
            product.StockQuantity -= 1;

            // Create order
            var order = new Order
            {
                UserId = userId,
                ProductId = productId,
                DeliveryAddress = "", // Sadece zorunlu alanlar olmadığı için boş geçiyoruz.
                PaymentMethod = "", // Sadece zorunlu alanlar olmadığı için boş geçiyoruz.
                OrderNumber = Guid.NewGuid().ToString(),
                TotalAmount = amountToPay,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order created successfully",
                OrderNumber = order.OrderNumber,
                WalletBalance = user.WalletBalance,
                PointsEarned = pointsEarned,
                RemainingPoints = user.Points
            });
        }

        [HttpGet("{orderNumber}")]
        //[Authorize]
        public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
        {
            var order = await _context.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(order);
        }

        [HttpGet("user/{userId}")]
        //[Authorize]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders.Include(o => o.Product).Where(o => o.UserId == userId).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("orders")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.Include(o => o.Product).Include(o => o.User).ToListAsync();
            return Ok(orders);
        }

        [HttpPut("{orderNumber}")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateOrder(string orderNumber, [FromBody] OrderDto orderDto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            order.DeliveryAddress = orderDto.DeliveryAddress;
            order.PaymentMethod = orderDto.PaymentMethod;

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpDelete("{orderNumber}")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOrder(string orderNumber)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order deleted successfully" });
        }
    }
}
