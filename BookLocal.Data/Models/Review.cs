using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; } 

        [Required]
        public string ReviewerName { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public int? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
    }
}
