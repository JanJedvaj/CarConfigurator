using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.ComponentTypes
{
    public class ComponentTypeCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, 100)]
        public int MinSelect { get; set; } = 1;

        [Range(0, 100)]
        public int MaxSelect { get; set; } = 1;

        public int? DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
