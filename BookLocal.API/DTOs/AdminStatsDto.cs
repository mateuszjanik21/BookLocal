namespace BookLocal.API.DTOs
{
    public class AdminStatsDto
    {
        public int TotalBusinesses { get; set; }
        public int NewBusinessesThisMonth { get; set; }

        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PendingVerifications { get; set; }
    }
}
