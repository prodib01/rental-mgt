using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RentalManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using System;
using RentalManagementSystem.Models;


[Authorize]
public class TenantController : Controller
{
	private readonly ITenantDashboardService _tenantDashboardService;
	private readonly RentalManagementContext _context;

	public TenantController(ITenantDashboardService tenantDashboardService, RentalManagementContext context)
	{
		_tenantDashboardService = tenantDashboardService;
		_context = context;
	}

	public async Task<IActionResult> Dashboard()
	{
		try
		{
			// Get user ID from claims
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized();
			}

			// Fetch tenant and their associated house
			var user = await _context.Users
				.Include(u => u.House)
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null || user.House == null)
			{
				return NotFound("User or associated house not found.");
			}

			// Calculate rent payment status for the current month
			var currentDate = DateTime.Now;
			var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			var rentPaid = await _context.Payments
				.AnyAsync(p => p.UserId == userId
						&& p.PaymentType == "Rent"
						&& p.PaymentDate >= firstDayOfMonth
						&& p.PaymentDate <= lastDayOfMonth);

			// Retrieve unpaid utilities
			var unpaidUtilities = await _context.UtilityReadings
				.Include(u => u.Utility)
				.Where(u => u.TenantId == userId && !u.IsPaid)
				.ToListAsync();

			var pendingBills = new List<PendingBillsViewModel>();

			// Add rent to pending bills if not paid
			if (!rentPaid)
			{
				pendingBills.Add(new PendingBillsViewModel
				{
					BillType = "Rent",
					Amount = user.House.Rent,
					Description = $"Rent for {DateTime.Now:MMMM yyyy}",
					DueDate = DateTime.Now.AddDays(5),
					HouseId = user.HouseId
				});
			}

			// Add unpaid utilities to pending bills
			if (unpaidUtilities.Any())
			{
				var totalUtilities = unpaidUtilities.Sum(u => u.TotalCost);
				var utilityDescriptions = string.Join(", ", unpaidUtilities.Select(u => $"{u.Utility.Name}: ${u.TotalCost}"));

				pendingBills.Add(new PendingBillsViewModel
				{
					BillType = "Utility",
					Amount = totalUtilities,
					Description = utilityDescriptions,
					DueDate = DateTime.Now,
					HouseId = user.HouseId
				});
			}

			// Fetch active maintenance requests
			var activeRequests = await _context.Requests
				.Where(r => r.TenantId == userId && r.Status == RequestStatus.InProgress)
				.Select(r => new ActiveRequestViewModel
				{
					RequestType = r.Title,
					Status = r.Status.ToString(),
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();

			// Count documents
			var documentsCount = await _context.LeaseDocuments
				.CountAsync(d => d.Lease.TenantId == userId);

			// Calculate next payment date
			var lastPayment = await _context.Payments
				.Where(p => p.UserId == userId && p.PaymentStatus == "Completed")
				.OrderByDescending(p => p.PaymentDate)
				.FirstOrDefaultAsync();

			DateTime nextPaymentDueDate = lastPayment != null
				? lastPayment.PaymentDate.AddMonths(1)
				: new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);

			var daysUntilNextPayment = (nextPaymentDueDate - DateTime.Now).Days;

			// Create and populate the view model
			var viewModel = new TenantDashboardViewModel
			{
				HouseNumber = user.House.HouseNumber,
				StreetName = user.House.Property?.Address ?? "Street name not available",
				Rent = (int)user.House.Rent,
				PendingBills = pendingBills,
				ActiveRequests = activeRequests,
				NextPaymentDueDate = nextPaymentDueDate,
				DaysUntilNextPayment = daysUntilNextPayment,
				DocumentsCount = documentsCount
			};

			return View(viewModel);
		}
		catch (Exception ex)
		{
			// Log the exception details here
			ViewBag.Error = "An error occurred while processing your request. Please try again later.";
			return View("Error");
		}
	}
}
