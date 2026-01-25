namespace BookLocal.API.DTOs
{
    public class EmployeeUpsertDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public bool IsStudent { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public string? PhotoUrl { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Specialization { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }

        public bool IsStudent { get; set; }
        public bool IsArchived { get; set; }
    }

    public class EmployeeDetailDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public bool IsStudent { get; set; }

        public decimal EstimatedRevenue { get; set; }
        public required ICollection<ServiceDto> AssignedServices { get; set; }
        public required ICollection<WorkScheduleDto> WorkSchedules { get; set; }
    }
}