using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Configurations
{
    public class ConfigurationAddComponentDto
    {
        [Required]
        public int ComponentId { get; set; }
    }
}
