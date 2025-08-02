using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;

public enum ReservationStatus
{
    confirmed,
    cancelled,
    completed
}

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

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
    public ReservationStatus Status { get; set; } = ReservationStatus.confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Customer { get; set; }
    public virtual Service Service { get; set; }
    public virtual Employee Employee { get; set; }
}