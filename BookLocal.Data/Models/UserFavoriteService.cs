using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class UserFavoriteService
    {
        [Key]
        public int UserFavoriteServiceId { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int ServiceVariantId { get; set; }
        [ForeignKey("ServiceVariantId")]
        public virtual ServiceVariant ServiceVariant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
