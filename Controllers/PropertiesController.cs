using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementSystem.Controllers
{
    [Route("Landlord/Property")]
    // [Authorize(Roles = "Landlord")]
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
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            IQueryable<Property> query = _context.Properties;

            if (userRole == "Landlord")
            {
                query = query.Where(p => p.UserId == userId);
            }

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
                    LandlordName = p.User.FullName,
                    NumberOfHouses = p.Houses.Count
                })
                .ToListAsync();

            var viewModel = new PropertyViewModel
            {
                Properties = properties,
                StatusMessage = totalProperties > 0 ? $"{totalProperties} properties found." : "No properties found."
            };

            // Explicitly reference the correct view path
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
                var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

                var property = new Property
                {
                    Address = propertyDto.Address,
                    Type = propertyDto.Type,
                    Description = propertyDto.Description,
                    UserId = userId
                };

                _context.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Landlord/Property/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Address,Type,Description")] PropertyDto propertyDto)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

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
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Landlord/Property/Delete/5
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

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

            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}