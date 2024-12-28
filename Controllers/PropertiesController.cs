using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RentalManagementSystem.Controllers
{
    [Authorize]
    [Route("Landlord/Property")]
    public class PropertiesController : Controller
    {
        private readonly RentalManagementContext _context;

        public PropertiesController(RentalManagementContext context)
        {
            _context = context;
        }

// GET: /Landlord/Property
[HttpGet]
[Route("")]
public async Task<IActionResult> Property(int page = 1, int pageSize = 10)
{
    // Get the logged-in user's ID and ensure it is a valid integer
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

    // Get the user's role (could be used for role-based filtering, if necessary)
    var userRole = User.FindFirst("Role")?.Value;

    // Ensure that only properties belonging to the logged-in user are fetched
    IQueryable<Property> query = _context.Properties.Where(p => p.UserId == userId);

    // Apply filtering if the user role is "Landlord" (optional role-based access)
    if (userRole == "Landlord")
    {
        query = query.Where(p => p.UserId == userId); // This line ensures only the logged-in user's properties are fetched
    }

    // Get the total number of properties for pagination
    var totalProperties = await query.CountAsync();

    // Get the properties for the current page
    var properties = await query
        .Include(p => p.Houses)    // Include related houses (if any)
        .Include(p => p.User)      // Include user details (landlord info)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new PropertyViewModel
        {
            Id = p.Id,
            Address = p.Address,
            Type = p.Type,
            Description = p.Description,
            LandlordName = p.User.FullName,   // Assuming User has a FullName property
            NumberOfHouses = p.Houses.Count  // Assuming Houses is a collection
        })
        .ToListAsync();

    // Prepare the view model to pass to the view
    var viewModel = new PropertyViewModel
    {
        Properties = properties,
        StatusMessage = totalProperties > 0 ? $"{totalProperties} properties found." : "No properties found."
    };

    // Return the properties view with the prepared view model
    return View("~/Views/Landlord/Property.cshtml", viewModel);
}


        // POST: /Landlord/Property/Add
        [HttpPost]
        [Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Address,Type,Description")] CreatePropertyDto propertyDto)
        {
            if (ModelState.IsValid)
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized("Invalid User ID.");
                }

                var property = new Property
                {
                    Address = propertyDto.Address,
                    Type = propertyDto.Type,
                    Description = propertyDto.Description,
                    UserId = userId
                };

                _context.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Property));
            }

            return RedirectToAction(nameof(Property));
        }

        // POST: /Landlord/Property/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Address,Type,Description")] PropertyDto propertyDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Invalid User ID.");
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (property.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                property.Address = propertyDto.Address;
                property.Type = propertyDto.Type;
                property.Description = propertyDto.Description;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Property));
            }

            return RedirectToAction(nameof(Property));
        }

        // GET: /Landlord/Property/Delete/5
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Invalid User ID.");
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (property.UserId != userId)
            {
                return Forbid();
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Property));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}
