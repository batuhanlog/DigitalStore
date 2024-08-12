using DigitalStore.Core.Models;
using DigitalStore.Repository.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CouponsController(ApplicationDbContext context)
        {
            this.context = context;
        }

       
        [HttpPost]
        public IActionResult CreateCoupon([FromBody] Coupon coupon)
        {
            coupon.Code = Guid.NewGuid().ToString().Substring(0, 10);
            context.Coupons.Add(coupon);
            context.SaveChanges();
            return Ok(coupon);
        }

        [HttpGet]
        public IActionResult GetCoupons()
        {
            var coupons = context.Coupons.ToList();
            return Ok(coupons);
        }

        
        [HttpDelete("{id}")]
        public IActionResult DeleteCoupon(int id)
        {
            var coupon = context.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }

            context.Coupons.Remove(coupon);
            context.SaveChanges();
            return Ok();
        }

        [HttpPost("apply")]
        public IActionResult ApplyCoupon(string code)
        {
            var coupon = context.Coupons.FirstOrDefault(c => c.Code == code && !c.IsUsed && c.ExpiryDate > DateTime.Now);
            if (coupon == null)
            {
                return BadRequest("Invalid or expired coupon");
            }

            coupon.IsUsed = true;
            context.SaveChanges();

            return Ok(coupon);
        }
    }
}
