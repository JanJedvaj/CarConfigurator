using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[PrimaryKey("CarConfigurationId", "ComponentId")]
[Table("CarConfigurationComponent")]
public partial class CarConfigurationComponent
{
    [Key]
    public int CarConfigurationId { get; set; }

    [Key]
    public int ComponentId { get; set; }

    public DateTime AddedAt { get; set; }

    [ForeignKey("CarConfigurationId")]
    [InverseProperty("CarConfigurationComponents")]
    public virtual CarConfiguration CarConfiguration { get; set; } = null!;

    [ForeignKey("ComponentId")]
    [InverseProperty("CarConfigurationComponents")]
    public virtual Component Component { get; set; } = null!;
}
