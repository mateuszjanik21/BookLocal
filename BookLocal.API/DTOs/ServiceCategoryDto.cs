using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ServiceCategoryUpsertDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
    }
    public class ServiceCategoryDto
    {
        public int ServiceCategoryId { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }
        public List<ServiceDto> Services { get; set; } = new();
    }
}
