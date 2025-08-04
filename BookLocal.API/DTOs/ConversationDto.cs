using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public required string ParticipantId { get; set; }
        public required string ParticipantName { get; set; }
        public required string LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
    }

    public class StartConversationDto
    {
        [Required]
        public int BusinessId { get; set; }
    }

    public class StartConversationAsOwnerDto
    {
        [Required]
        public required string CustomerId { get; set; }
    }
}
