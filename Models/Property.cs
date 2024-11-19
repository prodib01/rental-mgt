using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Property
{
    public int Id { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    [DataType(DataType.Currency)]

    public string Description { get; set; }

    // Foreign key to User (Landlord)
    public int UserId { get; set; }
    public User User { get; set; }

    // Navigation property for Houses
    public ICollection<House> Houses { get; set; } = new List<House>();
}

