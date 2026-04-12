namespace BookLocal.API.Interfaces
{
    public interface IAvailabilityService
    {
        Task<(bool Success, IEnumerable<DateTime>? Slots, string? ErrorMessage)> GetAvailableSlotsAsync(int employeeId, DateTime date, int serviceVariantId);
        Task<(bool Success, IEnumerable<DateTime>? Slots, string? ErrorMessage)> GetBundleAvailableSlotsAsync(int employeeId, DateTime date, int bundleId);
    }
}
