namespace BookLocal.API.DTOs
{
    public class ServiceUpsertDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public required int DurationMinutes { get; set; }
    }

    public class ServiceDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }
}
