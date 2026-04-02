using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ReservationCreateDto
    {
        public required int ServiceVariantId { get; set; }
        public required int EmployeeId { get; set; }
        public DateTime StartTime { get; set; }
        public string? DiscountCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public int LoyaltyPointsUsed { get; set; } = 0;
    }

    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public int ServiceVariantId { get; set; }
        public string VariantName { get; set; }
        public string ServiceName { get; set; }
        public decimal AgreedPrice { get; set; }

        public int? DiscountId { get; set; }
        public decimal DiscountAmount { get; set; }

        public string BusinessName { get; set; }
        public int BusinessId { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }
        public string? EmployeePhotoUrl { get; set; }
        public string? EmployeePhoneNumber { get; set; }

        public string CustomerId { get; set; }
        public string? CustomerFullName { get; set; }
        public string? CustomerPhotoUrl { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public string? GuestName { get; set; }

        public bool IsServiceArchived { get; set; }

        public bool HasReview { get; set; }
        public int? ServiceBundleId { get; set; }
        public string? BundleName { get; set; }
        public bool IsBundle { get; set; }
        public int LoyaltyPointsUsed { get; set; }
        public string PaymentMethod { get; set; }
        public List<BundleSubItemDto>? SubItems { get; set; }
    }

    public class BundleSubItemDto
    {
        public int ReservationId { get; set; }
        public string ServiceName { get; set; }
        public string VariantName { get; set; }
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
        public string? DiscountCode { get; set; }
        public string? CustomerId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public int LoyaltyPointsUsed { get; set; } = 0;
    }

    public class OwnerCreateBundleReservationDto
    {
        [Required]
        public int ServiceBundleId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public string GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public string? DiscountCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

    public class BundleReservationCreateDto
    {
        [Required]
        public int ServiceBundleId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public string? DiscountCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public int LoyaltyPointsUsed { get; set; } = 0;
    }

    public class ReservationSqlDto
    {
        public int ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public decimal AgreedPrice { get; set; }
        public string PaymentMethod { get; set; }
        public int ServiceVariantId { get; set; }
        public string VariantName { get; set; }
        public string ServiceName { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerFullName { get; set; }
        public string? GuestName { get; set; }
        public int? ServiceBundleId { get; set; }
        public bool HasReview { get; set; }
    }
}
