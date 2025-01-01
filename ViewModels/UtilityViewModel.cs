using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.ViewModels
{
	public class UtilityViewModel

	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Cost { get; set; }
	}

	public class UtilityReadingViewModel

	{
		public DateTime ReadingDate { get; set; }
		public string UtilityName { get; set; }
		public string TenantName { get; set; }
		public string HouseNumber { get; set; }
		public int PrevReading { get; set; }
		public int CurrentReading { get; set; }
		public int Consumption { get; set; }
		public int TotalCost { get; set; }
	}
}