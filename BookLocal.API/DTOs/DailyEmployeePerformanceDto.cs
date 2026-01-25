namespace BookLocal.API.DTOs
{
    public class DailyEmployeePerformanceDto
    {
        public int EmployeeId { get; set; }
        public DateOnly Date { get; set; }
        public string FullName { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Commission { get; set; }
        public double AverageRating { get; set; }
    }
}
