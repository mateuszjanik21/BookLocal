using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum ReservationStatus
{
    Confirmed,
    Cancelled,
    Completed
}

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public int BusinessId { get; set; }
    [ForeignKey("BusinessId")]
    public virtual Business Business { get; set; } = null!;

    [Required]
    public string CustomerId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Customer { get; set; }
    public virtual Service Service { get; set; }
    public virtual Employee Employee { get; set; }
}