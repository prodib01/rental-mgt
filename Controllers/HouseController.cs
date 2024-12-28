using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementSystem.Controllers
{
    [Route("Landlord/House")]
    // [Authorize(Roles = "Landlord")]
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
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            IQueryable<House> query = _context.Houses;

            if (userRole == "Landlord")
            {
                query = query.Where(h => h.Property.UserId == userId);
            }

            var totalHouses = await query.CountAsync();

            var houses = await query
                .Include(h => h.Property)
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

            var properties = await _context.Properties
                .Where(p => p.UserId == userId)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Type
                })
                .ToListAsync();

            var viewModel = new HouseViewModel
            {
                Houses = houses,
                Properties = properties,
                StatusMessage = totalHouses > 0 ? $"{totalHouses} houses found." : "No houses found."
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
                var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

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
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HouseNumber, PropertyId")] HouseDto houseDto)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

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

            if (house.PropertyId != houseDto.PropertyId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                house.PropertyId = houseDto.PropertyId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(House));
            }

            return RedirectToAction(nameof(House));
        }

        // GET: /Landlord/House/Delete/5
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

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
