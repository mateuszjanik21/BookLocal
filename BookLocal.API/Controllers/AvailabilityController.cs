using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId}/availability")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSlots(int employeeId, [FromQuery] DateTime date, [FromQuery] int serviceVariantId)
        {
            var result = await _availabilityService.GetAvailableSlotsAsync(employeeId, date, serviceVariantId);

            if (!result.Success) return BadRequest(result.ErrorMessage);

            return Ok(result.Slots);
        }

        [HttpGet("bundle")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetBundleAvailableSlots(int employeeId, [FromQuery] DateTime date, [FromQuery] int bundleId)
        {
            var result = await _availabilityService.GetBundleAvailableSlotsAsync(employeeId, date, bundleId);

            if (!result.Success) return BadRequest(result.ErrorMessage);

            return Ok(result.Slots);
        }
    }
}