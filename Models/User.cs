// Models/User.cs
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }  

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } 

    [Required]
    public string Role { get; set; } 
}
