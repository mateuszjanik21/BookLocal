using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public int PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }

        [Required]
        public string InvoiceNumber { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public string BuyerName { get; set; }
        public string? BuyerNIP { get; set; }
        public string BuyerAddress { get; set; }

        public string? PdfUrl { get; set; }
    }
}