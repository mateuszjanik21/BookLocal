using Microsoft.AspNetCore.Identity;

namespace BookLocal.Data.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}
