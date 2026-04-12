using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;

        public MessagesController(IMessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        [HttpPost("start")]
        public async Task<ActionResult<ConversationDto>> StartConversation(StartConversationDto startDto)
        {
            var result = await _messagesService.StartConversationAsync(startDto.BusinessId, User);
            return Ok(result.Data);
        }

        [HttpGet("my-conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetMyConversations()
        {
            var result = await _messagesService.GetMyConversationsAsync(User);
            return Ok(result.Data);
        }

        [HttpGet("{conversationId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int conversationId)
        {
            var result = await _messagesService.GetMessagesAsync(conversationId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPost("start-as-owner")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<object>> StartConversationAsOwner(StartConversationAsOwnerDto startDto)
        {
            var result = await _messagesService.StartConversationAsOwnerAsync(startDto.CustomerId, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetTotalUnreadCount()
        {
            var result = await _messagesService.GetTotalUnreadCountAsync(User);

            if (!result.Success) return Unauthorized();

            return Ok(result.Count);
        }
    }
}