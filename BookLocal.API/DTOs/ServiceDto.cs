using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ServiceVariantUpsertDto
    {
        public int? ServiceVariantId { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int DurationMinutes { get; set; }
        public int CleanupTimeMinutes { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
    }

    public class ServiceUpsertDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }

        [Required]
        public int ServiceCategoryId { get; set; }

        public List<ServiceVariantUpsertDto> Variants { get; set; } = new();
    }

    public class ServiceVariantDto
    {
        public int ServiceVariantId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public int CleanupTimeMinutes { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int FavoritesCount { get; set; }
    }

    public class ServiceDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; }

        [Required]
        public int ServiceCategoryId { get; set; }

        public List<ServiceVariantDto> Variants { get; set; } = new();
    }
}