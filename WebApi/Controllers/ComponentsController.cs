using AutoMapper;
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
        private readonly IMapper _mapper;

        public ComponentsController(
            IComponentService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("bytype/{componentTypeId:int}")]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetByType(int componentTypeId)
        {
            var entities = _service.GetByComponentType(componentTypeId);
            var dtos = entities.Select(x => _mapper.Map<ComponentResponseDto>(x));

            return Ok(dtos);
        }

        [HttpGet("search")]
        public ActionResult<object> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool onlyActive = false)
        {
            var entities = _service.Search(q, page, pageSize, onlyActive);
            var items = entities.Select(x => _mapper.Map<ComponentResponseDto>(x));
            var total = _service.Count(q, onlyActive);

            return Ok(new { total, page, pageSize, items });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetAll(
            [FromQuery] bool onlyActive = false)
        {
            var entities = _service.GetAll(onlyActive);
            var dtos = entities.Select(x => _mapper.Map<ComponentResponseDto>(x));

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentResponseDto> GetById(int id)
        {
            var entity = _service.GetById(id);
            if (entity == null)
                return NotFound();

            return Ok(_mapper.Map<ComponentResponseDto>(entity));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entity = _mapper.Map<Component>(dto);
                entity.CreatedAt = DateTime.UtcNow;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entity = _mapper.Map<Component>(dto);
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
