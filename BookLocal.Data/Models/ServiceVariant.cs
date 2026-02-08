using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class ServiceVariant
    {
        [Key]
        public int ServiceVariantId { get; set; }

        [Required]
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public int CleanupTimeMinutes { get; set; } = 0;

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<UserFavoriteService> UserFavoriteServices { get; set; } = new List<UserFavoriteService>();
    }
}