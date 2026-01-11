using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public int ServiceCategoryId { get; set; }
        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; } = false;

        public virtual ICollection<ServiceVariant> Variants { get; set; } = new List<ServiceVariant>();
    }
}
