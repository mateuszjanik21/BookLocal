using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationsService _reservationsService;

        public ReservationsController(IReservationsService reservationsService)
        {
            _reservationsService = reservationsService;
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateReservation(ReservationCreateDto reservationDto)
        {
            var result = await _reservationsService.CreateReservationAsync(reservationDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Nie można zidentyfikować użytkownika na podstawie tokenu.") return Unauthorized(result.ErrorMessage);
                if (result.ErrorMessage!.Contains("nie istnieje")) return NotFound(result.ErrorMessage);
                if (result.ErrorMessage.Contains("nie pracuje")) return BadRequest(result.ErrorMessage);
                if (result.ErrorMessage.Contains("już zajęty")) return Conflict(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpGet("my-stats")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<CustomerStatsDto>> GetMyStats()
        {
            var result = await _reservationsService.GetMyStatsAsync(User);

            if (!result.Success) return Unauthorized();

            return Ok(result.Data);
        }

        [HttpGet("my-reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations(
            [FromQuery] string scope = "upcoming",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reservationsService.GetMyReservationsAsync(scope, pageNumber, pageSize, User);
            return Ok(result.Data);
        }

        [HttpGet("calendar")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetCalendarEvents(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int? employeeId = null)
        {
            var result = await _reservationsService.GetCalendarEventsAsync(start, end, employeeId, User);
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
        {
            var result = await _reservationsService.GetReservationByIdAsync(id, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] UpdateReservationStatusDto statusDto)
        {
            var result = await _reservationsService.UpdateReservationStatusAsync(id, statusDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage!.Contains("Nie znaleziono")) return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpPatch("my-reservations/{id}/cancel")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var result = await _reservationsService.CancelReservationAsync(id, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Unauthorized") return Unauthorized();
                if (result.ErrorMessage!.Contains("Nie znaleziono")) return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpPost("bundle")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateBundleReservation([FromBody] BundleReservationCreateDto reservationDto)
        {
            var result = await _reservationsService.CreateBundleReservationAsync(reservationDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Unauthorized") return Unauthorized();
                if (result.ErrorMessage!.Contains("już zajęty")) return Conflict(result.ErrorMessage);
                if (result.ErrorMessage.Contains("błąd")) return BadRequest(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpPost("dashboard/reservations")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> CreateReservationAsOwner([FromBody] OwnerCreateReservationDto reservationDto)
        {
            var result = await _reservationsService.CreateReservationAsOwnerAsync(reservationDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage!.Contains("już zajęty")) return Conflict(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpPost("dashboard/reservations/bundle")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> CreateBundleReservationAsOwner([FromBody] OwnerCreateBundleReservationDto reservationDto)
        {
            var result = await _reservationsService.CreateBundleReservationAsOwnerAsync(reservationDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage!.Contains("już zajęty")) return Conflict(result.ErrorMessage);
                if (result.ErrorMessage.Contains("błąd")) return BadRequest(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpGet("{id}/adjacent")]
        public async Task<IActionResult> GetAdjacentReservations(int id)
        {
            var result = await _reservationsService.GetAdjacentReservationsAsync(id, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }
    }
}