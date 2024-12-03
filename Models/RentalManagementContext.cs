using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .HasMaxLength(50);

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


            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(h => h.House)
                .WithOne(h => h.Tenant)
                .HasForeignKey<User>(h => h.HouseId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });
    }

    public override int SaveChanges()
    {
        GenerateHouseNumbers();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateHouseNumbers();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateHouseNumbers()
    {
        foreach (var entry in ChangeTracker.Entries<House>().Where(e => e.State == EntityState.Added))
        {
            entry.Entity.HouseNumber = GenerateUniqueHouseNumber();
        }
    }

    private string GenerateUniqueHouseNumber()
    {
        // Example: Generate a unique identifier based on the current max ID
        int maxId = Houses.Max(h => (int?)h.Id) ?? 0;
        return $"HN-{maxId + 1:D5}";
    }
}
