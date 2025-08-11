using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    public int BusinessId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }
    public string? PhotoUrl { get; set; }

    public virtual Business Business { get; set; }
    public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}