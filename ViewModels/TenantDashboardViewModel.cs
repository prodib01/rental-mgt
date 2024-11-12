namespace RentalManagementSystem.ViewModels
{
    public class TenantDashboardViewModel
    {
        public int HouseNumber { get; set; }
        public string StreetName { get; set; }
        public int Rent { get; set; }
        public List<PendingBillsViewModel> PendingBills { get; set; }
    }

    public class PendingBillsViewModel
    {
        public string BillType { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
    }
}