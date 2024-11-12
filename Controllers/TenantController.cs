using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// [Authorize(Policy = "TenantOnly")]
public class TenantController : Controller
{
    public IActionResult Dashboard()
    {
        var model = GetTenantDashboardData();
        return View(model);
    }

    private TenantDashboardViewModel GetTenantDashboardData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // userId is a string, so no need for .Value here
        return new TenantDashboardViewModel
        {
            HouseNumber = 123, // Change this from "123" to 123 (int)
            StreetName = "Main Street",
            Rent = 1200,
            PendingBills = new List<PendingBillsViewModel>
        {
            new PendingBillsViewModel { BillType = "Rent", Amount = 1200, DueDate = DateTime.Now.AddDays(15) },
            new PendingBillsViewModel { BillType = "Electricity", Amount = 50, DueDate = DateTime.Now.AddDays(30) },
            new PendingBillsViewModel { BillType = "Water", Amount = 20, DueDate = DateTime.Now.AddDays(45) }
        }
        };
    }

}