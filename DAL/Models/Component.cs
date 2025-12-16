using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("Component")]
[Index("Name", Name = "UQ_Component_Name", IsUnique = true)]
public partial class Component
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public int? SortOrder { get; set; }

    public int ComponentTypeId { get; set; }

    public int? ImageId { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("Component")]
    public virtual ICollection<CarConfigurationComponent> CarConfigurationComponents { get; set; } = new List<CarConfigurationComponent>();

    [InverseProperty("CompatibleWithComponent")]
    public virtual ICollection<ComponentCompatibility> ComponentCompatibilityCompatibleWithComponents { get; set; } = new List<ComponentCompatibility>();

    [InverseProperty("Component")]
    public virtual ICollection<ComponentCompatibility> ComponentCompatibilityComponents { get; set; } = new List<ComponentCompatibility>();

    [ForeignKey("ComponentTypeId")]
    [InverseProperty("Components")]
    public virtual ComponentType ComponentType { get; set; } = null!;

    [ForeignKey("ImageId")]
    [InverseProperty("Components")]
    public virtual Image? Image { get; set; }
}
