using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public ChatHub(AppDbContext context, UserManager<User> userManager, IHubContext<PresenceHub> presenceHub)
        {
            _context = context;
            _userManager = userManager;
            _presenceHub = presenceHub;
        }

        public async Task SendMessage(int conversationId, string messageContent)
        {
            var senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversation = await _context.Conversations
                .Include(c => c.Business)
                .Include(c => c.Customer)
                .ThenInclude(cust => cust.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || (conversation.CustomerId != senderId && conversation.Business.OwnerId != senderId)) return;

            var sender = await _userManager.FindByIdAsync(senderId);
            if (sender == null) return;

            var message = new Message
            {
                Content = messageContent,
                SentAt = DateTime.UtcNow,
                ConversationId = conversationId,
                SenderId = senderId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var senderName = senderId == conversation.Business.OwnerId
                ? conversation.Business.Name
                : sender.FirstName + " " + sender.LastName;

            var messageDto = new MessageDto
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderId = message.SenderId,
                SenderFullName = senderName,
                SenderPhotoUrl = sender.PhotoUrl,
                IsRead = message.IsRead
            };
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", messageDto);

            var recipientId = senderId == conversation.CustomerId ? conversation.Business.OwnerId : conversation.CustomerId;

            if (!string.IsNullOrEmpty(recipientId))
            {
                await Clients.User(recipientId).SendAsync("ReceiveGlobalMessage", messageDto);

                var unreadCountForRecipient = await _context.Messages
                    .CountAsync(m => m.ConversationId == conversationId && m.SenderId != recipientId && !m.IsRead);

                var updatedConvoForRecipient = new ConversationDto
                {
                    ConversationId = conversation.ConversationId,
                    ParticipantId = senderId,
                    ParticipantName = senderName,
                    ParticipantPhotoUrl = sender.PhotoUrl,
                    LastMessage = message.Content,
                    LastMessageAt = message.SentAt,
                    UnreadCount = unreadCountForRecipient
                };

                await _presenceHub.Clients.User(recipientId).SendAsync("UpdateConversation", updatedConvoForRecipient);
            }
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isParticipant = await _context.Conversations
                .Include(c => c.Business)
                .AnyAsync(c => c.ConversationId == conversationId &&
                              (c.CustomerId == userId || c.Business.OwnerId == userId));

            if (isParticipant)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkMessagesAsRead(int conversationId)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isParticipant = await _context.Conversations
                .Include(c => c.Business)
                .AnyAsync(c => c.ConversationId == conversationId &&
                              (c.CustomerId == userId || c.Business.OwnerId == userId));

            if (!isParticipant) return;

            var messagesToUpdate = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            if (messagesToUpdate.Any())
            {
                foreach (var message in messagesToUpdate)
                {
                    message.IsRead = true;
                }
                await _context.SaveChangesAsync();

                await Clients.Group(conversationId.ToString()).SendAsync("MessagesRead", conversationId);

                var conversation = await _context.Conversations
                    .Include(c => c.Business)
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

                if (conversation != null)
                {
                    var ownerId = conversation.Business.OwnerId;
                    var customerId = conversation.CustomerId;
                    var lastMessage = await _context.Messages
                        .Where(m => m.ConversationId == conversationId)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefaultAsync();

                    var unreadForOwner = await _context.Messages
                        .CountAsync(m => m.ConversationId == conversationId && m.SenderId != ownerId && !m.IsRead);

                    var unreadForCustomer = await _context.Messages
                        .CountAsync(m => m.ConversationId == conversationId && m.SenderId != customerId && !m.IsRead);

                    var lastMessageAtUtc = lastMessage != null
                        ? DateTime.SpecifyKind(lastMessage.SentAt, DateTimeKind.Utc)
                        : (DateTime?)null;

                    var convoForOwner = new ConversationDto
                    {
                        ConversationId = conversation.ConversationId,
                        ParticipantId = customerId,
                        ParticipantName = conversation.Customer.FirstName + " " + conversation.Customer.LastName,
                        ParticipantPhotoUrl = conversation.Customer.PhotoUrl,
                        LastMessage = lastMessage?.Content ?? "Brak wiadomości",
                        LastMessageAt = lastMessageAtUtc,
                        UnreadCount = unreadForOwner
                    };
                    await _presenceHub.Clients.User(ownerId).SendAsync("UpdateConversation", convoForOwner);

                    var convoForCustomer = new ConversationDto
                    {
                        ConversationId = conversation.ConversationId,
                        ParticipantId = ownerId,
                        ParticipantName = conversation.Business.Name,
                        ParticipantPhotoUrl = conversation.Business.PhotoUrl,
                        LastMessage = lastMessage?.Content ?? "Brak wiadomości",
                        LastMessageAt = lastMessageAtUtc,
                        UnreadCount = unreadForCustomer
                    };
                    await _presenceHub.Clients.User(customerId).SendAsync("UpdateConversation", convoForCustomer);
                }
            }
        }
    }
}