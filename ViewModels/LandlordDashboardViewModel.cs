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
        // public List<PropertyViewModel> VacantPropertiesList { get; set; }
    }

    public class RecentPaymentViewModel
    {
        public string TenantName { get; set; }
        public string PropertyAddress { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }


}