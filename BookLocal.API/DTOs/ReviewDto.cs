using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? ReviewerPhotoUrl { get; set; }
        public string? ServiceName { get; set; }
        public string? EmployeeFullName { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
