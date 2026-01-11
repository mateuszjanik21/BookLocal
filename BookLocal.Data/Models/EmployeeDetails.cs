using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class EmployeeDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? Hobbies { get; set; }

        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }
    }
}