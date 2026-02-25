using System;
using System.Collections.Generic;

namespace BookLocal.API.DTOs
{
    public enum CustomerStatusFilter
    {
        All = 0,
        VIP = 1,
        Banned = 2,
        Standard = 3
    }

    public enum CustomerHistoryFilter
    {
        All = 0,
        WithHistory = 1,
        WithoutHistory = 2
    }

    public enum CustomerSpentFilter
    {
        All = 0,
        Any = 1,
        Over100 = 2,
        Over500 = 3,
        Over1000 = 4
    }

    public class CustomerListItemDto
    {
        public int ProfileId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime LastVisitDate { get; set; }
        public DateTime? NextVisitDate { get; set; }
        public decimal TotalSpent { get; set; }
        public bool IsVIP { get; set; }
        public bool IsBanned { get; set; }
        public int CancelledCount { get; set; }
        public int PointsBalance { get; set; }
    }

    public class CustomerDetailDto : CustomerListItemDto
    {
        public string? PrivateNotes { get; set; }
        public string? Allergies { get; set; }
        public string? Formulas { get; set; }
        public int NoShowCount { get; set; }
        public int VisitCount { get; set; }
        public List<ReservationHistoryDto> History { get; set; } = new List<ReservationHistoryDto>();
    }

    public class ReservationHistoryDto
    {
        public int ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public string ServiceName { get; set; }
        public string EmployeeName { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
    }

    public class UpdateCustomerNoteDto
    {
        public string? PrivateNotes { get; set; }
        public string? Allergies { get; set; }
        public string? Formulas { get; set; }
    }

    public class UpdateCustomerStatusDto
    {
        public bool IsVIP { get; set; }
        public bool IsBanned { get; set; }
    }
}
