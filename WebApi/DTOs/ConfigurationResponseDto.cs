namespace WebApi.DTOs.Configurations
{
    public class ConfigurationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
