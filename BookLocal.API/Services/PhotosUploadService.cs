using BookLocal.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class PhotosUploadService : IPhotosService
    {
        private readonly IPhotoService _photoService;
        private readonly AppDbContext _context;

        public PhotosUploadService(IPhotoService photoService, AppDbContext context)
        {
            _photoService = photoService;
            _context = context;
        }

        public async Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadProfilePhotoAsync(IFormFile file, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null, "Unauthorized", 401);

            var uploadResult = await _photoService.UploadPhotoAsync(file);
            if (uploadResult.Error != null) return (false, null, uploadResult.Error.Message, 400);

            var photoUrl = uploadResult.SecureUrl.AbsoluteUri;

            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null) return (false, null, "Nie znaleziono użytkownika.", 404);
            dbUser.PhotoUrl = photoUrl;

            if (user.IsInRole("owner"))
            {
                var ownerAsEmployee = await _context.Employees
                    .Include(e => e.Business)
                    .FirstOrDefaultAsync(e => e.Business.OwnerId == userId && e.Position == "Właściciel");

                if (ownerAsEmployee != null)
                {
                    ownerAsEmployee.PhotoUrl = photoUrl;
                }
            }

            await _context.SaveChangesAsync();
            return (true, photoUrl, null, 200);
        }

        public async Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadBusinessPhotoAsync(IFormFile file, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);
            if (business == null) return (false, null, "Brak uprawnień.", 403);

            var uploadResult = await _photoService.UploadPhotoAsync(file);
            if (uploadResult.Error != null) return (false, null, uploadResult.Error.Message, 400);

            business.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
            await _context.SaveChangesAsync();

            return (true, business.PhotoUrl, null, 200);
        }

        public async Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadEmployeePhotoAsync(int employeeId, IFormFile file, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.Business)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, null, "Brak uprawnień.", 403);

            var uploadResult = await _photoService.UploadPhotoAsync(file);
            if (uploadResult.Error != null) return (false, null, uploadResult.Error.Message, 400);

            employee.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
            await _context.SaveChangesAsync();

            return (true, employee.PhotoUrl, null, 200);
        }

        public async Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadCategoryPhotoAsync(int categoryId, IFormFile file, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var category = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.Business.OwnerId == ownerId);

            if (category == null) return (false, null, "Brak uprawnień.", 403);

            var uploadResult = await _photoService.UploadPhotoAsync(file);
            if (uploadResult.Error != null) return (false, null, uploadResult.Error.Message, 400);

            category.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
            await _context.SaveChangesAsync();

            return (true, category.PhotoUrl, null, 200);
        }

        public async Task<(bool Success, string? PhotoUrl, string? ErrorMessage, int StatusCode)> UploadBundlePhotoAsync(int bundleId, IFormFile file, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var bundle = await _context.ServiceBundles
                .Include(sb => sb.Business)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == bundleId && sb.Business.OwnerId == ownerId);

            if (bundle == null) return (false, null, "Brak uprawnień.", 403);

            var uploadResult = await _photoService.UploadPhotoAsync(file);
            if (uploadResult.Error != null) return (false, null, uploadResult.Error.Message, 400);

            bundle.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
            await _context.SaveChangesAsync();

            return (true, bundle.PhotoUrl, null, 200);
        }
    }
}
