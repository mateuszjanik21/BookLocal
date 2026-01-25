using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        public string? Position { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsArchived { get; set; } = false;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        public virtual EmployeeDetails? EmployeeDetails { get; set; }
        public virtual EmployeeFinanceSettings? FinanceSettings { get; set; }

        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();

        public virtual ICollection<EmploymentContract> Contracts { get; set; } = new List<EmploymentContract>();
        public virtual ICollection<ScheduleException> ScheduleExceptions { get; set; } = new List<ScheduleException>();
        public virtual ICollection<EmployeePayroll> Payrolls { get; set; } = new List<EmployeePayroll>();
    }
}