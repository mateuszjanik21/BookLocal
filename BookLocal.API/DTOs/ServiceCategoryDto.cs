using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ServiceCategoryUpsertDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        [Required]
        public int MainCategoryId { get; set; }
    }
    public class ServiceCategoryDto
    {
        public int ServiceCategoryId { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }
        public List<ServiceDto> Services { get; set; } = new();
    }

    public class ServiceCategoryFeedDto
    {
        public int ServiceCategoryId { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }
        public int BusinessId { get; set; }
        public required string BusinessName { get; set; }
        public string? BusinessCity { get; set; }
        public IEnumerable<ServiceDto> Services { get; set; } = new List<ServiceDto>();
    }
}
