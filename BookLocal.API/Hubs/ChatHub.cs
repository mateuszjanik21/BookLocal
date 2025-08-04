using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace BookLocal.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChatHub(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendMessage(int conversationId, string messageContent)
        {
            var senderId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversation = await _context.Conversations
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || (conversation.CustomerId != senderId && conversation.Business.OwnerId != senderId))
            {
                return;
            }

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
                IsRead = message.IsRead
            };

            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkMessagesAsRead(int conversationId)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messagesToUpdate = await _context.Messages
                .Where(m => m.ConversationId == conversationId &&
                            m.SenderId != userId &&
                            !m.IsRead)
                .ToListAsync();

            if (messagesToUpdate.Any())
            {
                foreach (var message in messagesToUpdate)
                {
                    message.IsRead = true;
                }
                await _context.SaveChangesAsync();

                await Clients.Group(conversationId.ToString()).SendAsync("MessagesRead", conversationId);
            }
        }
    }
}