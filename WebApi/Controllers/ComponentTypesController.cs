using AutoMapper;
using DAL.Models;
using DAL.Services.ComponentTypes;
using DAL.Services.Logs;
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
        private readonly ILogService _logService;

        public ComponentTypesController(
            IComponentTypeService service,
            IMapper mapper,
            ILogService logService)
        {
            _service = service;
            _mapper = mapper;
            _logService = logService;
        }

        [HttpGet("configurator")]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetForConfigurator()
        {
            try
            {
                var entities = _service.GetForConfigurator().ToList();
                var dtos = entities.Select(x => _mapper.Map<ComponentTypeResponseDto>(x)).ToList();

                _logService.LogInfo($"Returned {dtos.Count} component type(s) for configurator.", null, "ComponentTypesController");
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in GetForConfigurator: {ex.Message}", null, "ComponentTypesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentTypeResponseDto>> GetAll()
        {
            try
            {
                var entities = _service.GetAll().ToList();
                var dtos = entities.Select(x => _mapper.Map<ComponentTypeResponseDto>(x)).ToList();

                _logService.LogInfo($"Admin GetAll component types -> {dtos.Count}.", null, "ComponentTypesController");
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Admin GetAll: {ex.Message}", null, "ComponentTypesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentTypeResponseDto> GetById(int id)
        {
            try
            {
                var entity = _service.GetById(id);
                if (entity == null)
                {
                    _logService.LogWarn($"Admin GetById component type id={id} not found.", null, "ComponentTypesController");
                    return NotFound();
                }

                _logService.LogInfo($"Admin GetById component type id={id} returned.", null, "ComponentTypesController");
                return Ok(_mapper.Map<ComponentTypeResponseDto>(entity));
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Admin GetById id={id}: {ex.Message}", null, "ComponentTypesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentTypeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn("Admin Create component type: invalid ModelState.", null, "ComponentTypesController");
                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<ComponentType>(dto);
                entity.Name = entity.Name.Trim();

                var id = _service.Add(entity);

                _logService.LogInfo($"Admin created component type id={id}, name='{entity.Name}', min={entity.MinSelect}, max={entity.MaxSelect}.", null, "ComponentTypesController");
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Admin Create component type '{dto.Name}': {ex.Message}", null, "ComponentTypesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public ActionResult Update([FromBody] ComponentTypeUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn($"Admin Update component type: invalid ModelState id={dto.Id}.", null, "ComponentTypesController");
                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<ComponentType>(dto);
                entity.Name = entity.Name.Trim();

                _service.Update(entity);

                _logService.LogInfo($"Admin updated component type id={dto.Id}, name='{dto.Name}', min={dto.MinSelect}, max={dto.MaxSelect}.", null, "ComponentTypesController");
                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Admin Update component type id={dto.Id}: {ex.Message}", null, "ComponentTypesController", ex.ToString());
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

                _logService.LogInfo($"Admin deleted component type id={id}.", null, "ComponentTypesController");
                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in Admin Delete component type id={id}: {ex.Message}", null, "ComponentTypesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }
    }
}
