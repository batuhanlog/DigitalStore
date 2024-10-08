﻿using DigitalStore.Core.DTOs;
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

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.Include(c => c.ProductCategories)
                                                      .ThenInclude(pc => pc.Product)
                                                      .ToListAsync();
            return Ok(categories);
        }
     
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

        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromQuery] string token, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    throw new InvalidTokenException();
                }

                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if ((emailClaim != null && emailClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)) ||
                    (nameClaim != null && nameClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)))
                {
                }
                else
                {
                    throw new UnauthorizedActionException();
                }
                if (string.IsNullOrEmpty(categoryDto.Name) || string.IsNullOrEmpty(categoryDto.Url) || string.IsNullOrEmpty(categoryDto.Tags))
                {
                    return BadRequest("Name, Url ve Tags alanları gereklidir.");
                }
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
              
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                
                throw new InternalServerErrorException();  
            }
        }
        //[Authorize(Roles = "admin")]
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
        //[Authorize(Roles = "admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory([FromQuery] int id, [FromQuery] string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    throw new InvalidTokenException();
                }
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if ((emailClaim != null && emailClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)) ||
                    (nameClaim != null && nameClaim.Contains("admin", StringComparison.OrdinalIgnoreCase)))
                {
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
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException("Kategori silinirken bir hata oluştu.", ex);
            }
        }


    }
}
