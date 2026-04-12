using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/bundles")]
    public class ServiceBundlesController : ControllerBase
    {
        private readonly IServiceBundlesService _serviceBundlesService;

        public ServiceBundlesController(IServiceBundlesService serviceBundlesService)
        {
            _serviceBundlesService = serviceBundlesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceBundleDto>>> GetBundles(int businessId)
        {
            var result = await _serviceBundlesService.GetBundlesAsync(businessId, User);
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceBundleDto>> GetBundle(int businessId, int id)
        {
            var result = await _serviceBundlesService.GetBundleAsync(businessId, id);

            if (!result.Success) return NotFound();

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<ServiceBundleDto>> CreateBundle(int businessId, CreateServiceBundleDto dto)
        {
            var result = await _serviceBundlesService.CreateBundleAsync(businessId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetBundle), new { businessId, id = result.Data!.ServiceBundleId }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<ServiceBundleDto>> UpdateBundle(int businessId, int id, CreateServiceBundleDto dto)
        {
            var result = await _serviceBundlesService.UpdateBundleAsync(businessId, id, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage == "Nie znaleziono pakietu.") return NotFound();
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteBundle(int businessId, int id)
        {
            var result = await _serviceBundlesService.DeleteBundleAsync(businessId, id, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound();
            }

            return NoContent();
        }
    }
}
