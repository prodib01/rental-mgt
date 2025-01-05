using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;

namespace RentalManagementSystem.Services
{
	public interface IReportService
	{
		Task<FinancialReportDto> GenerateFinancialReportAsync(ReportFilterDto filter);
		Task<OccupancyReportDto> GenerateOccupancyReportAsync(ReportFilterDto filter);
		Task<MaintenanceReportDto> GenerateMaintenanceReportAsync(ReportFilterDto filter);
		Task<LeaseReportDto> GenerateLeaseReportAsync(ReportFilterDto filter);
		Task<byte[]> ExportReportToPdfAsync(string reportType, ReportFilterDto filter);
		Task<byte[]> ExportReportToExcelAsync(string reportType, ReportFilterDto filter);
	}

	public class ReportService : IReportService
	{
		private readonly RentalManagementContext _context;

		public ReportService(RentalManagementContext context)
		{
			_context = context;
		}

		public async Task<FinancialReportDto> GenerateFinancialReportAsync(ReportFilterDto filter)
		{
			var payments = await _context.Payments
				.Include(p => p.House)
				.Where(p => (!filter.HouseId.HasValue || p.HouseId == filter.HouseId) &&
							(!filter.StartDate.HasValue || p.PaymentDate >= filter.StartDate) &&
							(!filter.EndDate.HasValue || p.PaymentDate <= filter.EndDate))
				.ToListAsync();

			var PropertyAddress = filter.PropertyId.HasValue
				? await _context.Properties
					.Where(p => p.Id == filter.PropertyId)
					.Select(p => p.Address)
					.FirstOrDefaultAsync()
				: "All Properties";

			var report = new FinancialReportDto
			{
				StartDate = filter.StartDate ?? DateTime.MinValue,
				EndDate = filter.EndDate ?? DateTime.Now,
				TotalRevenue = payments.Sum(p => p.Amount),
				NetIncome = payments.Sum(p => p.Amount),
				PropertyAddress = PropertyAddress,
				Payments = payments.Select(p => new PaymentSummaryDto
				{
					Date = p.PaymentDate,
					Amount = p.Amount,
					PropertyAddress = p.House.Property.Address,
					TenantName = p.User.FullName
				}).ToList(),
			};

			return report;
		}

public async Task<OccupancyReportDto> GenerateOccupancyReportAsync(ReportFilterDto filter)
{
    var houses = await _context.Houses
        .Include(h => h.Property)
        .Include(h => h.Tenant) // Include the Tenant property (not CurrentTenant)
        .Where(h => !filter.PropertyId.HasValue || h.PropertyId == filter.PropertyId)
        .ToListAsync();

    var propertyAddress = filter.PropertyId.HasValue
        ? await _context.Properties
            .Where(p => p.Id == filter.PropertyId)
            .Select(p => p.Address)
            .FirstOrDefaultAsync()
        : "All Properties";

    var report = new OccupancyReportDto
    {
        GeneratedDate = DateTime.Now,
        TotalUnits = houses.Count,
        OccupiedUnits = houses.Count(h => h.Tenant != null), // Check if Tenant is not null
        PropertyAddress = propertyAddress,
UnitStatus = houses.Select(h => new UnitStatusDto
{
    UnitNumber = h.HouseNumber,
    IsOccupied = h.Tenant != null,
    TenantName = h.Tenant?.FullName,
    LeaseStartDate = h.Tenant?.Leases.FirstOrDefault()?.StartDate ?? DateTime.MinValue,
    LeaseEndDate = h.Tenant?.Leases.FirstOrDefault()?.EndDate ?? DateTime.MinValue
}).ToList(),
        VacantUnits = houses.Count(h => h.Tenant == null), // Check for vacant units where Tenant is null
        OccupancyRate = houses.Any()
            ? houses.Count(h => h.Tenant != null) * 100m / houses.Count
            : 0
    };

    return report;
}


public async Task<MaintenanceReportDto> GenerateMaintenanceReportAsync(ReportFilterDto filter)
{
    var requestsQuery = _context.Requests
        .Include(r => r.Tenant)
        .ThenInclude(t => t.House)
        .ThenInclude(h => h.Property)
        .Where(r =>
            (!filter.PropertyId.HasValue || r.Tenant.House.PropertyId == filter.PropertyId) &&
            (!filter.StartDate.HasValue || r.CreatedAt >= filter.StartDate) &&
            (!filter.EndDate.HasValue || r.CreatedAt <= filter.EndDate));

    // Fetch filtered data
    var requests = await requestsQuery.ToListAsync();

    // Determine property address
    var propertyAddress = filter.PropertyId.HasValue
        ? await _context.Properties
            .Where(p => p.Id == filter.PropertyId)
            .Select(p => p.Address)
            .FirstOrDefaultAsync() ?? "Unknown Property"
        : "All Properties";

var report = new MaintenanceReportDto
{
    StartDate = filter.StartDate ?? DateTime.MinValue,
    EndDate = filter.EndDate ?? DateTime.Now,
    TotalRequests = requests.Count,
    ResolvedRequests = requests.Count(r => r.Status == RequestStatus.Completed),
    PendingRequests = requests.Count(r => r.Status != RequestStatus.Completed),
    PropertyAddress = propertyAddress,
    Requests = requests.Select(r => new RequestSummaryDto
    {
        Title = r.Title,
        Description = r.Description,
        Status = r.Status.ToString(),
        Priority = r.Priority.ToString(),
        CreatedAt = r.CreatedAt,  // Remove the ?? DateTime.MinValue if CreatedAt is not nullable
        CompletedAt = r.CompletedAt ?? DateTime.MinValue,  // Add null coalescing if CompletedAt is nullable
        PropertyAddress = r.Tenant.House.Property.Address,
        UnitNumber = r.Tenant.House.HouseNumber
    }).ToList()
};

    return report;
}


public async Task<LeaseReportDto> GenerateLeaseReportAsync(ReportFilterDto filter)
{
    var now = DateTime.UtcNow;

    // Fetch leases with related tenant and property data
    var leases = await _context.Leases
        .Include(l => l.Tenant) // Include tenant data
        .Include(l => l.Tenant.House) // Include tenant's house
        .ThenInclude(h => h.Property) // Include house's property
        .Where(l => !filter.PropertyId.HasValue || l.Tenant.House.PropertyId == filter.PropertyId)
        .ToListAsync();

    // Determine property address or default to "All Properties"
    var propertyAddress = filter.PropertyId.HasValue
        ? await _context.Properties
            .Where(p => p.Id == filter.PropertyId)
            .Select(p => p.Address)
            .FirstOrDefaultAsync()
        : "All Properties";

    // Generate report
    var report = new LeaseReportDto
    {
        GeneratedDate = now,
        PropertyAddress = propertyAddress,
        ActiveLeases = leases.Count(l => l.EndDate > now),
        ExpiringLeases = leases.Count(l => l.EndDate <= now.AddMonths(2) && l.EndDate > now),
        RenewedLeases = leases.Count(l => l.UpdatedAt.HasValue), // Assume renewal updates `UpdatedAt`
        LeaseDetails = leases.Select(l => new LeaseSummaryDto
        {
            TenantName = l.Tenant.FullName,
            UnitNumber = l.Tenant.House.HouseNumber,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            MonthlyRent = l.Tenant.House.Rent,
            Status = l.EndDate > now ? "Active" : "Expired"
        }).ToList()
    };

    return report;
}


		public async Task<byte[]> ExportReportToPdfAsync(string reportType, ReportFilterDto filter)
		{
			using (var ms = new MemoryStream())
			{
				using (var doc = new Document(PageSize.A4, 50, 50, 25, 25))
				{
					var writer = PdfWriter.GetInstance(doc, ms);
					doc.Open();

					var title = new Paragraph($"{reportType.ToUpper()} REPORT",
						new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD));
					title.Alignment = Element.ALIGN_CENTER;
					doc.Add(title);
					doc.Add(new Paragraph("\n"));

					switch (reportType.ToLower())
					{
						case "financial":
							var financialReport = await GenerateFinancialReportAsync(filter);
							AddFinancialReportToPdf(doc, financialReport);
							break;
						case "occupancy":
							var occupancyReport = await GenerateOccupancyReportAsync(filter);
							AddOccupancyReportToPdf(doc, occupancyReport);
							break;
						case "maintenance":
							var maintenanceReport = await GenerateMaintenanceReportAsync(filter);
							AddMaintenanceReportToPdf(doc, maintenanceReport);
							break;
						case "lease":
							var leaseReport = await GenerateLeaseReportAsync(filter);
							AddLeaseReportToPdf(doc, leaseReport);
							break;
					}

					doc.Close();
				}

				return ms.ToArray();
			}
		}

		public async Task<byte[]> ExportReportToExcelAsync(string reportType, ReportFilterDto filter)
		{
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add($"{reportType} Report");

				switch (reportType.ToLower())
				{
					case "financial":
						var financialReport = await GenerateFinancialReportAsync(filter);
						AddFinancialReportToExcel(worksheet, financialReport);
						break;
					case "occupancy":
						var occupancyReport = await GenerateOccupancyReportAsync(filter);
						AddOccupancyReportToExcel(worksheet, occupancyReport);
						break;
					case "maintenance":
						var maintenanceReport = await GenerateMaintenanceReportAsync(filter);
						AddMaintenanceReportToExcel(worksheet, maintenanceReport);
						break;
					case "lease":
						var leaseReport = await GenerateLeaseReportAsync(filter);
						AddLeaseReportToExcel(worksheet, leaseReport);
						break;
				}

				return await package.GetAsByteArrayAsync();
			}
		}

		private void AddFinancialReportToPdf(Document doc, FinancialReportDto report)
		{
			doc.Add(new Paragraph($"Property: {report.PropertyAddress}"));
			doc.Add(new Paragraph($"Start Date: {report.StartDate:d}"));
			doc.Add(new Paragraph($"End Date: {report.EndDate:d}"));
			doc.Add(new Paragraph($"Total Revenue: {report.TotalRevenue:C}"));
			doc.Add(new Paragraph($"Net Income: {report.NetIncome:C}"));
			doc.Add(new Paragraph("\n"));
			// Add more details as needed
		}

		private void AddOccupancyReportToPdf(Document doc, OccupancyReportDto report)
		{
			doc.Add(new Paragraph($"Property: {report.PropertyAddress}"));
			doc.Add(new Paragraph($"Generated Date: {report.GeneratedDate:d}"));
			doc.Add(new Paragraph($"Total Units: {report.TotalUnits}"));
			doc.Add(new Paragraph($"Occupied Units: {report.OccupiedUnits}"));
			doc.Add(new Paragraph($"Vacant Units: {report.VacantUnits}"));
			doc.Add(new Paragraph($"Occupancy Rate: {report.OccupancyRate:P}"));
			doc.Add(new Paragraph("\n"));
			// Add more details as needed
		}

		private void AddMaintenanceReportToPdf(Document doc, MaintenanceReportDto report)
		{
			doc.Add(new Paragraph($"Property: {report.PropertyAddress}"));
			doc.Add(new Paragraph($"Start Date: {report.StartDate:d}"));
			doc.Add(new Paragraph($"End Date: {report.EndDate:d}"));
			doc.Add(new Paragraph($"Total Requests: {report.TotalRequests}"));
			doc.Add(new Paragraph($"Resolved Requests: {report.ResolvedRequests}"));
			doc.Add(new Paragraph($"Pending Requests: {report.PendingRequests}"));
			doc.Add(new Paragraph("\n"));
			// Add more details as needed
		}

		private void AddLeaseReportToPdf(Document doc, LeaseReportDto report)
		{
			doc.Add(new Paragraph($"Property: {report.PropertyAddress}"));
			doc.Add(new Paragraph($"Generated Date: {report.GeneratedDate:d}"));
			doc.Add(new Paragraph($"Active Leases: {report.ActiveLeases}"));
			doc.Add(new Paragraph($"Expiring Leases: {report.ExpiringLeases}"));
			doc.Add(new Paragraph($"Renewed Leases: {report.RenewedLeases}"));
			doc.Add(new Paragraph("\n"));
			// Add more details as needed
		}

		private void AddFinancialReportToExcel(ExcelWorksheet worksheet, FinancialReportDto report)
		{
			worksheet.Cells[1, 1].Value = "Property";
			worksheet.Cells[1, 2].Value = report.PropertyAddress;
			worksheet.Cells[2, 1].Value = "Start Date";
			worksheet.Cells[2, 2].Value = report.StartDate;
			worksheet.Cells[3, 1].Value = "End Date";
			worksheet.Cells[3, 2].Value = report.EndDate;
			worksheet.Cells[4, 1].Value = "Total Revenue";
			worksheet.Cells[4, 2].Value = report.TotalRevenue;
			worksheet.Cells[6, 1].Value = "Net Income";
			worksheet.Cells[6, 2].Value = report.NetIncome;
			// Add more details as needed
		}

		private void AddOccupancyReportToExcel(ExcelWorksheet worksheet, OccupancyReportDto report)
		{
			worksheet.Cells[1, 1].Value = "Property";
			worksheet.Cells[1, 2].Value = report.PropertyAddress;
			worksheet.Cells[2, 1].Value = "Generated Date";
			worksheet.Cells[2, 2].Value = report.GeneratedDate;
			worksheet.Cells[3, 1].Value = "Total Units";
			worksheet.Cells[3, 2].Value = report.TotalUnits;
			worksheet.Cells[4, 1].Value = "Occupied Units";
			worksheet.Cells[4, 2].Value = report.OccupiedUnits;
			worksheet.Cells[5, 1].Value = "Vacant Units";
			worksheet.Cells[5, 2].Value = report.VacantUnits;
			worksheet.Cells[6, 1].Value = "Occupancy Rate";
			worksheet.Cells[6, 2].Value = report.OccupancyRate;
			// Add more details as needed
		}

		private void AddMaintenanceReportToExcel(ExcelWorksheet worksheet, MaintenanceReportDto report)
		{
			worksheet.Cells[1, 1].Value = "Property";
			worksheet.Cells[1, 2].Value = report.PropertyAddress;
			worksheet.Cells[2, 1].Value = "Start Date";
			worksheet.Cells[2, 2].Value = report.StartDate;
			worksheet.Cells[3, 1].Value = "End Date";
			worksheet.Cells[3, 2].Value = report.EndDate;
			worksheet.Cells[4, 1].Value = "Total Requests";
			worksheet.Cells[4, 2].Value = report.TotalRequests;
			worksheet.Cells[5, 1].Value = "Resolved Requests";
			worksheet.Cells[5, 2].Value = report.ResolvedRequests;
			worksheet.Cells[6, 1].Value = "Pending Requests";
			worksheet.Cells[6, 2].Value = report.PendingRequests;
			// Add more details as needed
		}

		private void AddLeaseReportToExcel(ExcelWorksheet worksheet, LeaseReportDto report)
		{
			worksheet.Cells[1, 1].Value = "Property";
			worksheet.Cells[1, 2].Value = report.PropertyAddress;
			worksheet.Cells[2, 1].Value = "Generated Date";
			worksheet.Cells[2, 2].Value = report.GeneratedDate;
			worksheet.Cells[3, 1].Value = "Active Leases";
			worksheet.Cells[3, 2].Value = report.ActiveLeases;
			worksheet.Cells[4, 1].Value = "Expiring Leases";
			worksheet.Cells[4, 2].Value = report.ExpiringLeases;
			worksheet.Cells[5, 1].Value = "Renewed Leases";
			worksheet.Cells[5, 2].Value = report.RenewedLeases;
			// Add more details as needed
		}
	}
}
