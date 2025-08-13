using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class MainCategory
    {
        [Key]
        public int MainCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public virtual ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
    }
}