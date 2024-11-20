using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class LandlordController : Controller
{
    public IActionResult Dashboard()
    {
        var model = GetLandlordDashboardData();
        return View(model);

        
    }

    private LandlordDashboardViewModel GetLandlordDashboardData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // This would typically come from your database
        return new LandlordDashboardViewModel
        {
            TotalProperties = 5,
            OccupiedProperties = 3,
            VacantProperties = 2,
            TotalTenants = 3,
            MonthlyRevenue = 5000M,
            PendingMaintenanceRequests = 2,
            UpcomingLeaseRenewals = 1,
            RecentPayments = new List<RecentPaymentViewModel>
            {
                new RecentPaymentViewModel {
                    TenantName = "John Doe",
                    PropertyAddress = "123 Main St",
                    Amount = 1200M,
                    PaymentDate = DateTime.Now.AddDays(-2)
                }
            },

        };
    }
}