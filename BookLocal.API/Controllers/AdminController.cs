using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "superadmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans()
        {
            var dtos = await _adminService.GetPlansAsync();
            return Ok(dtos);
        }

        [HttpPost("plans")]
        public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(CreateSubscriptionPlanDto dto)
        {
            var result = await _adminService.CreatePlanAsync(dto);
            return CreatedAtAction(nameof(GetPlans), new { }, result);
        }

        [HttpPut("plans/{id}")]
        public async Task<IActionResult> UpdatePlan(int id, CreateSubscriptionPlanDto dto)
        {
            var success = await _adminService.UpdatePlanAsync(id, dto);
            if (!success) return NotFound("Nie znaleziono planu.");

            return NoContent();
        }

        [HttpDelete("plans/{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var success = await _adminService.DeletePlanAsync(id);
            if (!success) return NotFound("Nie znaleziono planu.");

            return NoContent();
        }

        [HttpGet("businesses")]
        public async Task<ActionResult<IEnumerable<AdminBusinessListDto>>> GetBusinesses([FromQuery] string? status = null)
        {
            var dtos = await _adminService.GetBusinessesAsync(status);
            return Ok(dtos);
        }

        [HttpPatch("businesses/{id}/verify")]
        public async Task<IActionResult> VerifyBusiness(int id, [FromBody] VerifyBusinessDto dto)
        {
            var result = await _adminService.VerifyBusinessAsync(id, dto);
            if (!result.Success) return NotFound(result.Message);

            return Ok(new { result.Message });
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var stats = await _adminService.GetStatsAsync();
            return Ok(stats);
        }
    }
}
