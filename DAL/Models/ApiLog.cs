using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("ApiLog")]
[Index("Timestamp", Name = "IX_ApiLog_Timestamp", AllDescending = true)]
public partial class ApiLog
{
    [Key]
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    [StringLength(20)]
    public string Level { get; set; } = null!;

    [StringLength(1000)]
    public string Message { get; set; } = null!;

    public int? UserId { get; set; }

    [StringLength(50)]
    public string? Source { get; set; }

    public string? Details { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ApiLogs")]
    public virtual User? User { get; set; }
}
