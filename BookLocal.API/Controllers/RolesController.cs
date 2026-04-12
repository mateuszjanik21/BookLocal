using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpPost("setup")]
        public async Task<IActionResult> SetupRoles()
        {
            var result = await _rolesService.SetupRolesAsync();

            return Ok(result.Message);
        }
    }
}