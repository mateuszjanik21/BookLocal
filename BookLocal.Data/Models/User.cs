using Microsoft.AspNetCore.Identity;

namespace BookLocal.Data.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public string? PhotoUrl { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
