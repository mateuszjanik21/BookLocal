using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class ServiceBundle
    {
        [Key]
        public int ServiceBundleId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ServiceBundleItem> BundleItems { get; set; } = new List<ServiceBundleItem>();
    }
}