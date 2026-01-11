using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class EmployeeCertificate
    {
        [Key]
        public int CertificateId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Institution { get; set; }

        public DateOnly DateObtained { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsVisibleToClient { get; set; } = true;
    }
}