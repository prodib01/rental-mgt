using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementSystem.Controllers
{
    [Authorize]
    [Route("Landlord/Property")]
    public class PropertiesController : Controller
    {
        private readonly RentalManagementContext _context;
        private readonly ILogger<PropertiesController> _logger;

private int GetCurrentUserId()
{
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        throw new UnauthorizedAccessException("Invalid User ID.");
    }
    return userId;
}


        public PropertiesController(RentalManagementContext context, ILogger<PropertiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

[HttpGet]
[Route("")]
public async Task<IActionResult> Properties(int page = 1, int pageSize = 10)
{
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

    var query = _context.Properties
        .Where(p => p.UserId == userId);

    var totalProperties = await query.CountAsync();

    var properties = await query
        .Include(p => p.Houses)
        .Include(p => p.User)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new PropertyViewModel
        {
            Id = p.Id,
            Address = p.Address,
            Type = p.Type,
            Description = p.Description,
            NumberOfHouses = p.Houses.Count
        })
        .ToListAsync();

    var viewModel = new PropertyListViewModel
    {
        Properties = properties,
        NewProperty = new PropertyViewModel()
    };

    return View("~/Views/Landlord/Property.cshtml", viewModel);
}



[HttpPost]
[Route("AddProperty")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddProperty(PropertyViewModel model)
{
    _logger.LogInformation("AddProperty method called");
    _logger.LogInformation($"Model data - Address: {model.Address}, Type: {model.Type}, Description: {model.Description}");

    if (!ModelState.IsValid)
    {
        foreach (var modelStateEntry in ModelState.Values)
        {
            foreach (var error in modelStateEntry.Errors)
            {
                _logger.LogError($"Model validation error: {error.ErrorMessage}");
            }
        }
        TempData["ErrorMessage"] = "Please correct the form errors.";
        return RedirectToAction("Properties");
    }

    try
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation($"Current user ID: {userId}");

        var propertyExists = await _context.Properties
            .AnyAsync(p => p.Address == model.Address && p.UserId == userId);
            
        if (propertyExists)
        {
            _logger.LogWarning($"Property with address {model.Address} already exists for user {userId}");
            TempData["ErrorMessage"] = "A property with this address already exists.";
            return RedirectToAction("Properties");
        }

        var property = new Property
        {
            Address = model.Address,
            Type = model.Type,
            Description = model.Description,
            UserId = userId,
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Property added successfully. ID: {property.Id}");

        TempData["SuccessMessage"] = "Property added successfully.";
        return RedirectToAction("Properties");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding property");
        TempData["ErrorMessage"] = $"An error occurred while adding the property: {ex.Message}";
        return RedirectToAction("Properties");
    }
}


        // POST: Edit Property
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(int id, PropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form errors.";
                return RedirectToAction("Properties");
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update property details
                    property.Address = model.Address;
                    property.Type = model.Type;
                    property.Description = model.Description;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Property updated successfully.";
                    return RedirectToAction("Properties");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error editing property");
                    TempData["ErrorMessage"] = "An error occurred while updating the property.";
                    return RedirectToAction("Properties");
                }
            }
        }

[HttpPost]
[Route("Delete/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteProperty(int id)
{
    try
    {
        var userId = GetCurrentUserId();
        var property = await _context.Properties
            .Include(p => p.Houses)  // Include related houses
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (property == null)
        {
            _logger.LogWarning($"Property with ID {id} not found or does not belong to user {userId}");
            return Json(new { success = false, message = "Property not found." });
        }

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Remove the property (this will also remove related houses due to cascade delete)
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Property {id} and its relationships successfully deleted");
                return Json(new { success = true, message = "Property deleted successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error during property deletion transaction for property {id}");
                return Json(new { success = false, message = "Database error occurred while deleting property." });
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Unexpected error deleting property {id}");
        return Json(new { success = false, message = "An unexpected error occurred." });
    }
}

    }
}
