using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RentalManagementSystem.Models;


public class RentalManagementContext : DbContext
{
	public RentalManagementContext(DbContextOptions<RentalManagementContext> options) : base(options)
	{
	}

	public DbSet<Property> Properties { get; set; }
	public DbSet<House> Houses { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<Payment> Payments { get; set; }
	public DbSet<Lease> Leases { get; set; }
	public DbSet<LeaseDocument> LeaseDocuments { get; set; }

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

		// Configure Payment entity
		modelBuilder.Entity<Payment>(entity =>
		{
			entity.ToTable("Payments");

			entity.Property(e => e.Amount)
				.HasColumnType("decimal(18,2)")
				.IsRequired();

			entity.Property(e => e.PaymentDate)
				.IsRequired();

			entity.Property(e => e.PaymentMethod)
				.HasConversion<string>()
				.HasMaxLength(50);

			entity.Property(e => e.PaymentStatus)
				.HasConversion<string>()
				.HasMaxLength(50);

			entity.Property(e => e.PaymentReference)
				.HasMaxLength(100);

			entity.Property(e => e.Description)
				.HasMaxLength(500);


			entity.HasOne(p => p.House)
			.WithMany(h => h.Payments)
			.HasForeignKey(p => p.HouseId);

			entity.HasOne(p => p.User)
			.WithMany(u => u.Payments)
			.HasForeignKey(p => p.UserId);
		});

		// Configure Lease entity
		modelBuilder.Entity<Lease>(entity =>
		{
			entity.ToTable("Leases");

			// Configure Id as the primary key
			entity.HasKey(e => e.Id);

			// Configure TenantId as a required foreign key
			entity.Property(e => e.TenantId)
				.IsRequired();

			// Configure relationship between Lease and User
			entity.HasOne(e => e.Tenant)
				.WithMany() // Assuming User doesn't have a navigation property for leases
				.HasForeignKey(e => e.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure StartDate
			entity.Property(e => e.StartDate)
				.IsRequired();

			// Configure EndDate
			entity.Property(e => e.EndDate)
				.IsRequired();

			// Configure CreatedAt
			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()")
				.ValueGeneratedOnAdd();

			// Configure UpdatedAt
			entity.Property(e => e.UpdatedAt)
				.ValueGeneratedOnUpdate();

			// Add additional constraints or indexes if necessary
		});

		modelBuilder.Entity<LeaseDocument>(entity =>

		{
			entity.ToTable("LeaseDocuments");

			entity.HasKey(e => e.Id);

			entity.Property(e => e.LeaseId)
			.IsRequired();

			entity.HasOne(e => e.Lease)
			.WithMany()
			.HasForeignKey(e => e.LeaseId)
			.OnDelete(DeleteBehavior.Cascade);

			entity.Property(e => e.DocumentPath)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.DocumentName)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.GeneratedAt)
				.IsRequired();

			entity.Property(e => e.Version)
				.IsRequired()
				.HasMaxLength(50);
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
