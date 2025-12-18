using DAL.Models;
using DAL.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Images;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;

        public ImagesController(IImageService service)
        {
            _service = service;
        }

        [HttpGet("search")]
        public ActionResult<object> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var items = _service.SearchByFileName(q, page, pageSize)
                .Select(x => new ImageResponseDto
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    Length = x.Length,
                    StoragePathOrUrl = x.StoragePathOrUrl,
                    UploadedAt = x.UploadedAt,
                    AltText = x.AltText,
                    Width = x.Width,
                    Height = x.Height
                });

            var total = _service.Count(q);
            return Ok(new { total, page, pageSize, items });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ImageCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = new Image
                {
                    FileName = dto.FileName.Trim(),
                    ContentType = dto.ContentType.Trim(),
                    Length = dto.Length,
                    StoragePathOrUrl = dto.StoragePathOrUrl.Trim(),
                    AltText = dto.AltText,
                    Width = dto.Width,
                    Height = dto.Height,
                    UploadedAt = DateTime.UtcNow
                };

                var id = _service.Create(entity);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
