using AutoMapper;
using DAL.Models;
using DAL.Services.Images;
using DAL.Services.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.DTOs.Images;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly ILogService _logService;

        public ImagesController(
            IImageService service,
            IMapper mapper,
            IWebHostEnvironment env,
            ILogService logService)
        {
            _service = service;
            _mapper = mapper;
            _env = env;
            _logService = logService;
        }

        [HttpGet("search")]
        public ActionResult<object> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var entities = _service.SearchByFileName(q, page, pageSize).ToList();
                var items = entities.Select(x => _mapper.Map<ImageResponseDto>(x)).ToList();
                var total = _service.Count(q);

                _logService.LogInfo(
                    $"Image search q='{q}', page={page}, pageSize={pageSize} -> returned {items.Count} of total {total}.",
                    null,
                    "ImagesController");

                return Ok(new { total, page, pageSize, items });
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Image Search q='{q}': {ex.Message}",
                    null,
                    "ImagesController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        public sealed class UploadImageForm
        {
            [Required]
            public IFormFile File { get; set; } = default!;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm] UploadImageForm form)
        {
            try
            {
                var file = form.File;

                if (file == null || file.Length == 0)
                {
                    _logService.LogWarn("Upload failed: no file uploaded.", null, "ImagesController");
                    return BadRequest("No file uploaded.");
                }

                var allowed = new[] { "image/png", "image/jpeg", "image/webp" };
                if (!allowed.Contains(file.ContentType))
                {
                    _logService.LogWarn($"Upload failed: invalid contentType '{file.ContentType}'.", null, "ImagesController");
                    return BadRequest("Only PNG, JPEG or WEBP allowed.");
                }

                var webRoot = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var uploadsRoot = Path.Combine(webRoot, "uploads");
                Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(file.FileName);
                var newName = $"{Guid.NewGuid():N}{ext}";
                var physicalPath = Path.Combine(uploadsRoot, newName);

                await using (var fs = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(fs);
                }

                var image = new Image
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    StoragePathOrUrl = $"/uploads/{newName}",
                    UploadedAt = DateTime.UtcNow
                };

                var id = _service.Create(image);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var url = baseUrl + image.StoragePathOrUrl;

                _logService.LogInfo(
                    $"Admin uploaded image id={id}, file='{file.FileName}', contentType='{file.ContentType}', length={file.Length}, url='{image.StoragePathOrUrl}'.",
                    null,
                    "ImagesController");

                return Ok(new { id, url });
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Upload: {ex.Message}",
                    null,
                    "ImagesController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id);

                _logService.LogInfo($"Admin deleted image id={id}.", null, "ImagesController");
                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Delete image id={id}: {ex.Message}", null, "ImagesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }
    }
}
