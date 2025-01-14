using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RentalManagementSystem.Services;

[Authorize]
public class TenantController : Controller
{
	private readonly ITenantDashboardService _tenantDashboardService;

	public TenantController(ITenantDashboardService tenantDashboardService)
	{
		_tenantDashboardService = tenantDashboardService;
	}

	public async Task<IActionResult> Dashboard()
	{
		try
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from claims
			var dashboardData = await _tenantDashboardService.GetDashboardDataAsync(userId);
			return View(dashboardData); // Ensure dashboardData is passed to the view
		}
		catch (Exception ex)
		{
			// Handle exceptions and log if necessary
			ViewBag.Error = ex.Message;
			return View("Error");
		}
	}
}
