using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessesController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessesController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BusinessSearchResultDto>>> GetAllBusinesses([FromQuery] string? searchQuery)
        {
            var businesses = await _businessService.GetAllBusinessesAsync(searchQuery);
            return Ok(businesses);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BusinessDetailDto>> GetBusiness(int id)
        {
            var businessDto = await _businessService.GetBusinessAsync(id);

            if (businessDto == null) return NotFound();

            return Ok(businessDto);
        }

        [HttpGet("my-business")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<BusinessDetailDto>> GetMyBusiness()
        {
            var result = await _businessService.GetMyBusinessAsync(User);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateBusiness(int id, BusinessDto businessDto)
        {
            var success = await _businessService.UpdateBusinessAsync(id, businessDto, User);

            if (!success) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var success = await _businessService.DeleteBusinessAsync(id, User);

            if (!success) return NotFound();

            return NoContent();
        }

        [HttpGet("dashboard-data")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<DashboardDataDto>> GetDashboardData()
        {
            var data = await _businessService.GetDashboardDataAsync(User);
            return Ok(data);
        }
    }
}