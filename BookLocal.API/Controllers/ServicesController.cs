using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesService _servicesService;

        public ServicesController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesForBusiness(int businessId)
        {
            var result = await _servicesService.GetServicesForBusinessAsync(businessId);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<ServiceDto>> AddService(int businessId, ServiceUpsertDto serviceDto)
        {
            var result = await _servicesService.AddServiceAsync(businessId, serviceDto, User);

            if (!result.Success) return Forbid();

            return CreatedAtAction(nameof(GetServicesForBusiness), new { businessId, id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{serviceId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateService(int businessId, int serviceId, ServiceUpsertDto serviceDto)
        {
            var result = await _servicesService.UpdateServiceAsync(businessId, serviceId, serviceDto, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("{serviceId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteService(int businessId, int serviceId)
        {
            var result = await _servicesService.DeleteServiceAsync(businessId, serviceId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("{serviceId}/variants/{variantId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteServiceVariant(int businessId, int serviceId, int variantId)
        {
            var result = await _servicesService.DeleteServiceVariantAsync(businessId, serviceId, variantId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpPatch("{serviceId}/restore")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> RestoreService(int businessId, int serviceId)
        {
            var result = await _servicesService.RestoreServiceAsync(businessId, serviceId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpPatch("{serviceId}/variants/{variantId}/restore")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> RestoreServiceVariant(int businessId, int serviceId, int variantId)
        {
            var result = await _servicesService.RestoreServiceVariantAsync(businessId, serviceId, variantId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }
    }
}