using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Compatibilities
{
    public class CompatibilityRuleCreateDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required]
        public int CompatibleWithComponentId { get; set; }

        public bool IsAllowed { get; set; } = true;
    }
}
