namespace BookLocal.API.DTOs
{
    public class EmployeeUpsertDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Position { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Position { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
