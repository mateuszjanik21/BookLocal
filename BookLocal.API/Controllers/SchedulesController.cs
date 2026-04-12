using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "owner")]
    public class SchedulesController : ControllerBase
    {
        private readonly ISchedulesService _schedulesService;

        public SchedulesController(ISchedulesService schedulesService)
        {
            _schedulesService = schedulesService;
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> GetSchedule(int employeeId)
        {
            var result = await _schedulesService.GetScheduleAsync(employeeId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("dostęp")) return Forbid();
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPut("{employeeId}")]
        public async Task<IActionResult> UpdateSchedule(int employeeId, [FromBody] List<WorkScheduleDto> schedulePayload)
        {
            var result = await _schedulesService.UpdateScheduleAsync(employeeId, schedulePayload, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage!.Contains("kolidujące")) return Conflict(new { message = result.ErrorMessage });
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }
    }
}