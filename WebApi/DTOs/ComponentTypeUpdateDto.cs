using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.ComponentTypes
{
    public class ComponentTypeUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, 100)]
        public int MinSelect { get; set; }

        [Range(0, 100)]
        public int MaxSelect { get; set; }

        public int? DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
