namespace WebApi.DTOs.Images
{
    public class ImageResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
        public string StoragePathOrUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public string? AltText { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
