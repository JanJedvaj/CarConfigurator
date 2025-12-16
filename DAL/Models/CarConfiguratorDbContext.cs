using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.Models;

public partial class CarConfiguratorDbContext : DbContext
{
    public CarConfiguratorDbContext()
    {
    }

    public CarConfiguratorDbContext(DbContextOptions<CarConfiguratorDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<CarConfiguration> CarConfigurations { get; set; }

    public virtual DbSet<CarConfigurationComponent> CarConfigurationComponents { get; set; }

    public virtual DbSet<Component> Components { get; set; }

    public virtual DbSet<ComponentCompatibility> ComponentCompatibilities { get; set; }

    public virtual DbSet<ComponentType> ComponentTypes { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-N9V5141;Database=CarConfiguratorDb;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.ApiLogs).HasConstraintName("FK_ApiLog_User");
        });

        modelBuilder.Entity<CarConfiguration>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Draft");

            entity.HasOne(d => d.User).WithMany(p => p.CarConfigurations).HasConstraintName("FK_CarConfiguration_User");
        });

        modelBuilder.Entity<CarConfigurationComponent>(entity =>
        {
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CarConfiguration).WithMany(p => p.CarConfigurationComponents).HasConstraintName("FK_ConfigComp_Config");

            entity.HasOne(d => d.Component).WithMany(p => p.CarConfigurationComponents).HasConstraintName("FK_ConfigComp_Component");
        });

        modelBuilder.Entity<Component>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.ComponentType).WithMany(p => p.Components)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Component_ComponentType");

            entity.HasOne(d => d.Image).WithMany(p => p.Components).HasConstraintName("FK_Component_Image");
        });

        modelBuilder.Entity<ComponentCompatibility>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsAllowed).HasDefaultValue(true);

            entity.HasOne(d => d.CompatibleWithComponent).WithMany(p => p.ComponentCompatibilityCompatibleWithComponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compat_Component_With");

            entity.HasOne(d => d.Component).WithMany(p => p.ComponentCompatibilityComponents).HasConstraintName("FK_Compat_Component");
        });

        modelBuilder.Entity<ComponentType>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxSelect).HasDefaultValue(1);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValue("User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
