using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalManagementSystem.ViewModels
{
public class AddTenantViewModel
{
    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email Address is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Display(Name = "Temporary Password")]
    public string? Password { get; set; }

    [Display(Name = "Confirm Temporary Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "House Number is required")]
    [Display(Name = "House Number")]
    public string HouseNumber { get; set; }
}

    public class TenantListViewModel
    {
        public List<TenantViewModel> Tenants { get; set; } = new List<TenantViewModel>();
        public AddTenantViewModel NewTenant { get; set; } = new AddTenantViewModel();
        public List<SelectListItem> AvailableHouses { get; set; } = new List<SelectListItem>();
    }

    public class TenantViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string HouseNumber { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}