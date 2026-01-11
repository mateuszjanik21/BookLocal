using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum AbsenceType
    {
        Vacation,
        SickLeave, 
        UnpaidLeave,
        Training,
        Other
    }

    public class ScheduleException
    {
        [Key]
        public int ExceptionId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public DateOnly DateFrom { get; set; }

        [Required]
        public DateOnly DateTo { get; set; }

        public AbsenceType Type { get; set; }

        public bool BlocksCalendar { get; set; } = true;

        public string? Reason { get; set; } 

        public bool IsApproved { get; set; } = false;
    }
}