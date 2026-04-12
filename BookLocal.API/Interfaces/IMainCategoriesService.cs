namespace BookLocal.API.Interfaces
{
    public interface IMainCategoriesService
    {
        Task<IEnumerable<object>> GetMainCategoriesAsync();
    }
}
