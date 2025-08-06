using BookLocal.API.DTOs;
using BookLocal.Data;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public MessagesController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("start")]
        public async Task<ActionResult<ConversationDto>> StartConversation(StartConversationDto startDto)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.BusinessId == startDto.BusinessId && c.CustomerId == customerId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    BusinessId = startDto.BusinessId,
                    CustomerId = customerId
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return Ok(new { conversationId = conversation.ConversationId });
        }

        [HttpGet("my-conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetMyConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

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
                .Include(c => c.Messages)
                .Select(c => new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    ParticipantId = userRoles.Contains("owner") ? c.CustomerId : c.Business.OwnerId,
                    ParticipantName = userRoles.Contains("owner") ? (c.Customer.FirstName + " " + c.Customer.LastName) : c.Business.Name,
                    ParticipantPhotoUrl = userRoles.Contains("owner") ? c.Customer.PhotoUrl : c.Business.PhotoUrl,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault() != null ? c.Messages.OrderByDescending(m => m.SentAt).First().Content : "Brak wiadomości",
                    LastMessageAt = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault() != null ? c.Messages.OrderByDescending(m => m.SentAt).First().SentAt : DateTime.MinValue,
                    UnreadCount = c.Messages.Count(m => m.SenderId != userId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return Ok(conversations);
        }

        [HttpGet("{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = await _context.Conversations
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null || (conversation.CustomerId != userId && conversation.Business.OwnerId != userId))
            {
                return Forbid();
            }

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    ConversationId = m.ConversationId,
                    Content = m.Content,
                    SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc),
                    SenderId = m.SenderId,
                    SenderFullName = m.SenderId == conversation.Business.OwnerId
                        ? conversation.Business.Name
                        : m.Sender.FirstName + " " + m.Sender.LastName,
                    SenderPhotoUrl = m.Sender.PhotoUrl,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("start-as-owner")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<object>> StartConversationAsOwner(StartConversationAsOwnerDto startDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);

            if (business == null)
            {
                return Forbid();
            }

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.BusinessId == business.BusinessId && c.CustomerId == startDto.CustomerId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    BusinessId = business.BusinessId,
                    CustomerId = startDto.CustomerId
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return Ok(new { conversationId = conversation.ConversationId });
        }
    }
}