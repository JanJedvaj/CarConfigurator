namespace WebApi.DTOs.Compatibilities
{
    public class CompatibilityResponseDto
    {
        public int ComponentId { get; set; }
        public int CompatibleWithComponentId { get; set; }
        public bool IsAllowed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
