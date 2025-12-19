using DAL.Models;
using DAL.Services.Configurations;
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

        public ConfigurationsController(IConfigurationService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId:int}")]
        public ActionResult<IEnumerable<ConfigurationResponseDto>> GetByUser(int userId)
        {
            var list = _service.GetUserConfigurations(userId)
                .Select(x => new ConfigurationResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    TotalPrice = x.TotalPrice
                });

            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ConfigurationDetailsResponseDto> GetDetails(int id)
        {
            var cfg = _service.GetConfigurationDetails(id);
            if (cfg == null) return NotFound();

            var dto = new ConfigurationDetailsResponseDto
            {
                Id = cfg.Id,
                UserId = cfg.UserId,
                Name = cfg.Name,
                CreatedAt = cfg.CreatedAt,
                UpdatedAt = cfg.UpdatedAt,
                TotalPrice = cfg.TotalPrice,
                Components = cfg.CarConfigurationComponents
                    .Select(cc => cc.Component)
                    .Where(c => c != null)
                    .Select(c => new ComponentResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Title = c.Title,
                        Description = c.Description,
                        Price = c.Price,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder,
                        ComponentTypeId = c.ComponentTypeId,
                        ComponentTypeName = c.ComponentType?.Name ?? "",
                        ImageId = c.ImageId,
                        ImageUrl = c.Image?.StoragePathOrUrl
                    })
                    .ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public ActionResult Create([FromBody] ConfigurationCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var id = _service.CreateConfiguration(dto.UserId, dto.Name.Trim());
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public ActionResult Update([FromBody] ConfigurationUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var existing = _service.GetConfiguration(dto.Id);
                if (existing == null) return NotFound();

                existing.Name = dto.Name.Trim();
                _service.UpdateConfiguration(existing);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _service.DeleteConfiguration(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Add component (provjere kompatibilnosti + maxSelect su u service-u)
        [HttpPost("{configurationId:int}/components")]
        public ActionResult AddComponent(int configurationId, [FromBody] ConfigurationAddComponentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                _service.AddComponentToConfiguration(configurationId, dto.ComponentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{configurationId:int}/components/{componentId:int}")]
        public ActionResult RemoveComponent(int configurationId, int componentId)
        {
            try
            {
                _service.RemoveComponentFromConfiguration(configurationId, componentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{configurationId:int}/components")]
        public ActionResult Clear(int configurationId)
        {
            try
            {
                _service.ClearConfiguration(configurationId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
