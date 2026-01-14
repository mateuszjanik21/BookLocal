using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        public int InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceNet { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VatRate { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossValue { get; set; }
    }
}
