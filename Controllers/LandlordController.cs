using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RentalManagementSystem.Services;

[Authorize]
public class LandlordController : Controller
{
    private readonly ILandlordDashboardService _dashboardService;

    public LandlordController(ILandlordDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

public async Task<IActionResult> Dashboard()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId))
    {
        // Log the issue or redirect to an error page
        return Unauthorized("User ID is not available.");
    }

    var model = await _dashboardService.GetDashboardDataAsync(userId);
    return View(model);
}
}