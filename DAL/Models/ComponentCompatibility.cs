using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[PrimaryKey("ComponentId", "CompatibleWithComponentId")]
[Table("ComponentCompatibility")]
public partial class ComponentCompatibility
{
    [Key]
    public int ComponentId { get; set; }

    [Key]
    public int CompatibleWithComponentId { get; set; }

    public bool IsAllowed { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("CompatibleWithComponentId")]
    [InverseProperty("ComponentCompatibilityCompatibleWithComponents")]
    public virtual Component CompatibleWithComponent { get; set; } = null!;

    [ForeignKey("ComponentId")]
    [InverseProperty("ComponentCompatibilityComponents")]
    public virtual Component Component { get; set; } = null!;
}
