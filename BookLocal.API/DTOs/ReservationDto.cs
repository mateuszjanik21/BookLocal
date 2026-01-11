using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ReservationCreateDto
    {
        public required int ServiceVariantId { get; set; }
        public required int EmployeeId { get; set; }
        public required DateTime StartTime { get; set; }
    }

    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }

        // Szczegóły usługi
        public int ServiceVariantId { get; set; }
        public string VariantName { get; set; }
        public string ServiceName { get; set; } 
        public decimal AgreedPrice { get; set; } 

        public string BusinessName { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }

        public string CustomerId { get; set; }
        public string? CustomerFullName { get; set; }
        public string? GuestName { get; set; }

        public bool IsServiceArchived { get; set; }
        public bool HasReview { get; set; }
    }

    public class UpdateReservationStatusDto
    {
        [Required]
        public string Status { get; set; }
    }

    public class OwnerCreateReservationDto
    {
        [Required]
        public int ServiceVariantId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public string GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
    }
}
