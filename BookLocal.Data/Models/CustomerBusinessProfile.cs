using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class CustomerBusinessProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        public string? PrivateNotes { get; set; }
        public string? Allergies { get; set; }
        public string? Formulas { get; set; }

        public bool IsVIP { get; set; } = false;
        public bool IsBanned { get; set; } = false;

        public int NoShowCount { get; set; }
        public decimal TotalSpent { get; set; }

        public DateTime LastVisitDate { get; set; }
    }
}