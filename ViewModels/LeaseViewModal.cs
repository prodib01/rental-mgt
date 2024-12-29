using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalManagementSystem.ViewModels
{
	public class LeaseViewModel
	{
		public int Id { get; set; }
		public int TenantId { get; set; }
		public string TenantName { get; set; }
		public string TenantEmail { get; set; }
		public string HouseNumber { get; set; }
		public string PropertyAddress { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal MonthlyRent { get; set; }
		public IEnumerable<SelectListItem> Tenants { get; set; }
		public IEnumerable<LeaseListItemViewModel> Leases { get; set; }
		
	}
	
	public class LeaseListItemViewModel
	
	{
		public int Id { get; set; }
		public int TenantId { get; set; }
		public string TenantName { get; set; }
		public string TenantEmail { get; set; }
		public string HouseNumber { get; set; }
		public string PropertyAddress { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal MonthlyRent { get; set; }
	}
	
	
}