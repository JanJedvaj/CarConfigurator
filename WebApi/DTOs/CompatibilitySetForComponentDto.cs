using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Compatibilities
{
    public class CompatibilitySetForComponentDto
    {
        [Required]
        public int ComponentId { get; set; }

        public List<int> AllowedCompatibleIds { get; set; } = new();
    }
}
