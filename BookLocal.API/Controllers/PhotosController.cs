using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly IPhotosService _photosService;

    public PhotosController(IPhotosService photosService)
    {
        _photosService = photosService;
    }

    private ActionResult? ValidateImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Nie przesłano pliku.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Plik jest za duży (maksymalny rozmiar to 5MB).");

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Niedozwolony format pliku. Dozwolone to: JPEG, PNG, WEBP.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(ext))
            return BadRequest("Niedozwolone rozszerzenie pliku.");

        return null;
    }

    [HttpPost("upload-profile-photo")]
    [RequestSizeLimit(5242880)]
    public async Task<ActionResult<object>> UploadProfilePhoto(IFormFile file)
    {
        var validationError = ValidateImageFile(file);
        if (validationError != null) return validationError;

        var result = await _photosService.UploadProfilePhotoAsync(file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Unauthorized") return Unauthorized();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }

    [HttpPost("business")]
    [Authorize(Roles = "owner")]
    [RequestSizeLimit(5242880)]
    public async Task<ActionResult<object>> UploadBusinessPhoto(IFormFile file)
    {
        var validationError = ValidateImageFile(file);
        if (validationError != null) return validationError;

        var result = await _photosService.UploadBusinessPhotoAsync(file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }

    [HttpPost("employee/{employeeId}")]
    [Authorize(Roles = "owner")]
    [RequestSizeLimit(5242880)]
    public async Task<ActionResult<object>> UploadEmployeePhoto(int employeeId, IFormFile file)
    {
        var validationError = ValidateImageFile(file);
        if (validationError != null) return validationError;

        var result = await _photosService.UploadEmployeePhotoAsync(employeeId, file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }

    [HttpPost("category/{categoryId}")]
    [Authorize(Roles = "owner")]
    [RequestSizeLimit(5242880)]
    public async Task<ActionResult<object>> UploadCategoryPhoto(int categoryId, IFormFile file)
    {
        var validationError = ValidateImageFile(file);
        if (validationError != null) return validationError;

        var result = await _photosService.UploadCategoryPhotoAsync(categoryId, file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }

    [HttpPost("bundle/{bundleId}")]
    [Authorize(Roles = "owner")]
    [RequestSizeLimit(5242880)]
    public async Task<ActionResult<object>> UploadBundlePhoto(int bundleId, IFormFile file)
    {
        var validationError = ValidateImageFile(file);
        if (validationError != null) return validationError;

        var result = await _photosService.UploadBundlePhotoAsync(bundleId, file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }
}