using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Models
{
    public class Business
    {
        [Key]
        public int BusinessId { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Required]
        [MinLength(10)]
        public string NIP { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User Owner { get; set; }
        public string? PhotoUrl { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
