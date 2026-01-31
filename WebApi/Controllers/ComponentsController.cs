using AutoMapper;
using DAL.Models;
using DAL.Services.Components;
using DAL.Services.Logs;
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
        private readonly ILogService _logService;

        public ComponentsController(
            IComponentService service,
            IMapper mapper,
            ILogService logService)
        {
            _service = service;
            _mapper = mapper;
            _logService = logService;
        }

        [HttpGet("bytype/{componentTypeId:int}")]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetByType(int componentTypeId)
        {
            try
            {
                var entities = _service.GetByComponentType(componentTypeId).ToList();
                var dtos = entities.Select(x => _mapper.Map<ComponentResponseDto>(x)).ToList();

                _logService.LogInfo(
                    $"Returned {dtos.Count} component(s) for componentTypeId={componentTypeId}.",
                    null,
                    "ComponentsController");

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in GetByType componentTypeId={componentTypeId}: {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public ActionResult<object> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool onlyActive = false)
        {
            try
            {
                var entities = _service.Search(q, page, pageSize, onlyActive).ToList();
                var items = entities.Select(x => _mapper.Map<ComponentResponseDto>(x)).ToList();
                var total = _service.Count(q, onlyActive);

                _logService.LogInfo(
                    $"Component search q='{q}', page={page}, pageSize={pageSize}, onlyActive={onlyActive} -> returned {items.Count} of total {total}.",
                    null,
                    "ComponentsController");

                return Ok(new { total, page, pageSize, items });
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Search q='{q}', page={page}, pageSize={pageSize}, onlyActive={onlyActive}: {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<ComponentResponseDto>> GetAll([FromQuery] bool onlyActive = false)
        {
            try
            {
                var entities = _service.GetAll(onlyActive).ToList();
                var dtos = entities.Select(x => _mapper.Map<ComponentResponseDto>(x)).ToList();

                _logService.LogInfo(
                    $"Admin GetAll components onlyActive={onlyActive} -> {dtos.Count} item(s).",
                    null,
                    "ComponentsController");

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Admin GetAll onlyActive={onlyActive}: {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public ActionResult<ComponentResponseDto> GetById(int id)
        {
            try
            {
                var entity = _service.GetById(id);
                if (entity == null)
                {
                    _logService.LogWarn(
                        $"Admin GetById component id={id} not found.",
                        null,
                        "ComponentsController");

                    return NotFound();
                }

                _logService.LogInfo(
                    $"Admin GetById component id={id} returned.",
                    null,
                    "ComponentsController");

                return Ok(_mapper.Map<ComponentResponseDto>(entity));
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Admin GetById id={id}: {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create([FromBody] ComponentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn(
                    "Admin Create component: invalid ModelState.",
                    null,
                    "ComponentsController");

                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<Component>(dto);
                entity.CreatedAt = DateTime.UtcNow;

                _service.Create(entity);

                _logService.LogInfo(
                    $"Admin created component Name='{entity.Name}', Title='{entity.Title}', TypeId={entity.ComponentTypeId}, Price={entity.Price}.",
                    null,
                    "ComponentsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Admin Create component Name='{dto.Name}': {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public ActionResult Update([FromBody] ComponentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn(
                    $"Admin Update component: invalid ModelState for id={dto.Id}.",
                    null,
                    "ComponentsController");

                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<Component>(dto);
                _service.Update(entity);

                _logService.LogInfo(
                    $"Admin updated component id={dto.Id}, Name='{dto.Name}', Title='{dto.Title}', TypeId={dto.ComponentTypeId}, Price={dto.Price}.",
                    null,
                    "ComponentsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Admin Update component id={dto.Id}: {ex.Message}",
                    null,
                    "ComponentsController",
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

                _logService.LogInfo(
                    $"Admin deleted component id={id}.",
                    null,
                    "ComponentsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Admin Delete component id={id}: {ex.Message}",
                    null,
                    "ComponentsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }
    }
}
