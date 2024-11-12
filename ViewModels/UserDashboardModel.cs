namespace RentalManagementSystem.ViewModels
{
    public class UserDashboardModel
    {
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalSpent { get; set; }
        // Add more properties as needed
    }
}