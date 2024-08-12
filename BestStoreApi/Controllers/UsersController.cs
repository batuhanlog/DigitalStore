using DigitalStore.Core.Models;
using DigitalStore.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        //[Authorize(Roles = "admin")]
        public IActionResult GetUsers(int? page, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            if (page == null || page < 1)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            var count = _userManager.Users.Count();
            totalPages = (int)Math.Ceiling(count / (decimal)pageSize);

            var users = _userManager.Users
                .OrderByDescending(u => u.Id)
                .Skip((int)(page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userProfiles = users.Select(user => new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Address,
                Role = "client",
                CreatedAt = user.CreatedAt,
                WalletBalance = user.WalletBalance,
                Points = user.Points
            }).ToList();

            var response = new
            {
                Users = userProfiles,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUser(int id, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userProfileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Address,
                Role = roles.FirstOrDefault(),
                CreatedAt = user.CreatedAt,
                WalletBalance = user.WalletBalance,
                Points = user.Points
            };

            return Ok(userProfileDto);
        }
        //[Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> DeleteUser(int id, [FromQuery] string token)
        {
            var userId = GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to delete user.");
            }

            return Ok("User deleted successfully.");
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
}
