using AutoMapper;
using DAL.Models;
using DAL.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public ImagesController(
            IImageService service,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _service = service;
            _mapper = mapper;
            _env = env;
        }

        [HttpGet("search")]
        public ActionResult<object> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var entities = _service.SearchByFileName(q, page, pageSize);
            var items = entities.Select(x => _mapper.Map<WebApi.DTOs.Images.ImageResponseDto>(x));
            var total = _service.Count(q);

            return Ok(new { total, page, pageSize, items });
        }

        public sealed class UploadImageForm
        {
            [Required]
            public IFormFile File { get; set; } = default!;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)] // 20 MB
        public async Task<IActionResult> Upload([FromForm] UploadImageForm form)
        {
            var file = form.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowed = new[] { "image/png", "image/jpeg", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                return BadRequest("Only PNG, JPEG or WEBP allowed.");

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
            return Ok(new { id, url = baseUrl + image.StoragePathOrUrl });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
