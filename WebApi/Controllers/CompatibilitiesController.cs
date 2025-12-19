using AutoMapper;
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
        private readonly IMapper _mapper;

        public CompatibilitiesController(
            ICompatibilityService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<IEnumerable<CompatibilityResponseDto>> GetAll()
        {
            var entities = _service.GetAll();
            var dtos = entities.Select(x => _mapper.Map<CompatibilityResponseDto>(x));

            return Ok(dtos);
        }

        [HttpGet("allowed/{componentId:int}")]
        public ActionResult<IEnumerable<int>> GetAllowedIds(int componentId)
        {
            try
            {
                var ids = _service.GetAllowedCompatibleComponentIds(componentId);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("rule")]
        public ActionResult SetRule([FromBody] CompatibilityRuleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _service.SetRule(
                    dto.ComponentId,
                    dto.CompatibleWithComponentId,
                    dto.IsAllowed
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("rule")]
        public ActionResult DeleteRule(
            [FromQuery] int componentId,
            [FromQuery] int compatibleWithComponentId)
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

        [Authorize(Roles = "Admin")]
        [HttpPost("setforcomponent")]
        public ActionResult SetForComponent([FromBody] CompatibilitySetForComponentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _service.SetAllowedForComponent(
                    dto.ComponentId,
                    dto.AllowedCompatibleIds
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
