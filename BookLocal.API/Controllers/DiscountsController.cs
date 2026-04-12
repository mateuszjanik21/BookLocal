using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [Route("api/businesses/{businessId}/discounts")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountsService _discountsService;

        public DiscountsController(IDiscountsService discountsService)
        {
            _discountsService = discountsService;
        }

        [HttpGet]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<DiscountDto>>> GetDiscounts(int businessId)
        {
            var result = await _discountsService.GetDiscountsAsync(businessId, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<DiscountDto>> CreateDiscount(int businessId, CreateDiscountDto dto)
        {
            var result = await _discountsService.CreateDiscountAsync(businessId, dto, User);

            if (!result.Success) return Forbid();

            if (result.Data == null && result.ErrorMessage != null)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPut("{discountId}/toggle")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> ToggleDiscount(int businessId, int discountId)
        {
            var result = await _discountsService.ToggleDiscountAsync(businessId, discountId, User);

            if (!result.Success)
            {
                return NotFound();
            }

            return Ok(new { Message = "Status zaktualizowany", result.IsActive });
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<VerifyDiscountResult>> VerifyDiscount(int businessId, VerifyDiscountRequest request)
        {
            var result = await _discountsService.VerifyDiscountAsync(businessId, request);

            if (!result.Success || result.Data == null) return BadRequest("Wewnętrzny błąd weryfikacji.");

            return Ok(result.Data);
        }
    }
}
