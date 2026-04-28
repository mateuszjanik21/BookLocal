using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly AppDbContext _context;

        public MessagesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> StartConversationAsync(int businessId, ClaimsPrincipal user)
        {
            var customerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.CustomerId == customerId);

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        BusinessId = businessId,
                        CustomerId = customerId ?? ""
                    };
                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, new { conversationId = conversation.ConversationId }, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(bool Success, IEnumerable<ConversationDto>? Data)> GetMyConversationsAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            IQueryable<Conversation> query;

            if (userRoles.Contains("owner"))
            {
                query = _context.Conversations.Where(c => c.Business.OwnerId == userId);
            }
            else
            {
                query = _context.Conversations.Where(c => c.CustomerId == userId);
            }

            var conversations = await query
                .Include(c => c.Business)
                .Include(c => c.Customer)
                .Select(c => new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    ParticipantId = userRoles.Contains("owner") ? c.CustomerId : c.Business.OwnerId ?? "",
                    ParticipantName = userRoles.Contains("owner") ? (c.Customer != null ? c.Customer.FirstName + " " + c.Customer.LastName : "Nieznany") : c.Business.Name ?? "",
                    ParticipantPhotoUrl = userRoles.Contains("owner") ? (c.Customer != null ? c.Customer.PhotoUrl : null) : c.Business.PhotoUrl,

                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault() != null
                        ? c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()!.Content
                        : "Brak wiadomości",
                    LastMessageAt = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault() != null
                        ? (DateTime?)c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()!.SentAt
                        : null,

                    UnreadCount = c.Messages.Count(m => m.SenderId != userId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            foreach (var c in conversations)
            {
                if (c.LastMessageAt.HasValue)
                {
                    c.LastMessageAt = DateTime.SpecifyKind(c.LastMessageAt.Value, DateTimeKind.Utc);
                }
            }

            return (true, conversations);
        }

        public async Task<(bool Success, IEnumerable<MessageDto>? Data, string? ErrorMessage, int StatusCode)> GetMessagesAsync(int conversationId, int pageNumber, int pageSize, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = await _context.Conversations
                .AsNoTracking()
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null) return (false, null, "Nie znaleziono konwersacji.", 404);

            if (conversation.CustomerId != userId && conversation.Business.OwnerId != userId)
            {
                return (false, null, "Brak uprawnień.", 403);
            }

            var messagesQuery = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    ConversationId = m.ConversationId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    SenderId = m.SenderId,

                    SenderFullName = m.SenderId == conversation.Business.OwnerId
                        ? conversation.Business.Name ?? ""
                        : (m.Sender != null ? m.Sender.FirstName + " " + m.Sender.LastName : "Nieznany"),

                    SenderPhotoUrl = m.SenderId == conversation.Business.OwnerId
                        ? conversation.Business.PhotoUrl
                        : (m.Sender != null ? m.Sender.PhotoUrl : null),

                    IsRead = m.IsRead
                })
                .ToListAsync();

            // Reverse the order back to chronological for correct display in the chat window
            var messages = messagesQuery.OrderBy(m => m.SentAt).ToList();

            foreach (var m in messages)
            {
                m.SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc);
            }

            return (true, messages, null, 200);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> StartConversationAsOwnerAsync(string customerId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);

            if (business == null)
            {
                return (false, null, "Brak uprawnień.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.BusinessId == business.BusinessId && c.CustomerId == customerId);

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        BusinessId = business.BusinessId,
                        CustomerId = customerId
                    };
                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, new { conversationId = conversation.ConversationId }, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(bool Success, int Count, string? ErrorMessage)> GetTotalUnreadCountAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, 0, "Unauthorized");

            var count = await _context.Messages
                .CountAsync(m => m.ConversationId != 0 &&
                                 (m.Conversation.CustomerId == userId || m.Conversation.Business.OwnerId == userId) &&
                                 m.SenderId != userId &&
                                 !m.IsRead);

            return (true, count, null);
        }
    }
}
