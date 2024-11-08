using Microsoft.EntityFrameworkCore;

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
        // Configure Property entity
        modelBuilder.Entity<Property>(entity =>
        {
            entity.Property(e => e.Rent)
                .HasColumnType("decimal(18,2)"); // Specify the precision and scale here
        });

        // Configure House entity
        modelBuilder.Entity<House>(entity =>
        {
            entity.Property(e => e.Rent)
                .HasColumnType("decimal(18,2)"); // Specify the precision and scale here
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.PasswordHash)
                .HasColumnType("varchar(128)");
        });
    }
}
