using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IInvoicesService
    {
        Task<(bool Success, InvoiceDto? Data, string? ErrorMessage, int StatusCode)> GenerateInvoiceAsync(int businessId, CreateReservationInvoiceDto dto, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> GetInvoicesAsync(int businessId, int page, int pageSize, string? search, string? month, ClaimsPrincipal user);
    }
}
