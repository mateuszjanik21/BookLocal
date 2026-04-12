using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPublicPlans()
        {
            var result = await _subscriptionService.GetPublicPlansAsync();
            return Ok(result.Data);
        }

        [HttpPost("subscribe")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> Subscribe([FromBody] int planId)
        {
            var result = await _subscriptionService.SubscribeAsync(planId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("Nie znaleziono firmy")) return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpGet("current")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<object>> GetCurrentSubscription()
        {
            var result = await _subscriptionService.GetCurrentSubscriptionAsync(User);

            if (!result.Success) return NotFound();

            return Ok(result.Data);
        }
    }
}
