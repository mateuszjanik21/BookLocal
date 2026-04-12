using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IMessagesService
    {
        Task<(bool Success, object? Data, string? ErrorMessage)> StartConversationAsync(int businessId, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<ConversationDto>? Data)> GetMyConversationsAsync(ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<MessageDto>? Data, string? ErrorMessage, int StatusCode)> GetMessagesAsync(int conversationId, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> StartConversationAsOwnerAsync(string customerId, ClaimsPrincipal user);
        Task<(bool Success, int Count, string? ErrorMessage)> GetTotalUnreadCountAsync(ClaimsPrincipal user);
    }
}
