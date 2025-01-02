using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.ViewModels
{
	public class HouseViewModel
	{
		public int Id { get; set; }

		[Required]
		public string HouseNumber { get; set; }

		[Required]
		[DataType(DataType.Currency)]
		public decimal Rent { get; set; }

		public bool IsOccupied { get; set; }

		[Required]
		[Display(Name = "Property Type")]
		public int PropertyId { get; set; }
		
		public string StatusMessage { get; set; }

		public IEnumerable<SelectListItem> Properties { get; set; }
		public IEnumerable<HouseListItemViewModel> Houses { get; set; }
		public string ButtonText { get; set; }
	}

	public class HouseListItemViewModel
	{
		public int Id { get; set; }
		public string HouseNumber { get; set; }
		public decimal Rent { get; set; }
		public int PropertyId { get; set; }
		public string PropertyType { get; set; }
		  public DateTime? VacantSince { get; set; }
	}
}