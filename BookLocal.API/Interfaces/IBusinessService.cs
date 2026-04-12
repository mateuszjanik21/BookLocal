using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IBusinessService
    {
        Task<IEnumerable<BusinessSearchResultDto>> GetAllBusinessesAsync(string? searchQuery);
        Task<BusinessDetailDto?> GetBusinessAsync(int id);
        Task<(bool Success, BusinessDetailDto? Data, string? ErrorMessage)> GetMyBusinessAsync(ClaimsPrincipal user);
        Task<bool> UpdateBusinessAsync(int id, BusinessDto businessDto, ClaimsPrincipal user);
        Task<bool> DeleteBusinessAsync(int id, ClaimsPrincipal user);
        Task<DashboardDataDto> GetDashboardDataAsync(ClaimsPrincipal user);
    }
}
