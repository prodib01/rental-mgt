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
		Task<TenantDashboardViewModel> GetDashboardDataAsync(int userId);
	}

	public class TenantDashboardService : ITenantDashboardService
	{
		private readonly RentalManagementContext _context;

		public TenantDashboardService(RentalManagementContext context)
		{
			_context = context;
		}

		public async Task<TenantDashboardViewModel> GetDashboardDataAsync(int userId) // Changed from string to int
		{
			// Remove the TryParse since we're already getting an int
			var tenant = await _context.Users
				.Include(u => u.House)
				.FirstOrDefaultAsync(u => u.Id == userId);

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
			var nextPaymentInfo = await CalculateNextPaymentAsync(userId); // This already expects an int

			// Fetch pending bills
			var pendingBills = await _context.Payments
				.Where(p => p.UserId == userId && p.PaymentStatus == "Pending")
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
			// Method already accepts int, no changes needed here
			var lastPayment = await _context.Payments
				.Where(p => p.UserId == userId && p.PaymentStatus == "Completed")
				.OrderByDescending(p => p.PaymentDate)
				.FirstOrDefaultAsync();

			DateTime nextDueDate;
			if (lastPayment != null)
			{
				nextDueDate = lastPayment.PaymentDate.AddMonths(1);
			}
			else
			{
				nextDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
			}

			int daysRemaining = (nextDueDate - DateTime.Now).Days;

			return (nextDueDate, daysRemaining);
		}
	}
}
