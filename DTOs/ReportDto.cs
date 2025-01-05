using System;
using System.Collections.Generic;

namespace RentalManagementSystem.DTOs
{
	public class ReportFilterDto
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? HouseId { get; set; }
		public int? PropertyId { get; set; }
		
		// Consider using an enum for ReportType to restrict valid values
		public string ReportType { get; set; }
	}

	public class FinancialReportDto
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; } // Nullable in case it's an ongoing report
		public decimal TotalRevenue { get; set; }
		public decimal NetIncome { get; set; }
		public List<PaymentSummaryDto> Payments { get; set; }
		public string PropertyAddress { get; set; }
	}

	public class OccupancyReportDto
	{
		public DateTime GeneratedDate { get; set; }
		public int TotalUnits { get; set; }
		public int OccupiedUnits { get; set; }
		public int VacantUnits { get; set; }
		public decimal OccupancyRate { get; set; }
		public List<UnitStatusDto> UnitStatus { get; set; }
		public string PropertyAddress { get; set; }
	}

	public class MaintenanceReportDto
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; } // Nullable for ongoing maintenance tracking
		public int TotalRequests { get; set; }
		public int ResolvedRequests { get; set; }
		public int PendingRequests { get; set; }
		public decimal TotalMaintenanceCost { get; set; }
		public List<RequestSummaryDto> Requests { get; set; }
		public string PropertyAddress { get; set; }
	}

	public class LeaseReportDto
	{
		public DateTime GeneratedDate { get; set; }
		public int ActiveLeases { get; set; }
		public int ExpiringLeases { get; set; }
		public int RenewedLeases { get; set; }
		public List<LeaseSummaryDto> LeaseDetails { get; set; }
		public string PropertyAddress { get; set; }
	}

	// Supporting DTOs
	public class PaymentSummaryDto
	{
		public DateTime Date { get; set; }
		public decimal Amount { get; set; }
		public string PropertyAddress { get; set; }
		public string TenantName { get; set; }
		public int TenantId { get; set; } 
	}

	public class UnitStatusDto
	{
		public int UnitId { get; set; }
		public string Status { get; set; }
		public string TenantName { get; set; }
		
		public string UnitNumber { get; set; }
		public DateTime LeaseEndDate { get; set; }
		public bool IsOccupied { get; set; }
		public DateTime LeaseStartDate { get; set; }
	}

	public class RequestSummaryDto
	{
		public string Status { get; set; }
		public string UnitNumber { get; set; }
		public string PropertyAddress { get; set; }
		public DateTime CompletedAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public string Priority { get; set; }
			public string Title { get; set; }
	public string Description { get; set; }
	}

	public class LeaseSummaryDto
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string TenantName { get; set; }
		public decimal MonthlyRent { get; set; }
		public string Status { get; set; }
		public string UnitNumber { get; set; }
	}
}

