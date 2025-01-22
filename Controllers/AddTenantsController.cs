using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace RentalManagementSystem.Controllers
{
	[Authorize]
	[Route("Landlord/Tenants")]
	public class AddTenantsController : Controller
	{
		private readonly RentalManagementContext _context;
		private readonly ILogger<AddTenantsController> _logger;

		public AddTenantsController(RentalManagementContext context, ILogger<AddTenantsController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: Tenant Management
[HttpGet]
[Route("")]
public async Task<IActionResult> Tenants()
{
	var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	if (!int.TryParse(userIdStr, out var userId))
	{
		return Unauthorized("Invalid User ID");
	}

	var tenants = await _context.Users
		.Where(u => u.Role == "Tenant" && 
					u.House != null && 
					u.House.Property != null && 
					u.House.Property.UserId == userId)
		.Select(u => new TenantViewModel
		{
			Id = u.Id,
			FullName = u.FullName,
			Email = u.Email,
			PhoneNumber = u.PhoneNumber,
			HouseNumber = u.House.HouseNumber,
			LastLoginDate = u.LastLoginDate
		})
		.ToListAsync();

	// Get available houses AND currently occupied houses
	var houses = await _context.Houses
		.Where(h => h.Property.UserId == userId && 
					((!h.IsOccupied && h.Tenant == null)))
		.Select(h => new SelectListItem
		{
			Value = h.HouseNumber,
			Text = h.HouseNumber
		})
		.ToListAsync();

	var model = new TenantListViewModel
	{
		Tenants = tenants,
		NewTenant = new AddTenantViewModel(),
		AvailableHouses = houses
	};

	return View("~/Views/Landlord/Tenants.cshtml", model);
}

		// POST: Add Tenant
		[HttpPost]
		[Route("Add")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTenant(AddTenantViewModel model)
		{
			// In your AddTenant action
			if (!ModelState.IsValid)
			{
				foreach (var modelStateEntry in ModelState.Values)
				{
					foreach (var error in modelStateEntry.Errors)
					{
						// Log these errors
						_logger.LogError($"Model validation error: {error.ErrorMessage}");
					}
				}
				TempData["ErrorMessage"] = "Please correct the form errors.";
				return RedirectToAction("Tenants");
			}

			// Check if email is already in use
			var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
			if (emailExists)
			{
				TempData["ErrorMessage"] = "An account with this email already exists.";
				return RedirectToAction("Tenants");
			}

			// Find the house by house number
			var house = await _context.Houses
				.FirstOrDefaultAsync(h => h.HouseNumber == model.HouseNumber && h.Tenant == null && !h.IsOccupied);

			if (house == null)
			{
				TempData["ErrorMessage"] = "This house is invalid or already occupied.";
				return RedirectToAction("Tenants");
			}

			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					_logger.LogInformation($"Adding new tenant: {model.Email}");
					var tenant = new User
					{
						FullName = model.FullName,
						Email = model.Email,
						PhoneNumber = model.PhoneNumber,
						Role = "Tenant",
						PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
						House = house,
						PasswordChanged = false // Initial password
					};

					_context.Users.Add(tenant);

					house.IsOccupied = true;
					house.VacantSince = null;
					_context.Houses.Update(house);
					
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();

					_logger.LogInformation($"Successfully added tenant: {model.Email}");
					TempData["SuccessMessage"] = "Tenant added successfully.";
					return RedirectToAction("Tenants");
				}
				catch (Exception ex)
				{
					// Rollback the transaction
					await transaction.RollbackAsync();

					await transaction.RollbackAsync();
					_logger.LogError(ex, "Error adding tenant");
					TempData["ErrorMessage"] = $"An error occurred while adding the tenant: {ex.Message}";
					return RedirectToAction("Tenants");
				}
			}
		}

	// POST: Edit Tenant
[HttpPost]
[Route("EditTenant")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditTenant(int id, AddTenantViewModel model)
{
    if (!ModelState.IsValid)
    {
        var errors = string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        _logger.LogError($"Model validation errors for tenant {id}: {errors}");
        TempData["ErrorMessage"] = $"Validation errors: {errors}";
        return RedirectToAction("Tenants");
    }

		var tenant = await _context.Users
			.Include(u => u.House)
			.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Tenant");

		if (tenant == null)
		{
			return NotFound();
		}

		// Check if email is already in use by another tenant
		var emailExists = await _context.Users
			.AnyAsync(u => u.Email == model.Email && u.Id != id);

		if (emailExists)
		{
			TempData["ErrorMessage"] = "An account with this email already exists.";
			return RedirectToAction("Tenants");
		}

		using (var transaction = await _context.Database.BeginTransactionAsync())
		{
			try
			{
				// Find the new house
				var newHouse = await _context.Houses
					.FirstOrDefaultAsync(h => h.HouseNumber == model.HouseNumber &&
											(h.Tenant == null || h.Tenant.Id == tenant.Id));

				if (newHouse == null)
				{
					TempData["ErrorMessage"] = "Selected house is invalid or already occupied.";
					return RedirectToAction("Tenants");
				}

				// Update the old house if exists
				if (tenant.House != null && tenant.House.Id != newHouse.Id)
				{
					tenant.House.IsOccupied = false;
					tenant.House.VacantSince = DateTime.UtcNow;
				}

				// Update tenant details
				tenant.FullName = model.FullName;
				tenant.Email = model.Email;
				tenant.PhoneNumber = model.PhoneNumber;
				tenant.House = newHouse;
				newHouse.IsOccupied = true;
				newHouse.VacantSince = null;

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				TempData["SuccessMessage"] = "Tenant updated successfully.";
				return RedirectToAction("Tenants");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error editing tenant");
				TempData["ErrorMessage"] = "An error occurred while updating the tenant.";
				return RedirectToAction("Tenants");
			}
		}
	}
	
		// POST: Delete Tenant
		[Route("Delete/{id}")]
		public async Task<IActionResult> DeleteTenant(int id)
		{
			var tenant = await _context.Users
				.Include(u => u.House)
				.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Tenant");

			if (tenant == null)
			{
				return NotFound();
			}

			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					// Remove the tenant's association with the house
					if (tenant.House != null)
					{
						tenant.House = null;
					}

					_context.Users.Remove(tenant);
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();

					TempData["SuccessMessage"] = "Tenant deleted successfully.";
					return RedirectToAction(nameof(Tenants));
				}
				catch (Exception ex)
				{
					// Rollback the transaction
					await transaction.RollbackAsync();

					// Log the exception
					// _logger.LogError(ex, "Error deleting tenant");
					TempData["ErrorMessage"] = "An error occurred while deleting the tenant.";
					return RedirectToAction("Tenants");
				}
			}
		}
	}
}