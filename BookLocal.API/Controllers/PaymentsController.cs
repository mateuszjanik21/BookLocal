using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentsService _paymentsService;

        public PaymentsController(IPaymentsService paymentsService)
        {
            _paymentsService = paymentsService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto paymentDto)
        {
            var result = await _paymentsService.CreatePaymentAsync(paymentDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage == "Nie znaleziono rezerwacji.") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpGet("business/{businessId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> GetBusinessPayments(
            int businessId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? sort = null,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? methodFilter = null,
            [FromQuery] string? statusFilter = null)
        {
            var result = await _paymentsService.GetBusinessPaymentsAsync(businessId, page, pageSize, sort, sortDir, methodFilter, statusFilter, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetReservationPayments(int reservationId)
        {
            var result = await _paymentsService.GetReservationPaymentsAsync(reservationId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto dto)
        {
            var result = await _paymentsService.UpdatePaymentAsync(id, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var result = await _paymentsService.DeletePaymentAsync(id, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }
    }
}
