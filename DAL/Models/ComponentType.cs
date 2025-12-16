using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("ComponentType")]
[Index("Name", Name = "UQ_ComponentType_Name", IsUnique = true)]
public partial class ComponentType
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int MinSelect { get; set; }

    public int MaxSelect { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("ComponentType")]
    public virtual ICollection<Component> Components { get; set; } = new List<Component>();
}
