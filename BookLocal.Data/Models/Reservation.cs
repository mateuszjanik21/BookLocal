using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum ReservationStatus
{
    Confirmed,
    Cancelled,
    Completed,
    NoShow
}

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public int BusinessId { get; set; }
    [ForeignKey("BusinessId")]
    public virtual Business Business { get; set; } = null!;

    public string? CustomerId { get; set; }
    public virtual User? Customer { get; set; }

    public string? GuestName { get; set; }
    public string? GuestPhoneNumber { get; set; }

    [Required]
    public int ServiceVariantId { get; set; }
    [ForeignKey("ServiceVariantId")]
    public virtual ServiceVariant ServiceVariant { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal AgreedPrice { get; set; }

    public int? ServiceBundleId { get; set; }
    [ForeignKey("ServiceBundleId")]
    public virtual ServiceBundle? ServiceBundle { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Employee Employee { get; set; }
    public virtual Review? Review { get; set; }
}