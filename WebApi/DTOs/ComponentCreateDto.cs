using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Components
{
    public class ComponentCreateDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = null!;

        [Required, StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public int? SortOrder { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }

        public int? ImageId { get; set; }
    }
}
