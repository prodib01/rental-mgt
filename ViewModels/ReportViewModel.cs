using System;
using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.DTOs;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace RentalManagementSystem.ViewModels
{
public class ReportIndexViewModel
{
	public ReportFilterViewModel Filter { get; set; } = new ReportFilterViewModel();
	public IEnumerable<SelectListItem> Properties { get; set; }
}

	public class ReportFilterViewModel
	{
		[Required(ErrorMessage = "Report type is required")]
		public string ReportType { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? PropertyId { get; set; }
		public int? HouseId { get; set; }
	}

	public class ExportRequestViewModel
	{
		public ReportFilterViewModel Filter { get; set; }
		public string Format { get; set; }
	}
}