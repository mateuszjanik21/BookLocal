using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly AppDbContext _context;

    public PhotosController(IPhotoService photoService, AppDbContext context)
    {
        _photoService = photoService;
        _context = context;
    }

    [HttpPost("upload-profile-photo")]
    public async Task<ActionResult<object>> UploadProfilePhoto(IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var uploadResult = await _photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

        var photoUrl = uploadResult.SecureUrl.AbsoluteUri;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();
        user.PhotoUrl = photoUrl;

        if (User.IsInRole("owner"))
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
        return Ok(new { photoUrl = user.PhotoUrl });
    }

    [HttpPost("business")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<object>> UploadBusinessPhoto(IFormFile file)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);
        if (business == null) return Forbid();

        var uploadResult = await _photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

        business.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _context.SaveChangesAsync();

        return Ok(new { photoUrl = business.PhotoUrl });
    }

    [HttpPost("employee/{employeeId}")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<object>> UploadEmployeePhoto(int employeeId, IFormFile file)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _context.Employees
            .Include(e => e.Business)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return Forbid();
        }

        var uploadResult = await _photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

        employee.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _context.SaveChangesAsync();

        return Ok(new { photoUrl = employee.PhotoUrl });
    }

    [HttpPost("category/{categoryId}")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<object>> UploadCategoryPhoto(int categoryId, IFormFile file)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _context.ServiceCategories
            .Include(sc => sc.Business)
            .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.Business.OwnerId == ownerId);

        if (category == null)
        {
            return Forbid();
        }

        var uploadResult = await _photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

        category.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _context.SaveChangesAsync();

        return Ok(new { photoUrl = category.PhotoUrl });
    }

    [HttpPost("bundle/{bundleId}")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<object>> UploadBundlePhoto(int bundleId, IFormFile file)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var bundle = await _context.ServiceBundles
            .Include(sb => sb.Business)
            .FirstOrDefaultAsync(sb => sb.ServiceBundleId == bundleId && sb.Business.OwnerId == ownerId);

        if (bundle == null) return Forbid();

        var uploadResult = await _photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

        bundle.PhotoUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _context.SaveChangesAsync();

        return Ok(new { photoUrl = bundle.PhotoUrl });
    }
}