using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.Services;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace RentalManagementSystem.Controllers
{
	[Authorize]
	[Route("Landlord/Reports")]
	public class ReportsController : Controller
	{
		private readonly IReportService _reportService;
		private readonly RentalManagementContext _context;
		private readonly ILogger<AddTenantsController> _logger;

		public ReportsController(
			IReportService reportService,
			RentalManagementContext context, ILogger<AddTenantsController> logger)
		{
			_reportService = reportService;
			_context = context;
			_logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized();
			}

			var properties = await _context.Properties
				.Where(p => p.UserId == userId)
				.Select(p => new SelectListItem
				{
					Value = p.Id.ToString(),
					Text = p.Address
				})
				.ToListAsync();

			var recentReports = new List<RecentReportViewModel>();

			// Financial Reports
			var financialReports = await _context.FinancialReports
				.Where(r => r.Property.UserId == userId)
				.OrderByDescending(r => r.EndDate)
				.Take(5)
				.Select(r => new RecentReportViewModel
				{
					Id = r.Id,
					ReportType = "Financial",
					PropertyId = r.PropertyId,
					PropertyAddress = r.Property.Address,
					GeneratedDate = r.EndDate,
					StartDate = r.StartDate,
					EndDate = r.EndDate,
					KeyMetric = r.TotalRevenue
				})
				.ToListAsync();
			recentReports.AddRange(financialReports);
			
			var occupancyReports = await _context.OccupancyReports
				.Where(r => r.Property.UserId == userId)
				.OrderByDescending(r => r.GeneratedDate)
				.Take(5)
				.Select(r => new RecentReportViewModel
				{
					Id = r.Id,
					ReportType = "Occupancy",
					PropertyId = r.PropertyId,
					PropertyAddress = r.Property.Address,
					GeneratedDate = r.GeneratedDate,
					KeyMetric = r.OccupancyRate
				})
				.ToListAsync();
				
			var maintenanceReports = await _context.MaintenanceReports
				.Where(r => r.Property.UserId == userId)
				.OrderByDescending(r => r.EndDate)
				.Take(5)
				.Select(r => new RecentReportViewModel
				{
					Id = r.Id,
					ReportType = "Maintenance",
					PropertyId = r.PropertyId,
					PropertyAddress = r.Property.Address,
					GeneratedDate = r.EndDate,
					StartDate = r.StartDate,
					EndDate = r.EndDate,
					KeyMetric = r.TotalMaintenanceCost
				})
				.ToListAsync();
				
			var leaseReports = await _context.LeaseReports
				.Where(r => r.Property.UserId == userId)
				.OrderByDescending(r => r.GeneratedDate)
				.Take(5)
				.Select(r => new RecentReportViewModel
				{
					Id = r.Id,
					ReportType = "Lease",
					PropertyId = r.PropertyId,
					PropertyAddress = r.Property.Address,
					GeneratedDate = r.GeneratedDate,
					KeyMetric = r.ActiveLeases
				})
				.ToListAsync();		

			var viewModel = new ReportIndexViewModel
			{
				Properties = properties,
				RecentReports = recentReports.OrderByDescending(r => r.GeneratedDate).Take(10).ToList(),
			};


			return View("~/Views/Landlord/Reports.cshtml", viewModel);
		}

		[HttpPost("GenerateReport")]
		public async Task<IActionResult> GenerateReport([FromBody] ReportFilterViewModel filter)
		{
			try
			{
				_logger.LogInformation($"Received report request for type: {filter?.ReportType}");

				if (!ModelState.IsValid)
				{
					_logger.LogWarning("Invalid model state: " + string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
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

				_logger.LogInformation($"Processing report with filter: PropertyId={filterDto.PropertyId}, StartDate={filterDto.StartDate}, EndDate={filterDto.EndDate}");

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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating report");
				return StatusCode(500, "An error occurred while generating the report");
			}
		}

		[HttpPost("ExportReport")]
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