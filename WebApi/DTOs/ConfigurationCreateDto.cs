using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Configurations
{
    public class ConfigurationCreateDto
    {
        [Required]
        public int UserId { get; set; }   // kasnije možeš čitati iz JWT-a

        [Required, StringLength(150)]
        public string Name { get; set; } = null!;
    }
}
