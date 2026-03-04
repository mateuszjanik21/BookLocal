namespace BookLocal.API.Services
{
    public interface ILazyStateService
    {
        Task SyncUserStateAsync(string userId, string userRole);
    }
}
