using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementSystem.Services
{
	public interface ILandlordDashboardService
	{
		Task<LandlordDashboardViewModel> GetDashboardDataAsync(string userId);
	}

	public class LandlordDashboardService : ILandlordDashboardService
	{
		private readonly RentalManagementContext _context;

		public LandlordDashboardService(RentalManagementContext context)
		{
			_context = context;
		}

public async Task<LandlordDashboardViewModel> GetDashboardDataAsync(string userId)
{
	if (string.IsNullOrEmpty(userId))
	{
		throw new ArgumentNullException(nameof(userId));
	}

	// Get all properties for the landlord
	var properties = await _context.Properties
		.Include(p => p.Houses)
		.Where(p => p.UserId == int.Parse(userId))
		.ToListAsync();

	int totalProperties = properties.Count;

	// Get all houses for the landlord's properties
	var houses = await _context.Houses
		.Include(h => h.Property)
		.Include(h => h.Tenant)
		.Where(h => h.Property.UserId == int.Parse(userId))
		.ToListAsync();

	int totalHouses = houses.Count;
	int occupiedHouses = houses.Count(h => h.IsOccupied);
	int vacantHouses = totalHouses - occupiedHouses;

	// Get vacant houses
	var vacantHousesList = houses
		.Where(h => !h.IsOccupied)
		.Select(h => new VacantHouseViewModel
		{
			HouseNumber = h.HouseNumber,
			Address = h.Property.Address,
			MonthlyRent = h.Rent,
			VacantSince = h.VacantSince
		})
		.ToList();

	// Calculate monthly revenue from rents of occupied houses
	var monthlyRevenue = houses
		.Where(h => h.IsOccupied)
		.Sum(h => h.Rent);
		
	var upcomingLeaseRenewals = await _context.Leases
	.Include(l => l.Tenant)
	.Where(l => l.EndDate >= DateTime.UtcNow && l.EndDate <= DateTime.UtcNow.AddDays(30))
	.CountAsync();	
	
var pendingMaintenanceRequests = await _context.Requests
    .Include(r => r.Tenant.House)
    .Where(r => r.Tenant.House.Property.UserId == int.Parse(userId) && r.Status == RequestStatus.Pending)
    .CountAsync();

	
	var totalTenants = await _context.Users
	.Where(u => u.Role == "Tenant" && u.House != null && u.House.Property.UserId == int.Parse(userId))
	.CountAsync();
		
	var recentPayments = await _context.Payments
		.Include(p => p.House)
		.ThenInclude(h => h.Property)
		.Where(p => p.House.Property.UserId == int.Parse(userId) && p.PaymentStatus == "Completed")
		.OrderByDescending(p => p.PaymentDate)
		.Take(5) // Get the 5 most recent payments
		.Select(p => new RecentPaymentViewModel
		{
			TenantName = p.User.FullName,
			PropertyAddress = p.House.Property.Address,
			Amount = p.Amount,
			PaymentDate = p.PaymentDate
		})
		.ToListAsync();
		

	return new LandlordDashboardViewModel
	{
		TotalProperties = totalHouses,
		MonthlyRevenue = monthlyRevenue,
		OccupiedProperties = occupiedHouses,
		VacantProperties = vacantHouses,
		VacantHouses = vacantHousesList, 
		TotalTenants = totalTenants,
		PendingMaintenanceRequests = pendingMaintenanceRequests,
		UpcomingLeaseRenewals = upcomingLeaseRenewals,
		RecentPayments = new List<RecentPaymentViewModel>() 
	};
}

public async Task MarkHouseAsVacant(int houseId)
{
	var house = await _context.Houses.FindAsync(houseId);
	if (house != null)
	{
		house.IsOccupied = false;
		house.VacantSince = DateTime.UtcNow;
		await _context.SaveChangesAsync();
	}
}


	}
}