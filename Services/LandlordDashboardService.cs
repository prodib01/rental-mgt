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
			Address = h.Property.Address,
			MonthlyRent = h.Rent,
		})
		.ToList();

	// Calculate monthly revenue from rents of occupied houses
	var monthlyRevenue = houses
		.Where(h => h.IsOccupied)
		.Sum(h => h.Rent);
		
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
		TotalProperties = totalProperties,
		MonthlyRevenue = monthlyRevenue,
		OccupiedProperties = occupiedHouses,
		VacantProperties = vacantHouses,
		VacantHouses = vacantHousesList, // Populate the vacant houses list
		TotalTenants = 0, // Replace with actual tenant count
		PendingMaintenanceRequests = 0, // Replace with maintenance request data
		UpcomingLeaseRenewals = 0, // Replace with lease renewal data
		RecentPayments = new List<RecentPaymentViewModel>() // Replace with recent payment data
	};
}

	}
}