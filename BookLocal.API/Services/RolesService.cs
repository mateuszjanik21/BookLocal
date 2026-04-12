using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BookLocal.API.Services
{
    public class RolesService : IRolesService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<(bool Success, string? Message)> SetupRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("customer"));
            }
            if (!await _roleManager.RoleExistsAsync("owner"))
            {
                await _roleManager.CreateAsync(new IdentityRole("owner"));
            }
            return (true, "Role zostały skonfigurowane.");
        }
    }
}
