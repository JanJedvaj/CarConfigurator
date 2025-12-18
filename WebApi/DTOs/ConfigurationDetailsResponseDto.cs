using WebApi.DTOs.Components;

namespace WebApi.DTOs.Configurations
{
    public class ConfigurationDetailsResponseDto : ConfigurationResponseDto
    {
        public List<ComponentResponseDto> Components { get; set; } = new();
    }
}
