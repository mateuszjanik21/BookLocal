using BookLocal.API.DTOs;

namespace BookLocal.API.Interfaces
{
    public interface ICategoriesService
    {
        Task<IEnumerable<ServiceCategoryFeedDto>> GetCategoryFeedAsync();
    }
}
