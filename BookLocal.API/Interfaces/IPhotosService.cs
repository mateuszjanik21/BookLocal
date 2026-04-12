using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IPhotosService
    {
        Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadProfilePhotoAsync(IFormFile file, ClaimsPrincipal user);
        Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadBusinessPhotoAsync(IFormFile file, ClaimsPrincipal user);
        Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadEmployeePhotoAsync(int employeeId, IFormFile file, ClaimsPrincipal user);
        Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadCategoryPhotoAsync(int categoryId, IFormFile file, ClaimsPrincipal user);
        Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadBundlePhotoAsync(int bundleId, IFormFile file, ClaimsPrincipal user);
    }
}
