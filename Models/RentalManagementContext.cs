using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class RentalManagementContext : DbContext
{
    public RentalManagementContext(DbContextOptions<RentalManagementContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<House> Houses { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Disable cascade delete globally
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
        {
            fk.DeleteBehavior = DeleteBehavior.NoAction;
        }

        // Configure Property entity
        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties");

            entity.HasOne(p => p.User)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        });

        // Configure House entity
        modelBuilder.Entity<House>(entity =>
        {
            entity.ToTable("Houses");

            entity.Property(e => e.HouseNumber)
                .IsRequired()
                .HasMaxLength(50);  // Adjust max length as needed

            entity.Property(e => e.Rent)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Relationship with Property
            entity.HasOne(h => h.Property)
                .WithMany(p => p.Houses)
                .HasForeignKey(h => h.PropertyId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasColumnType("varchar(128)");

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(h => h.House)
                .WithOne(h => h.User) // This is invalid because House doesn't have a Users property
                .HasForeignKey<User>(h => h.HouseId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

        });
    }
}