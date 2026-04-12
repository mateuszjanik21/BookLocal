using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IPaymentsService
    {
        Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> CreatePaymentAsync(CreatePaymentDto paymentDto, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage, int StatusCode)> GetBusinessPaymentsAsync(int businessId, int page, int pageSize, string? sort, string? sortDir, string? methodFilter, string? statusFilter, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<PaymentDto>? Data, string? ErrorMessage, int StatusCode)> GetReservationPaymentsAsync(int reservationId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> UpdatePaymentAsync(int id, UpdatePaymentDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> DeletePaymentAsync(int id, ClaimsPrincipal user);
    }
}
