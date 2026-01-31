using AutoMapper;
using DAL.Services.Configurations;
using DAL.Services.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Components;
using WebApi.DTOs.Configurations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IConfigurationService _service;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;

        public ConfigurationsController(
            IConfigurationService service,
            IMapper mapper,
            ILogService logService)
        {
            _service = service;
            _mapper = mapper;
            _logService = logService;
        }

        [HttpGet("user/{userId:int}")]
        public ActionResult<IEnumerable<ConfigurationResponseDto>> GetByUser(int userId)
        {
            try
            {
                var entities = _service.GetUserConfigurations(userId).ToList();
                var dtos = entities.Select(x => _mapper.Map<ConfigurationResponseDto>(x)).ToList();

                _logService.LogInfo(
                    $"Returned {dtos.Count} configuration(s) for userId={userId}.",
                    userId,
                    "ConfigurationsController");

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in GetByUser userId={userId}: {ex.Message}",
                    userId,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public ActionResult<ConfigurationDetailsResponseDto> GetDetails(int id)
        {
            try
            {
                var cfg = _service.GetConfigurationDetails(id);
                if (cfg == null)
                {
                    _logService.LogWarn(
                        $"GetDetails configuration id={id} not found.",
                        null,
                        "ConfigurationsController");

                    return NotFound();
                }

                var dto = _mapper.Map<ConfigurationDetailsResponseDto>(cfg);
                dto.Components = cfg.CarConfigurationComponents
                    .Select(cc => cc.Component)
                    .Where(c => c != null)
                    .Select(c => _mapper.Map<ComponentResponseDto>(c!))
                    .ToList();

                _logService.LogInfo(
                    $"Returned configuration details id={id}, components={dto.Components.Count}, totalPrice={dto.TotalPrice}.",
                    cfg.UserId,
                    "ConfigurationsController");

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in GetDetails configuration id={id}: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Create([FromBody] ConfigurationCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn(
                    $"Create configuration: invalid ModelState for userId={dto.UserId}.",
                    dto.UserId,
                    "ConfigurationsController");

                return BadRequest(ModelState);
            }

            try
            {
                var id = _service.CreateConfiguration(dto.UserId, dto.Name.Trim());

                _logService.LogInfo(
                    $"Created configuration id={id} for userId={dto.UserId}, name='{dto.Name}'.",
                    dto.UserId,
                    "ConfigurationsController");

                return Ok(new { id });
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Create configuration for userId={dto.UserId}: {ex.Message}",
                    dto.UserId,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public ActionResult Update([FromBody] ConfigurationUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn(
                    $"Update configuration: invalid ModelState id={dto.Id}.",
                    null,
                    "ConfigurationsController");

                return BadRequest(ModelState);
            }

            try
            {
                var existing = _service.GetConfiguration(dto.Id);
                if (existing == null)
                {
                    _logService.LogWarn(
                        $"Update configuration id={dto.Id} not found.",
                        null,
                        "ConfigurationsController");

                    return NotFound();
                }

                existing.Name = dto.Name.Trim();
                _service.UpdateConfiguration(existing);

                _logService.LogInfo(
                    $"Updated configuration id={dto.Id}, newName='{dto.Name}'.",
                    existing.UserId,
                    "ConfigurationsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Update configuration id={dto.Id}: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _service.DeleteConfiguration(id);

                _logService.LogInfo(
                    $"Deleted configuration id={id}.",
                    null,
                    "ConfigurationsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Delete configuration id={id}: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{configurationId:int}/components")]
        public ActionResult AddComponent(int configurationId, [FromBody] ConfigurationAddComponentDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn(
                    $"AddComponent: invalid ModelState configurationId={configurationId}.",
                    null,
                    "ConfigurationsController");

                return BadRequest(ModelState);
            }

            try
            {
                _service.AddComponentToConfiguration(configurationId, dto.ComponentId);

                _logService.LogInfo(
                    $"Added componentId={dto.ComponentId} to configurationId={configurationId}.",
                    null,
                    "ConfigurationsController");

                return Ok();
            }
            catch (Exception ex)
            {
                // ovo je često "normalan" fail (maxSelect / compatibility)
                _logService.LogWarn(
                    $"Failed to add componentId={dto.ComponentId} to configurationId={configurationId}. Reason: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{configurationId:int}/components/{componentId:int}")]
        public ActionResult RemoveComponent(int configurationId, int componentId)
        {
            try
            {
                _service.RemoveComponentFromConfiguration(configurationId, componentId);

                _logService.LogInfo(
                    $"Removed componentId={componentId} from configurationId={configurationId}.",
                    null,
                    "ConfigurationsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in RemoveComponent configurationId={configurationId}, componentId={componentId}: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{configurationId:int}/components")]
        public ActionResult Clear(int configurationId)
        {
            try
            {
                _service.ClearConfiguration(configurationId);

                _logService.LogInfo(
                    $"Cleared all components from configurationId={configurationId}.",
                    null,
                    "ConfigurationsController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in Clear configurationId={configurationId}: {ex.Message}",
                    null,
                    "ConfigurationsController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }
    }
}
