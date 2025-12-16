using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("CarConfiguration")]
public partial class CarConfiguration
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalPrice { get; set; }

    [InverseProperty("CarConfiguration")]
    public virtual ICollection<CarConfigurationComponent> CarConfigurationComponents { get; set; } = new List<CarConfigurationComponent>();

    [ForeignKey("UserId")]
    [InverseProperty("CarConfigurations")]
    public virtual User User { get; set; } = null!;
}
