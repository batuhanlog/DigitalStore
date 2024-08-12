using DigitalStore.Core.Models;
using DigitalStore.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalStore.Service.Infrastructure
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailSender _emailSender;
        private readonly TokenHandler _tokenHandler;

        public AccountController(IConfiguration configuration,
                                UserManager<User> userManager,
                                SignInManager<User> signInManager,
                                EmailSender emailSender,
                                TokenHandler tokenHandler)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _tokenHandler = tokenHandler;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            var user = new User
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PhoneNumber = userDto.Phone,
                Address = userDto.Address,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            var role = userDto.Role.ToLower() == "admin" ? "admin" : "client";
            await _userManager.AddToRoleAsync(user, role);

            var jwt = _tokenHandler.CreateAccessToken(user);
            var userProfileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Address,
                Role = role,
                CreatedAt = user.CreatedAt
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Error", "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Error", "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            var jwt = _tokenHandler.CreateAccessToken(user);

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
                CreatedAt = user.CreatedAt
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found. Please check the email address." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string emailSubject = "Password Reset";
            string username = user.FirstName + " " + user.LastName;
            string emailMessage = "Dear " + username + "\n" +
                                  "We received your password reset request.\n" +
                                  "Please use the following token to reset your password:\n" +
                                  token + "\n\n" +
                                  "Best Regards\n";

            await _emailSender.SendEmail(emailSubject, email, username, emailMessage);

            return Ok(new { Message = "Password reset link has been sent to your email.", Token = token });
        }

        //[Authorize]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string email, string token, string Newpassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Token", "Invalid token.");
                return BadRequest(ModelState);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, Newpassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("The password was successfully changed ✓ ");
        }
        //[Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required.");
            }

            // Token'ı çöz ve doğrula
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid token.");
            }

           
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            
            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return Unauthorized("User not found.");
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
                WalletBalance = user.WalletBalance, // WalletBalance'ı doğrudan al
                Points = user.Points
            };

            return Ok(userProfileDto);
        }
        //[Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(string token, UserProfileUpdateDto userProfileUpdateDto)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required.");
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid token.");
            }

           
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return Unauthorized();
            }

            user.FirstName = userProfileUpdateDto.FirstName;
            user.LastName = userProfileUpdateDto.LastName;
            user.Email = userProfileUpdateDto.Email;
            user.PhoneNumber = userProfileUpdateDto.Phone ?? "";
            user.Address = userProfileUpdateDto.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
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
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }

        //[Authorize]
        [HttpPut("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(string token, [Required, MinLength(8), MaxLength(100)] string password)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is required.");
            }

          
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid token.");
            }
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, password, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok();
        }

    }
}
