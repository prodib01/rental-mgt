namespace RentalManagementSystem.ViewModels
{
public class TenantDashboardViewModel
{
	public string HouseNumber { get; set; }
	public string StreetName { get; set; }
	public int Rent { get; set; }
	public List<PendingBillsViewModel> PendingBills { get; set; }
	public List<ActiveRequestViewModel> ActiveRequests { get; set; }
	public DateTime NextPaymentDueDate { get; set; }
	public int DaysUntilNextPayment { get; set; }
	public int DocumentsCount { get; set; }
}

	public class PendingBillsViewModel
	{
		public int? HouseId { get; set; }
		public string Description { get; set; }
		public string PaymentMethod { get; set; }
		public string BillType { get; set; }
		public decimal Amount { get; set; }
		public DateTime DueDate { get; set; }
	}
	
	public class ActiveRequestViewModel
	
	{
		public string RequestType { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}