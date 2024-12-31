using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace RentalManagementSystem.Controllers
{
	[Authorize]
	[Route("Landlord/Leases")]
	public class LeaseController : Controller
	{
		private readonly RentalManagementContext _context;

		public LeaseController(RentalManagementContext context)
		{
			_context = context;
		}

		// GET: /Landlord/Lease
		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Lease()
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized("Invalid User ID.");
			}

			var query = _context.Leases
			.Where(l => l.Tenant.House.Property.UserId == userId);

			var totalLeases = await query.CountAsync();

			var leases = await query
				.Include(l => l.Tenant)
				.OrderBy(l => l.Tenant.FullName)
				.Select(l => new LeaseListItemViewModel

				{
					Id = l.Id,
					TenantName = l.Tenant.FullName,
					HouseNumber = l.Tenant.House.HouseNumber,
					PropertyAddress = l.Tenant.House.Property.Address,
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					MonthlyRent = l.Tenant.House.Rent,
				})
				.ToListAsync();

			var tenants = await _context.Users
				.Where(u => u.House.Property.UserId == userId &&
					(!_context.Leases.Any(l => l.TenantId == u.Id) || // Not in any lease
					 _context.Leases.Any(l => l.TenantId == u.Id && _context.Leases.Any())))  // Or in current lease
				.Select(u => new SelectListItem
				{
					Value = u.Id.ToString(),
					Text = $"{u.FullName} ({u.Email})"
				})
				.ToListAsync();

			var viewModel = new LeaseViewModel
			{
				Leases = leases,
				Tenants = tenants
			};
			return View("~/Views/Landlord/Leases.cshtml", viewModel);
		}

		//POST: /Landlord/Lease/Add
		[HttpPost]
		[Route("Add")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add([Bind("TenantId, StartDate, EndDate")] CreateLeaseDto leaseDto)

		{
			if (ModelState.IsValid)
			{
				var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!int.TryParse(userIdStr, out var userId))
				{
					return Unauthorized("Invalid User ID.");
				}
				var tenant = await _context.Users
				.Where(u => u.House.Property.UserId == userId)
				.FirstOrDefaultAsync(u => u.Id == leaseDto.TenantId);

				if (tenant == null)

				{
					return Forbid();
				}

				var lease = new Lease
				{
					TenantId = leaseDto.TenantId,
					StartDate = leaseDto.StartDate,
					EndDate = leaseDto.EndDate
				};

				_context.Add(lease);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Lease));
			}

			return RedirectToAction(nameof(Lease));
		}

		//POST: /Landlord/Lease/Edit/5
		[HttpPost]
		[Route("Edit/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("TenantId,StartDate,EndDate")] CreateLeaseDto leaseDto)
		{
			// Add debug check
			Console.WriteLine($"Received Edit request for lease {id}");
			Console.WriteLine($"DTO values - TenantId: {leaseDto.TenantId}, StartDate: {leaseDto.StartDate}, EndDate: {leaseDto.EndDate}");

			if (!ModelState.IsValid)
			{
				var errors = string.Join("; ", ModelState.Values
					.SelectMany(x => x.Errors)
					.Select(x => x.ErrorMessage));
				Console.WriteLine($"Model validation failed: {errors}");
				return RedirectToAction(nameof(Lease));
			}

			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized("Invalid User ID.");
			}

			var lease = await _context.Leases
				.Include(l => l.Tenant)
				.ThenInclude(t => t.House)
				.ThenInclude(h => h.Property)
				.FirstOrDefaultAsync(l => l.Id == id);

			if (lease == null)
			{
				Console.WriteLine($"Lease {id} not found");
				return NotFound();
			}

			// Log current values before update
			Console.WriteLine($"Current lease values - TenantId: {lease.TenantId}, StartDate: {lease.StartDate}, EndDate: {lease.EndDate}");

			// Update lease properties
			lease.TenantId = leaseDto.TenantId;
			lease.StartDate = leaseDto.StartDate;
			lease.EndDate = leaseDto.EndDate;
			lease.UpdatedAt = DateTime.UtcNow;

			// Log new values after update
			Console.WriteLine($"Updated lease values - TenantId: {lease.TenantId}, StartDate: {lease.StartDate}, EndDate: {lease.EndDate}");

			try
			{
				_context.Entry(lease).State = EntityState.Modified;
				var changes = await _context.SaveChangesAsync();
				Console.WriteLine($"SaveChanges completed. Number of entities modified: {changes}");
				return RedirectToAction(nameof(Lease));
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Console.WriteLine($"Concurrency error: {ex.Message}");
				if (!LeaseExists(id))
				{
					return NotFound();
				}
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating lease: {ex.Message}");
				throw;
			}
		}
		// GET: /Landlord/Lease/Delete/5
		[Route("Delete/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized("Invalid User ID.");
			}
			var lease = await _context.Leases.FindAsync(id);
			if (lease == null)
			{
				return NotFound();
			}

			var tenant = await _context.Users
			.Where(u => u.House.Property.UserId == userId)
			.FirstOrDefaultAsync(u => u.Id == lease.TenantId);

			if (tenant == null)
			{
				return Forbid();
			}

			_context.Leases.Remove(lease);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Lease));
		}

		private bool LeaseExists(int id)
		{
			return _context.Leases.Any(e => e.Id == id);
		}
	}
}
