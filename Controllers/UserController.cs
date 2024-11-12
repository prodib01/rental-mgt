using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;


public class UserController : Controller
{
    public IActionResult UserDashboard()
    {
        // Retrieve data and pass it to the view
        var model = GetUserDashboardData();
        return View(model);
    }

    private UserDashboardModel GetUserDashboardData()
    {
        // Implement logic to fetch data for the user dashboard
        // and return an instance of UserDashboardModel
        return new UserDashboardModel
        {
            PendingOrders = 5,
            CompletedOrders = 10,
            TotalSpent = 1000,
            // Add other relevant data
        };
    }
}