using DAL.Models;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.ComponentTypes;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentTypesController : ControllerBase
    {
        private readonly IComponentTypeService _service;

        public ComponentTypesController(IComponentTypeService service)
        {
            _service = service;
        }

        // Configurator (user)
        [HttpGet("configurator")]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetForConfigurator()
        {
            var types = _service.GetForConfigurator()
                .Select(x => new ComponentTypeResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    MinSelect = x.MinSelect,
                    MaxSelect = x.MaxSelect,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                });

            return Ok(types);
        }

        // Admin CRUD
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetAll()
        {
            var types = _service.GetAll()
                .Select(x => new ComponentTypeResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    MinSelect = x.MinSelect,
                    MaxSelect = x.MaxSelect,
                    DisplayOrder = x.DisplayOrder,
                    IsActive = x.IsActive
                });

            return Ok(types);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentTypeResponseDto> GetById(int id)
        {
            var x = _service.GetById(id);
            if (x == null) return NotFound();

            return Ok(new ComponentTypeResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                MinSelect = x.MinSelect,
                MaxSelect = x.MaxSelect,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentTypeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = new ComponentType
                {
                    Name = dto.Name.Trim(),
                    MinSelect = dto.MinSelect,
                    MaxSelect = dto.MaxSelect,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive
                };

                var id = _service.Add(entity);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public ActionResult Update([FromBody] ComponentTypeUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = new ComponentType
                {
                    Id = dto.Id,
                    Name = dto.Name.Trim(),
                    MinSelect = dto.MinSelect,
                    MaxSelect = dto.MaxSelect,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive
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
    }
}
