using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementSystem.Services
{
	public interface ITenantDashboardService
	{
		Task<TenantDashboardViewModel> GetDashboardDataAsync(string userId);
	}

	public class TenantDashboardService : ITenantDashboardService
	{
		private readonly RentalManagementContext _context;

		public TenantDashboardService(RentalManagementContext context)
		{
			_context = context;
		}

		public async Task<TenantDashboardViewModel> GetDashboardDataAsync(string userId)
		{
			// Attempt to parse userId as an integer
			if (!int.TryParse(userId, out int parsedUserId))
			{
				throw new Exception("Invalid user ID format");
			}

			// Fetch tenant and related house details
			var tenant = await _context.Users
				.Include(u => u.House)
				.ThenInclude(h => h.Property) // Assuming Property is a navigation property in House
				.FirstOrDefaultAsync(u => u.Id == parsedUserId);

			if (tenant == null)
			{
				throw new Exception("Tenant not found");
			}

			var house = tenant.House;
			if (house == null)
			{
				throw new Exception("House details not found for this tenant");
			}

			// Fetch pending bills for the tenant
			var pendingBills = await _context.Payments
				.Where(p => p.UserId == parsedUserId && p.PaymentStatus == "Pending")
				.Select(p => new PendingBillsViewModel
				{
					BillType = p.PaymentMethod,  // Assuming PaymentMethod can represent the bill type
					Amount = p.Amount,
					DueDate = p.PaymentDate // Assuming PaymentDate represents the due date
				})
				.ToListAsync();

			var activerequests = await _context.Requests
				.Where(r => r.TenantId == parsedUserId && r.Status == RequestStatus.InProgress)
							.Select(r => new ActiveRequestViewModel
							{
								RequestType = r.Title,
								Status = r.Status.ToString(),
								CreatedAt = r.CreatedAt
							})
							.ToListAsync();

			// Build the dashboard view model
			return new TenantDashboardViewModel
			{
				HouseNumber = house.HouseNumber,
				StreetName = house.Property?.Address ?? "Street name not available", // Populate StreetName from Property
				Rent = (int)house.Rent,
				PendingBills = pendingBills,
				ActiveRequests = activerequests
			};
		}
	}
}
