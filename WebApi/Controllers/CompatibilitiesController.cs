using AutoMapper;
using DAL.Services.Compatibilities;
using DAL.Services.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Compatibilities;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompatibilitiesController : ControllerBase
    {
        private readonly ICompatibilityService _service;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;

        public CompatibilitiesController(
            ICompatibilityService service,
            IMapper mapper,
            ILogService logService)
        {
            _service = service;
            _mapper = mapper;
            _logService = logService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<CompatibilityResponseDto>> GetAll()
        {
            try
            {
                var rules = _service.GetAll().ToList();
                var dtos = rules.Select(x => _mapper.Map<CompatibilityResponseDto>(x)).ToList();

                _logService.LogInfo($"Admin fetched {dtos.Count} compatibility rule(s).", null, "CompatibilitiesController");
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in GetAll compatibilities: {ex.Message}", null, "CompatibilitiesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("allowed/{componentId:int}")]
        public ActionResult<IEnumerable<int>> GetAllowedIds(int componentId)
        {
            try
            {
                var ids = _service.GetAllowedCompatibleComponentIds(componentId).ToList();

                _logService.LogInfo($"Allowed compatible ids for componentId={componentId}: {ids.Count}.", null, "CompatibilitiesController");
                return Ok(ids);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error in GetAllowedIds componentId={componentId}: {ex.Message}", null, "CompatibilitiesController", ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("rule")]
        public ActionResult SetRule([FromBody] CompatibilityRuleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn("SetRule: invalid ModelState.", null, "CompatibilitiesController");
                return BadRequest(ModelState);
            }

            try
            {
                _service.SetRule(dto.ComponentId, dto.CompatibleWithComponentId, dto.IsAllowed);

                _logService.LogInfo(
                    $"Admin set rule ComponentId={dto.ComponentId}, CompatibleWith={dto.CompatibleWithComponentId}, IsAllowed={dto.IsAllowed}.",
                    null,
                    "CompatibilitiesController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in SetRule ComponentId={dto.ComponentId}, With={dto.CompatibleWithComponentId}: {ex.Message}",
                    null,
                    "CompatibilitiesController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("rule")]
        public ActionResult DeleteRule([FromQuery] int componentId, [FromQuery] int compatibleWithComponentId)
        {
            try
            {
                _service.DeleteRule(componentId, compatibleWithComponentId);

                _logService.LogInfo(
                    $"Admin deleted rule ComponentId={componentId}, CompatibleWith={compatibleWithComponentId}.",
                    null,
                    "CompatibilitiesController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in DeleteRule ComponentId={componentId}, With={compatibleWithComponentId}: {ex.Message}",
                    null,
                    "CompatibilitiesController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("setforcomponent")]
        public ActionResult SetForComponent([FromBody] CompatibilitySetForComponentDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logService.LogWarn($"SetForComponent: invalid ModelState componentId={dto.ComponentId}.", null, "CompatibilitiesController");
                return BadRequest(ModelState);
            }

            try
            {
                _service.SetAllowedForComponent(dto.ComponentId, dto.AllowedCompatibleIds);

                _logService.LogInfo(
                    $"Admin set allowed list for componentId={dto.ComponentId}, allowedCount={dto.AllowedCompatibleIds.Count}.",
                    null,
                    "CompatibilitiesController");

                return Ok();
            }
            catch (Exception ex)
            {
                _logService.LogError(
                    $"Error in SetForComponent componentId={dto.ComponentId}: {ex.Message}",
                    null,
                    "CompatibilitiesController",
                    ex.ToString());

                return BadRequest(ex.Message);
            }
        }
    }
}
