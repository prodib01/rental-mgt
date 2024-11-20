using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RentalManagementSystem.ViewModels
{
    public class PropertyViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Property Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Property Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public int NumberOfHouses { get; set; }

        [Display(Name = "Owner")]
        public string LandlordName { get; set; }

        // Properties for list view
        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public string StatusMessage { get; set; }

        // Constructor to initialize the collection
        public PropertyViewModel()
        {
            Properties = new List<PropertyViewModel>();
        }
    }
}