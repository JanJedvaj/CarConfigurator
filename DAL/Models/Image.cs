using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("Image")]
public partial class Image
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [StringLength(100)]
    public string ContentType { get; set; } = null!;

    public long Length { get; set; }

    [StringLength(500)]
    public string StoragePathOrUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    [StringLength(200)]
    public string? AltText { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    [InverseProperty("Image")]
    public virtual ICollection<Component> Components { get; set; } = new List<Component>();
}
