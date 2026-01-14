using BookLocal.API.Data;
using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [Route("api/businesses/{businessId}/discounts")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DiscountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/businesses/{businessId}/discounts
        [HttpGet]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<DiscountDto>>> GetDiscounts(int businessId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var discounts = await _context.Discounts
                .Where(d => d.BusinessId == businessId)
                .OrderByDescending(d => d.DiscountId)
                .Select(d => new DiscountDto
                {
                    DiscountId = d.DiscountId,
                    Code = d.Code,
                    Type = d.Type.ToString(),
                    Value = d.Value,
                    MaxUses = d.MaxUses,
                    UsedCount = d.UsedCount,
                    ValidFrom = d.ValidFrom,
                    ValidTo = d.ValidTo,
                    IsActive = d.IsActive,
                    ServiceId = d.ServiceId
                })
                .ToListAsync();

            return Ok(discounts);
        }

        // POST: api/businesses/{businessId}/discounts
        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<DiscountDto>> CreateDiscount(int businessId, CreateDiscountDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            if (await _context.Discounts.AnyAsync(d => d.BusinessId == businessId && d.Code == dto.Code && d.IsActive))
            {
                return BadRequest("Kod rabatowy o tej nazwie już istnieje.");
            }

            var discount = new Discount
            {
                BusinessId = businessId,
                Code = dto.Code.ToUpper(),
                Type = dto.Type,
                Value = dto.Value,
                MaxUses = dto.MaxUses,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                ServiceId = dto.ServiceId,
                IsActive = true
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return Ok(new DiscountDto
            {
                DiscountId = discount.DiscountId,
                Code = discount.Code,
                Type = discount.Type.ToString(),
                Value = discount.Value,
                MaxUses = discount.MaxUses,
                UsedCount = 0,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                ServiceId = discount.ServiceId,
                IsActive = true
            });
        }

        // PUT: api/businesses/{businessId}/discounts/{discountId}/toggle
        [HttpPut("{discountId}/toggle")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> ToggleDiscount(int businessId, int discountId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountId == discountId && d.BusinessId == businessId);
            if (discount == null) return NotFound();

            discount.IsActive = !discount.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Status zaktualizowany", IsActive = discount.IsActive });
        }

        // POST: api/businesses/{businessId}/discounts/verify
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<VerifyDiscountResult>> VerifyDiscount(int businessId, VerifyDiscountRequest request)
        {
            var code = request.Code?.Trim().ToUpper();
            if (string.IsNullOrEmpty(code)) return BadRequest("Kod jest wymagany.");

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.BusinessId == businessId && d.Code == code && d.IsActive);

            if (discount == null)
            {
                return Ok(new VerifyDiscountResult { IsValid = false, Message = "Kod nieprawidłowy lub nieaktywny." });
            }

            if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
            {
                return Ok(new VerifyDiscountResult { IsValid = false, Message = "Limit użycia kodu został wyczerpany." });
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (discount.ValidFrom.HasValue && today < discount.ValidFrom.Value)
            {
                return Ok(new VerifyDiscountResult { IsValid = false, Message = "Kod jeszcze nie jest aktywny." });
            }
            if (discount.ValidTo.HasValue && today > discount.ValidTo.Value)
            {
                return Ok(new VerifyDiscountResult { IsValid = false, Message = "Kod wygasł." });
            }

            if (discount.ServiceId.HasValue && request.ServiceId.HasValue && discount.ServiceId != request.ServiceId)
            {
                return Ok(new VerifyDiscountResult { IsValid = false, Message = "Kod nie dotyczy tej usługi." });
            }

            decimal discountAmount = 0;
            if (discount.Type == DiscountType.Percentage)
            {
                discountAmount = request.OriginalPrice * (discount.Value / 100m);
            }
            else
            {
                discountAmount = discount.Value;
            }

            if (discountAmount > request.OriginalPrice) discountAmount = request.OriginalPrice;

            return Ok(new VerifyDiscountResult
            {
                IsValid = true,
                DiscountId = discount.DiscountId,
                DiscountAmount = discountAmount,
                FinalPrice = request.OriginalPrice - discountAmount,
                Message = "Kod poprawny."
            });
        }
    }
}
