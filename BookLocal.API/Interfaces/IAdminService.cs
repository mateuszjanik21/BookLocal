using BookLocal.API.DTOs;

namespace BookLocal.API.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync();
        Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto);
        Task<bool> UpdatePlanAsync(int id, CreateSubscriptionPlanDto dto);
        Task<bool> DeletePlanAsync(int id);
        Task<IEnumerable<AdminBusinessListDto>> GetBusinessesAsync(string? status);
        Task<(bool Success, string Message, string? RejectionReason)> VerifyBusinessAsync(int id, VerifyBusinessDto dto);
        Task<AdminStatsDto> GetStatsAsync();
    }
}
