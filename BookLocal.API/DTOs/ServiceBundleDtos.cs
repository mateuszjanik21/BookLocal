using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ServiceBundleDto
    {
        public int ServiceBundleId { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceBundleItemDto> Items { get; set; } = new List<ServiceBundleItemDto>();
    }

    public class ServiceBundleItemDto
    {
        public int ServiceBundleItemId { get; set; }
        public int ServiceVariantId { get; set; }
        public string VariantName { get; set; }
        public string ServiceName { get; set; }
        public int DurationMinutes { get; set; }
        public int SequenceOrder { get; set; }
        public decimal OriginalPrice { get; set; }
    }

    public class CreateServiceBundleDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal TotalPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateServiceBundleItemDto> Items { get; set; } = new List<CreateServiceBundleItemDto>();
    }

    public class CreateServiceBundleItemDto
    {
        [Required]
        public int ServiceVariantId { get; set; }

        [Required]
        public int SequenceOrder { get; set; }
    }
}
