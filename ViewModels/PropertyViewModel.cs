using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.ViewModels
{

public class PropertyViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Property type is required")]
    [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
    public string Description { get; set; }

    public int NumberOfHouses { get; set; }
}

public class PropertyListViewModel
{
    public List<PropertyViewModel> Properties { get; set; }
    public PropertyViewModel NewProperty { get; set; }
}

}
