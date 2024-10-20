using System.ComponentModel.DataAnnotations;

public class Landlord
{
    public int Id { get; set; }  // Primary Key

    [Required]
    public string FirstName { get; set; }  // Landlord's first name

    [Required]
    public string LastName { get; set; }  // Landlord's last name

    [Required]
    [EmailAddress]
    public string Email { get; set; }  // Landlord's email

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }  // Landlord's phone number

    public string Address { get; set; }  // Landlord's address
}
