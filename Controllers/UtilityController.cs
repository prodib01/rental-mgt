using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using RentalManagementSystem.Services;

namespace RentalManagementSystem.Controllers
{
	[Authorize]
	[Route("Landlord/Utility")]
	public class UtilityController : Controller
	{
		private readonly IUtilityService _utilityService;
		private readonly RentalManagementContext _context;

		public UtilityController(IUtilityService utilityService, RentalManagementContext context)
		{
			_utilityService = utilityService;
			_context = context;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Index()
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized("Invalid User ID.");
			}

			var utilities = await _utilityService.GetAllUtilitiesAsync();
			ViewBag.Tenants = await GetTenantsSelectList(userId);

			return View("~/Views/Landlord/Utility.cshtml", utilities);
		}

		[HttpGet]
		[Route("Readings/{utilityId}")]
		public async Task<IActionResult> GetReadings(int utilityId)
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(userIdStr, out var userId))
			{
				return Unauthorized("Invalid User ID.");
			}

			var readings = await _utilityService.GetReadingsAsync(utilityId);
			return Json(readings);
		}

		[HttpPost]
		[Route("")]
		public async Task<IActionResult> Add(CreateUtilityDto dto)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				await _utilityService.CreateUtilityAsync(dto);
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// Log the error
				return StatusCode(500, "Error creating utility");
			}
		}

		[HttpPost]
		[Route("Edit/{id}")]
		public async Task<IActionResult> Edit(int id, CreateUtilityDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var utility = await _utilityService.GetUtilityByIdAsync(id);
			if (utility == null)
				return NotFound();

			utility.Name = dto.Name;
			utility.Cost = dto.Cost;

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[Route("Delete/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var utility = await _utilityService.GetUtilityByIdAsync(id);
			if (utility == null)
				return NotFound();

			_context.Utilities.Remove(utility);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

[HttpPost]
[Route("Reading/Add/{utilityId}")]
public async Task<IActionResult> AddReading(int utilityId, [FromBody] CreateUtilityReadingDto dto)
{
	try
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		await _utilityService.AddReadingAsync(utilityId, dto);
		return Ok();
	}
	catch (InvalidOperationException ex)
	{
		return BadRequest(ex.Message);
	}
	catch (Exception ex)
	{
		return StatusCode(500, "Error adding reading");
	}
}

[HttpPost]
[Route("Reading/Edit/{id}")]
public async Task<IActionResult> EditReading(int id, [FromBody] CreateUtilityReadingDto dto)
{
	try
	{
		if (!ModelState.IsValid)
		{
			var errors = string.Join("; ", ModelState.Values
				.SelectMany(v => v.Errors)
				.Select(e => e.ErrorMessage));
			return BadRequest($"Validation failed: {errors}");
		}

		var reading = await _utilityService.GetReadingByIdAsync(id);
		if (reading == null)
			return NotFound($"Reading with ID {id} not found");

		await _utilityService.UpdateReadingAsync(id, dto);
		return Ok();
	}
	catch (Exception ex)
	{
		// Log the exception here
		return StatusCode(500, $"Internal server error: {ex.Message}");
	}
}

[HttpPost]
[Route("Reading/Delete/{id}")]
public async Task<IActionResult> DeleteReading(int id)
{
	try
	{
		var reading = await _utilityService.GetReadingByIdAsync(id);
		if (reading == null)
			return NotFound($"Reading with ID {id} not found");

		_context.UtilityReadings.Remove(reading);
		await _context.SaveChangesAsync();
		return Ok();
	}
	catch (Exception ex)
	{
		// Log the exception here
		return StatusCode(500, $"Internal server error: {ex.Message}");
	}
}
		private async Task<List<SelectListItem>> GetTenantsSelectList(int userId)
		{
			return await _context.Users
				.Where(u => u.House.Property.UserId == userId)
				.Select(u => new SelectListItem
				{
					Value = u.Id.ToString(),
					Text = $"{u.FullName} ({u.House.HouseNumber})"
				})
				.ToListAsync();
		}
		
		[HttpGet]
[Route("Reading/Check/{utilityId}/{tenantId}")]
public async Task<IActionResult> CheckExistingReading(int utilityId, int tenantId)
{
    var existingReading = await _context.UtilityReadings
        .AnyAsync(r => r.UtilityId == utilityId && r.TenantId == tenantId);
    
    return Json(existingReading);
}
	}
}