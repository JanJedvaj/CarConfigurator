namespace WebApi.DTOs.Components
{
    public class ComponentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }

        public int ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = null!;

        public int? ImageId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
