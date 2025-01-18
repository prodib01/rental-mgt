using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentalManagementSystem.Controllers
{
    [Authorize]
    [Route("Tenant/Utilities")]
    public class TUtilityController : Controller
    {
        private readonly IUtilityService _utilityService;
        private readonly RentalManagementContext _context;

        // Constructor
        public TUtilityController(IUtilityService utilityService, RentalManagementContext context)
        {
            _utilityService = utilityService;
            _context = context;
        }

        // GET: Tenant/Utilities/Bills
        [HttpGet("Bills")]
        public async Task<IActionResult> TenantBills()
        {
            // Get the current user's ID
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Invalid User ID.");
            }

            // Fetch utility readings for the tenant
            var readings = await _context.UtilityReadings
                .Include(r => r.Utility)
                .Where(r => r.TenantId == userId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => new UtilityReadingViewModel
                {
                    ReadingDate = r.ReadingDate,
                    UtilityName = r.Utility.Name,
                    PrevReading = r.PrevReading,
                    CurrentReading = r.CurrentReading,
                    Consumption = r.Consumption,
                    TotalCost = r.TotalCost
                })
                .ToListAsync();

            // Return the view with readings
            return View("~/Views/Tenant/Utilities.cshtml", readings);
        }
    }
}
