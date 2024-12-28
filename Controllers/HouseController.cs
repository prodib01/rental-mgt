using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RentalManagementSystem.Controllers
{
    [Authorize]
    [Route("Landlord/House")]
    public class HousesController : Controller
    {
        private readonly RentalManagementContext _context;

        public HousesController(RentalManagementContext context)
        {
            _context = context;
        }

        // GET: /Landlord/House
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> House(int page = 1, int pageSize = 10)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized("Invalid User ID.");
        }

        // Always filter by the logged-in landlord's properties
        var query = _context.Houses
            .Where(h => h.Property.UserId == userId);

        var totalHouses = await query.CountAsync();

        var houses = await query
            .Include(h => h.Property)
            .OrderBy(h => h.Property.Address)
            .ThenBy(h => h.HouseNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new HouseListItemViewModel
            {
                Id = h.Id,
                HouseNumber = h.HouseNumber,
                Rent = h.Rent,
                PropertyId = h.PropertyId,
                PropertyType = h.Property.Type
            })
            .ToListAsync();

        // Only get properties owned by the current landlord
        var properties = await _context.Properties
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Address)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Type} - {p.Address}"
            })
            .ToListAsync();

        var viewModel = new HouseViewModel
        {
            Houses = houses,
            Properties = properties,
            StatusMessage = totalHouses > 0 ? $"Showing {houses.Count} of {totalHouses} houses." : "No houses found."
        };

        return View("~/Views/Landlord/House.cshtml", viewModel);
    }

        // POST: /Landlord/House/Add
        [HttpPost]
        [Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("HouseNumber, PropertyId, Rent")] CreateHouseDto houseDto)
        {
            if (ModelState.IsValid)
            {
                    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

                var property = await _context.Properties
                    .Where(p => p.UserId == userId)
                    .FirstOrDefaultAsync(p => p.Id == houseDto.PropertyId);

                if (property == null)
                {
                    return Forbid();
                }

                var house = new House
                {
                    PropertyId = houseDto.PropertyId,
                    Rent = houseDto.Rent // Add this line
                };

                _context.Add(house);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(House));
            }

            return RedirectToAction(nameof(House));
        }

        // POST: /Landlord/House/Edit/5
 // POST: /Landlord/House/Edit/5
[HttpPost]
[Route("Edit/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("HouseNumber, PropertyId, Rent")] HouseDto houseDto)
{
    if (!ModelState.IsValid)
    {
        return RedirectToAction(nameof(House));
    }

    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

    // Get the house with its property information
    var house = await _context.Houses
        .Include(h => h.Property)
        .FirstOrDefaultAsync(h => h.Id == id);

    if (house == null)
    {
        return NotFound();
    }

    // Check if the house belongs to a property owned by the current user
    if (house.Property.UserId != userId)
    {
        return Forbid();
    }

    // Verify the new property (if changed) belongs to the current user
    if (house.PropertyId != houseDto.PropertyId)
    {
        var newProperty = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == houseDto.PropertyId && p.UserId == userId);

        if (newProperty == null)
        {
            return Forbid();
        }
    }

    // Update the house properties
    house.HouseNumber = houseDto.HouseNumber;
    house.PropertyId = houseDto.PropertyId;
    house.Rent = houseDto.Rent;

    try
    {
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(House));
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!HouseExists(id))
        {
            return NotFound();
        }
        throw;
    }
}

        // GET: /Landlord/House/Delete/5
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

            var house = await _context.Houses.FindAsync(id);
            if (house == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(p => p.Id == house.PropertyId);

            if (property == null)
            {
                return Forbid();
            }

            _context.Houses.Remove(house);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(House));
        }

        private bool HouseExists(int id)
        {
            return _context.Houses.Any(e => e.Id == id);
        }
    }
}
