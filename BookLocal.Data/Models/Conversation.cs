using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        public required string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }
        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}