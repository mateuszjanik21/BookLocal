using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [Route("api/businesses/{businessId}/loyalty")]
    [ApiController]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("config")]
        public async Task<ActionResult<LoyaltyConfigDto>> GetConfig(int businessId)
        {
            var result = await _loyaltyService.GetConfigAsync(businessId);
            return Ok(result.Data);
        }

        [HttpPut("config")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateConfig(int businessId, [FromBody] LoyaltyConfigDto dto)
        {
            var result = await _loyaltyService.UpdateConfigAsync(businessId, dto);
            return Ok(result.Data);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<LoyaltyStatsDto>> GetStats([FromRoute] int businessId)
        {
            var result = await _loyaltyService.GetStatsAsync(businessId);
            return Ok(result.Data);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<object>> GetCustomerLoyalty(int businessId, string customerId)
        {
            var result = await _loyaltyService.GetCustomerLoyaltyAsync(businessId, customerId);
            return Ok(result.Data);
        }

        [HttpPost("recalculate")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> RecalculatePoints(int businessId)
        {
            var result = await _loyaltyService.RecalculatePointsAsync(businessId);

            if (!result.Success) return BadRequest(result.ErrorMessage);

            return Ok(new { result.Message });
        }
    }
}
