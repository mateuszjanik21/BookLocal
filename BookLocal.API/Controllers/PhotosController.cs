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

    [HttpPost("upload-profile-photo")]
    public async Task<ActionResult<object>> UploadProfilePhoto(IFormFile file)
    {
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
    public async Task<ActionResult<object>> UploadBusinessPhoto(IFormFile file)
    {
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
    public async Task<ActionResult<object>> UploadEmployeePhoto(int employeeId, IFormFile file)
    {
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
    public async Task<ActionResult<object>> UploadCategoryPhoto(int categoryId, IFormFile file)
    {
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
    public async Task<ActionResult<object>> UploadBundlePhoto(int bundleId, IFormFile file)
    {
        var result = await _photosService.UploadBundlePhotoAsync(bundleId, file, User);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new { photoUrl = result.PhotoUrl });
    }
}