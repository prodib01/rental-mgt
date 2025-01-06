using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Services;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementSystem.Controllers
{
	[Authorize]
[Route("Landlord/Reports")]
	public class ReportsController : Controller
	{
		private readonly IReportService _reportService;

		public ReportsController(IReportService reportService)
		{
			_reportService = reportService;
		}

		public IActionResult Index()
		{
			return View("~/Views/Landlord/Reports.cshtml", new ReportIndexViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> GenerateReport([FromBody] ReportFilterViewModel filter)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var filterDto = new ReportFilterDto
			{
				StartDate = filter.StartDate,
				EndDate = filter.EndDate,
				PropertyId = filter.PropertyId,
				HouseId = filter.HouseId,
				ReportType = filter.ReportType
			};

			object reportData = null;

			switch (filter.ReportType.ToLower())
			{
				case "financial":
					reportData = await _reportService.GenerateFinancialReportAsync(filterDto);
					break;
				case "occupancy":
					reportData = await _reportService.GenerateOccupancyReportAsync(filterDto);
					break;
				case "maintenance":
					reportData = await _reportService.GenerateMaintenanceReportAsync(filterDto);
					break;
				case "lease":
					reportData = await _reportService.GenerateLeaseReportAsync(filterDto);
					break;
				default:
					return BadRequest("Invalid report type");
			}

			return Json(reportData);
		}

		[HttpPost]
		public async Task<IActionResult> ExportReport([FromBody] ExportRequestViewModel request)
		{
			var filterDto = new ReportFilterDto
			{
				StartDate = request.Filter.StartDate,
				EndDate = request.Filter.EndDate,
				PropertyId = request.Filter.PropertyId,
				HouseId = request.Filter.HouseId,
				ReportType = request.Filter.ReportType
			};

			byte[] fileContent;
			string contentType;
			string fileName;

			if (request.Format.ToLower() == "pdf")
			{
				fileContent = await _reportService.ExportReportToPdfAsync(request.Filter.ReportType, filterDto);
				contentType = "application/pdf";
				fileName = $"{request.Filter.ReportType}-report.pdf";
			}
			else if (request.Format.ToLower() == "excel")
			{
				fileContent = await _reportService.ExportReportToExcelAsync(request.Filter.ReportType, filterDto);
				contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				fileName = $"{request.Filter.ReportType}-report.xlsx";
			}
			else
			{
				return BadRequest("Invalid export format");
			}

			return File(fileContent, contentType, fileName);
		}
	}
}