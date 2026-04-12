using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "owner")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpPost("recalculate-stats")]
        public async Task<IActionResult> RecalculateCustomerStats()
        {
            var result = await _maintenanceService.RecalculateCustomerStatsAsync();
            return Ok(new { result.Message });
        }
    }
}
