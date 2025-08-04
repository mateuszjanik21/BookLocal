using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public required string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [Required]
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [Required]
        public required string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
    }
}