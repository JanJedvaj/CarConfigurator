using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

[Table("User")]
[Index("Email", Name = "UQ_User_Email", IsUnique = true)]
[Index("UserName", Name = "UQ_User_UserName", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(256)]
    public string PasswordSalt { get; set; } = null!;


    [StringLength(20)]
    public string Role { get; set; } = null!;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();

    [InverseProperty("User")]
    public virtual ICollection<CarConfiguration> CarConfigurations { get; set; } = new List<CarConfiguration>();
}
