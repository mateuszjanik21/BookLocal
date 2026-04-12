using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/invoices")]
    [Authorize(Roles = "owner")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoicesService _invoicesService;

        public InvoicesController(IInvoicesService invoicesService)
        {
            _invoicesService = invoicesService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<InvoiceDto>> GenerateInvoice(int businessId, [FromBody] CreateReservationInvoiceDto dto)
        {
            var result = await _invoicesService.GenerateInvoiceAsync(businessId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage!.Contains("nie istnieje")) return NotFound(result.ErrorMessage);
                if (result.ErrorMessage.Contains("już wystawiona")) return Conflict(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<InvoiceDto>>> GetInvoices(
            int businessId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? search = null,
            [FromQuery] string? month = null)
        {
            var result = await _invoicesService.GetInvoicesAsync(businessId, page, pageSize, search, month, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }
    }
}
