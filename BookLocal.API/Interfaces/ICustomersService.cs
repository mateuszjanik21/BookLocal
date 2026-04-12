using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface ICustomersService
    {
        Task<(bool Success, PagedResultDto<CustomerListItemDto>? Data)> GetCustomersAsync(
            int businessId, ClaimsPrincipal user, string? search, CustomerStatusFilter status, 
            CustomerHistoryFilter history, CustomerSpentFilter spent, int page, int pageSize);
            
        Task<(bool Success, CustomerDetailDto? Data, string? ErrorMessage)> GetCustomerDetailsAsync(
            int businessId, string customerId, ClaimsPrincipal user);
            
        Task<(bool Success, PagedResultDto<ReservationHistoryDto>? Data)> GetCustomerHistoryAsync(
            int businessId, string customerId, ClaimsPrincipal user, int page, int pageSize);
            
        Task<bool> UpdateNotesAsync(int businessId, string customerId, UpdateCustomerNoteDto dto, ClaimsPrincipal user);
        
        Task<bool> UpdateStatusAsync(int businessId, string customerId, UpdateCustomerStatusDto dto, ClaimsPrincipal user);
    }
}
