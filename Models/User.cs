using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Models;

public class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public string Role { get; set; }

    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public string PhoneNumber { get; set; }

    public int? HouseId { get; set; }
    public House? House { get; set; }
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public bool PasswordChanged { get; set; }
}