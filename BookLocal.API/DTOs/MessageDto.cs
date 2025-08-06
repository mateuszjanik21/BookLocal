namespace BookLocal.API.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public required string Content { get; set; }
        public DateTime SentAt { get; set; }
        public required string SenderId { get; set; }
        public required string SenderFullName { get; set; }
        public string? SenderPhotoUrl { get; set; }
        public bool IsRead { get; set; }
    }
}
