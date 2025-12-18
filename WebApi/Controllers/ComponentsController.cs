using DAL.Models;
using DAL.Services.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Components;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentsController : ControllerBase
    {
        private readonly IComponentService _service;

        public ComponentsController(IComponentService service)
        {
            _service = service;
        }

        // Configurator: komponente po tipu (samo aktivne)
        [HttpGet("bytype/{componentTypeId:int}")]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetByType(int componentTypeId)
        {
            var list = _service.GetByComponentType(componentTypeId)
                .Select(MapToDto);

            return Ok(list);
        }

        // Search (admin ili korisnik, po želji)
        [HttpGet("search")]
        public ActionResult<object> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool onlyActive = false)
        {
            var items = _service.Search(q, page, pageSize, onlyActive).Select(MapToDto);
            var total = _service.Count(q, onlyActive);

            return Ok(new { total, page, pageSize, items });
        }

        // Admin CRUD
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetAll([FromQuery] bool onlyActive = false)
        {
            var list = _service.GetAll(onlyActive).Select(MapToDto);
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentResponseDto> GetById(int id)
        {
            var x = _service.GetById(id);
            if (x == null) return NotFound();
            return Ok(MapToDto(x));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = new Component
                {
                    Name = dto.Name.Trim(),
                    Title = dto.Title.Trim(),
                    Description = dto.Description,
                    Price = dto.Price,
                    IsActive = dto.IsActive,
                    SortOrder = dto.SortOrder,
                    ComponentTypeId = dto.ComponentTypeId,
                    ImageId = dto.ImageId,
                    CreatedAt = DateTime.UtcNow
                };

                _service.Create(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public ActionResult Update([FromBody] ComponentUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = new Component
                {
                    Id = dto.Id,
                    Name = dto.Name.Trim(),
                    Title = dto.Title.Trim(),
                    Description = dto.Description,
                    Price = dto.Price,
                    IsActive = dto.IsActive,
                    SortOrder = dto.SortOrder,
                    ComponentTypeId = dto.ComponentTypeId,
                    ImageId = dto.ImageId
                };

                _service.Update(entity);
                return Ok();
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

        private static ComponentResponseDto MapToDto(Component x)
        {
            return new ComponentResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder,
                ComponentTypeId = x.ComponentTypeId,
                ComponentTypeName = x.ComponentType?.Name ?? "",
                ImageId = x.ImageId,
                ImageUrl = x.Image?.StoragePathOrUrl
            };
        }
    }
}
