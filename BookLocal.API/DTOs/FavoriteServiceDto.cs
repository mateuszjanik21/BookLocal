namespace BookLocal.API.DTOs
{
    public class FavoriteServiceDto
    {
        public int ServiceVariantId { get; set; }
        public string ServiceName { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string? BusinessCity { get; set; }
        public string? BusinessPhotoUrl { get; set; }

        public bool IsActive { get; set; }
        public bool IsServiceArchived { get; set; }
    }
}
