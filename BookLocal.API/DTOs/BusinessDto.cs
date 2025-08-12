namespace BookLocal.API.DTOs
{
    public class BusinessDto
    {
        public required string Name { get; set; }
        public required string NIP { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }

    }

    public class BusinessDetailDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }
        public required string NIP { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public List<ServiceCategoryDto> Categories { get; set; } = new();
        public List<EmployeeDto> Employees { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public OwnerDto Owner { get; set; }
    }
    public class OwnerDto
    {
        public string? FirstName { get; set; }
    }
}
