using System.ComponentModel.DataAnnotations;

public class Tenant
{
    public int Id { get; set; }  // Primary Key

    [Required]
    public string FirstName { get; set; }  // Tenant's first name

    [Required]
    public string LastName { get; set; }  // Tenant's last name

    [Required]
    [EmailAddress]
    public string Email { get; set; }  // Tenant's email

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }  // Tenant's phone number

    public string Address { get; set; }  // Tenant's address
}
