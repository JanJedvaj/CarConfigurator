using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Configurations
{
    public class ConfigurationUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = null!;
    }
}
