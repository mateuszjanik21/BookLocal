using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Models
{
    public class WorkSchedule
    {
        [Key]
        public int WorkScheduleId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public bool IsDayOff { get; set; } = false;
    }
}
