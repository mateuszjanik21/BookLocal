using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupRoles()
    {
        if (!await _roleManager.RoleExistsAsync("customer"))
        {
            await _roleManager.CreateAsync(new IdentityRole("customer"));
        }
        if (!await _roleManager.RoleExistsAsync("owner"))
        {
            await _roleManager.CreateAsync(new IdentityRole("owner"));
        }
        return Ok("Role zostały skonfigurowane.");
    }
}