using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/[controller]")]
    [Authorize(Roles = "owner")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersService _customersService;

        public CustomersController(ICustomersService customersService)
        {
            _customersService = customersService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<CustomerListItemDto>>> GetCustomers(
            int businessId,
            [FromQuery] string? search,
            [FromQuery] CustomerStatusFilter status = CustomerStatusFilter.All,
            [FromQuery] CustomerHistoryFilter history = CustomerHistoryFilter.All,
            [FromQuery] CustomerSpentFilter spent = CustomerSpentFilter.All,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _customersService.GetCustomersAsync(businessId, User, search, status, history, spent, page, pageSize);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerDetails(int businessId, string customerId)
        {
            var result = await _customersService.GetCustomerDetailsAsync(businessId, customerId, User);

            if (!result.Success) return Forbid();
            if (result.Data == null) return NotFound(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpGet("{customerId}/history")]
        public async Task<ActionResult<PagedResultDto<ReservationHistoryDto>>> GetCustomerHistory(
            int businessId,
            string customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _customersService.GetCustomerHistoryAsync(businessId, customerId, User, page, pageSize);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPut("{customerId}/notes")]
        public async Task<IActionResult> UpdateNotes(int businessId, string customerId, UpdateCustomerNoteDto dto)
        {
            var success = await _customersService.UpdateNotesAsync(businessId, customerId, dto, User);

            if (!success) return NotFound();

            return NoContent();
        }

        [HttpPut("{customerId}/status")]
        public async Task<IActionResult> UpdateStatus(int businessId, string customerId, UpdateCustomerStatusDto dto)
        {
            var success = await _customersService.UpdateStatusAsync(businessId, customerId, dto, User);

            if (!success) return NotFound();

            return NoContent();
        }
    }
}
