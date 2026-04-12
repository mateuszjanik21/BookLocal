namespace BookLocal.API.Interfaces
{
    public interface IRolesService
    {
        Task<(bool Success, string? Message)> SetupRolesAsync();
    }
}
