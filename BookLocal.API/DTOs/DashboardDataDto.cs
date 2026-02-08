using BookLocal.API.DTOs;

namespace BookLocal.API.DTOs
{
    public class DashboardStatsDto
    {
        public int UpcomingReservationsCount { get; set; }
        public int ClientCount { get; set; }
        public int EmployeeCount { get; set; }
        public int ServiceCount { get; set; }
        public bool HasVariants { get; set; }
    }

    public class DashboardDataDto
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public List<ReservationDto> TodaysReservations { get; set; } = new();
        public List<ReviewDto> LatestReviews { get; set; } = new();
    }

    public class DashboardStatsSqlDto
    {
        public int UpcomingReservationsCount { get; set; }
        public int EmployeeCount { get; set; }
        public int ServiceCount { get; set; }
        public int ClientCount { get; set; }
        public bool HasVariants { get; set; }
    }

    public class ReviewSqlDto
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string ReviewerName { get; set; } = "Anonim";
        public DateTime CreatedAt { get; set; }
    }
}
