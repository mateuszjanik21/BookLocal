using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class OwnerCreateReservationDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public string GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
    }
}
