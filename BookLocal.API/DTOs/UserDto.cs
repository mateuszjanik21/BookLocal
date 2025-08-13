namespace BookLocal.API.DTOs
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? PhotoUrl { get; set; }
        public required IList<string> Roles { get; set; }
    }

    public class UserUpdateDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}
