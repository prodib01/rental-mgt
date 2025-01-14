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

			// Calculate next payment details
			var nextPaymentInfo = await CalculateNextPaymentAsync(parsedUserId);

			// Fetch pending bills
			var pendingBills = await _context.Payments
				.Where(p => p.UserId == parsedUserId && p.PaymentStatus == "Pending")
				.Select(p => new PendingBillsViewModel
				{
					BillType = p.PaymentType,
					PaymentMethod = p.PaymentMethod,
					Amount = p.Amount,
					DueDate = p.PaymentDate
					
				})
				.ToListAsync();

			// Fetch active requests
			var activeRequests = await _context.Requests
				.Where(r => r.TenantId == parsedUserId && r.Status == RequestStatus.InProgress)
				.Select(r => new ActiveRequestViewModel
				{
					RequestType = r.Title,
					Status = r.Status.ToString(),
					CreatedAt = r.CreatedAt
				})
				.ToListAsync();

			// Count documents
			var documentsCount = await _context.LeaseDocuments
				.CountAsync(d => d.Lease.TenantId == parsedUserId);

			// Build the dashboard view model
			return new TenantDashboardViewModel
			{
				HouseNumber = house.HouseNumber,
				StreetName = house.Property?.Address ?? "Street name not available",
				Rent = (int)house.Rent,
				PendingBills = pendingBills,
				ActiveRequests = activeRequests,
				NextPaymentDueDate = nextPaymentInfo.DueDate,
				DaysUntilNextPayment = nextPaymentInfo.DaysRemaining,
				DocumentsCount = documentsCount
			};
		}

		private async Task<(DateTime DueDate, int DaysRemaining)> CalculateNextPaymentAsync(int userId)
		{
			// Get the last payment date
			var lastPayment = await _context.Payments
				.Where(p => p.UserId == userId && p.PaymentStatus == "Completed")
				.OrderByDescending(p => p.PaymentDate)
				.FirstOrDefaultAsync();

			// Calculate next due date
			DateTime nextDueDate;
			if (lastPayment != null)
			{
				// Next payment is one month after the last payment
				nextDueDate = lastPayment.PaymentDate.AddMonths(1);
			}
			else
			{
				// If no previous payments, set due date to the 1st of next month
				nextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
			}

			// Calculate days remaining
			int daysRemaining = (nextDueDate - DateTime.Now).Days;

			return (nextDueDate, daysRemaining);
		}
	}
}
