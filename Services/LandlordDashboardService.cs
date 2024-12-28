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
                .Where(p => p.UserId == int.Parse(userId) && !p.IsDeleted)
                .ToListAsync();

            int totalProperties = properties.Count;
            
            // Count total houses
            // var totalHouses = properties.Sum(p => p.Houses.Count);

            // Calculate monthly revenue from payments
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            var monthlyRevenue = await _context.Payments
                .Where(p => p.House.Property.UserId == int.Parse(userId)
                    && p.PaymentDate.Month == currentMonth
                    && p.PaymentDate.Year == currentYear)
                .SumAsync(p => p.Amount);

            // Get recent payments
            var recentPayments = await _context.Payments
                .Include(p => p.House)
                    .ThenInclude(h => h.Property)
                .Where(p => p.House.Property.UserId == int.Parse(userId))
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentPaymentViewModel
                {
                    TenantName = "Tenant", // Since we don't have direct tenant info
                    PropertyAddress = p.House.Property.Address + " - Unit " + p.House.HouseNumber,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            return new LandlordDashboardViewModel
            {
                TotalProperties = totalProperties,
                // TotalHouses = totalHouses,
                MonthlyRevenue = monthlyRevenue,
                RecentPayments = recentPayments,
                // Set other properties to 0 since we don't have the data yet
                OccupiedProperties = 0,
                VacantProperties = totalProperties,
                TotalTenants = 0,
                PendingMaintenanceRequests = 0,
                UpcomingLeaseRenewals = 0
            };
        }
    }
}