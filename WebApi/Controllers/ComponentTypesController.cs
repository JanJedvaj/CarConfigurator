using AutoMapper;
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
        private readonly IMapper _mapper;

        public ComponentTypesController(
            IComponentTypeService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("configurator")]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetForConfigurator()
        {
            var entities = _service.GetForConfigurator();
            var dtos = entities.Select(x => _mapper.Map<ComponentTypeResponseDto>(x));

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetAll()
        {
            var entities = _service.GetAll();
            var dtos = entities.Select(x => _mapper.Map<ComponentTypeResponseDto>(x));

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentTypeResponseDto> GetById(int id)
        {
            var entity = _service.GetById(id);
            if (entity == null)
                return NotFound();

            return Ok(_mapper.Map<ComponentTypeResponseDto>(entity));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentTypeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entity = _mapper.Map<ComponentType>(dto);
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entity = _mapper.Map<ComponentType>(dto);
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
