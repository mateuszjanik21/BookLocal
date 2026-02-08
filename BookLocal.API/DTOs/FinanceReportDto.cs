namespace BookLocal.API.DTOs
{
    public class FinanceReportSqlDto
    {
        public DateTime ReportDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CashRevenue { get; set; }
        public decimal CardRevenue { get; set; }
        public decimal OnlineRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowCount { get; set; }
        public int NewCustomersCount { get; set; }
        public int ReturningCustomersCount { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal AverageTicketValue { get; set; }
        public string? TopSellingServiceName { get; set; }
    }
}
