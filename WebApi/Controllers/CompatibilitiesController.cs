using DAL.Services.Compatibilities;
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

        public CompatibilitiesController(ICompatibilityService service)
        {
            _service = service;
        }

        // Admin: sva pravila
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<CompatibilityResponseDto>> GetAll()
        {
            var list = _service.GetAll()
                .Select(x => new CompatibilityResponseDto
                {
                    ComponentId = x.ComponentId,
                    CompatibleWithComponentId = x.CompatibleWithComponentId,
                    IsAllowed = x.IsAllowed,
                    CreatedAt = x.CreatedAt
                });

            return Ok(list);
        }

        // User/Admin: kompatibilni IDs za komponentu
        [HttpGet("allowed/{componentId:int}")]
        public ActionResult<IEnumerable<int>> GetAllowedIds(int componentId)
        {
            try
            {
                return Ok(_service.GetAllowedCompatibleComponentIds(componentId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin: set single rule
        [Authorize(Roles = "Admin")]
        [HttpPost("rule")]
        public ActionResult SetRule([FromBody] CompatibilityRuleCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                _service.SetRule(dto.ComponentId, dto.CompatibleWithComponentId, dto.IsAllowed);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin: delete rule
        [Authorize(Roles = "Admin")]
        [HttpDelete("rule")]
        public ActionResult DeleteRule([FromQuery] int componentId, [FromQuery] int compatibleWithComponentId)
        {
            try
            {
                _service.DeleteRule(componentId, compatibleWithComponentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin: mass set for component
        [Authorize(Roles = "Admin")]
        [HttpPost("setforcomponent")]
        public ActionResult SetForComponent([FromBody] CompatibilitySetForComponentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                _service.SetAllowedForComponent(dto.ComponentId, dto.AllowedCompatibleIds);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
