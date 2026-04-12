namespace BookLocal.API.Interfaces
{
    public interface IMaintenanceService
    {
        Task<(bool Success, string Message)> RecalculateCustomerStatsAsync();
    }
}
