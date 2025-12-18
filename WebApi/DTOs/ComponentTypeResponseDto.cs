namespace WebApi.DTOs.ComponentTypes
{
    public class ComponentTypeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
