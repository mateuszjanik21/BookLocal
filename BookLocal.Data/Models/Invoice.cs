using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public Business Business { get; set; }

        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }

        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public User Customer { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime SaleDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNet { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGross { get; set; }

        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}