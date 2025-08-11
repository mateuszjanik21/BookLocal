namespace BookLocal.API.DTOs
{
    public class WorkScheduleDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public bool IsDayOff { get; set; }
    }
}
