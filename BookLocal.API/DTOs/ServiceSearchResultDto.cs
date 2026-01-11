namespace BookLocal.API.DTOs
{
    public class ServiceSearchResultDto
    {
        public int DefaultServiceVariantId { get; set; }
        public int ServiceId { get; set; }

        public required string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

        public int BusinessId { get; set; }
        public required string BusinessName { get; set; }
        public string? BusinessCity { get; set; }
        public string? MainCategoryName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ServiceCategorySearchResultDto
    {
        public int ServiceCategoryId { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }
        public int BusinessId { get; set; }
        public required string BusinessName { get; set; }
        public string? BusinessCity { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime BusinessCreatedAt { get; set; }

        // Tutaj używamy już zaktualizowanego ServiceDto (który ma listę wariantów)
        public List<ServiceDto> Services { get; set; } = new();
    }
}