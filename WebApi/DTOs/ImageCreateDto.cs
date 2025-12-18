using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Images
{
    public class ImageCreateDto
    {
        [Required, StringLength(255)]
        public string FileName { get; set; } = null!;

        [Required, StringLength(100)]
        public string ContentType { get; set; } = null!;

        [Range(1, long.MaxValue)]
        public long Length { get; set; }

        [Required, StringLength(500)]
        public string StoragePathOrUrl { get; set; } = null!;

        [StringLength(200)]
        public string? AltText { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
