namespace RentalManagementSystem.ViewModels
{
	public class LandlordDashboardViewModel
	{
		public int TotalProperties { get; set; }
		public int OccupiedProperties { get; set; }
		public int VacantProperties { get; set; }
		public int TotalTenants { get; set; }
		public decimal MonthlyRevenue { get; set; }
		public int PendingMaintenanceRequests { get; set; }
		public int UpcomingLeaseRenewals { get; set; }
		public List<RecentPaymentViewModel> RecentPayments { get; set; }
		public List<VacantHouseViewModel> VacantHouses { get; set; } // New property
	}

	public class RecentPaymentViewModel
	{
		public string TenantName { get; set; }
		public string PropertyAddress { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
	}

	public class VacantHouseViewModel // New class for vacant houses
	{
		public string HouseNumber { get; set; }
		public string Address { get; set; }
		public decimal MonthlyRent { get; set; }
		public DateTime? VacantSince { get; set; } // Nullable for better handling
	}
}
