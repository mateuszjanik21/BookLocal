using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class ServiceCategory
    {
        [Key]
        public int ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public string? PhotoUrl { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public int MainCategoryId { get; set; }

        [ForeignKey("MainCategoryId")]
        public virtual MainCategory MainCategory { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}