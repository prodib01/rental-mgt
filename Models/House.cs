using System.ComponentModel.DataAnnotations;

public class House
{
    public int Id { get; set; }

    [Required]
    public string HouseNumber { get; set; }

    [Required]
    public decimal Rent { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Foreign key to Property
    public int PropertyId { get; set; }
    public Property Property { get; set; }

    // Add navigation property to User
    public User? Tenant { get; set; }
}

