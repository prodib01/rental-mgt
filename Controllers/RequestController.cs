using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Services;
using System.Security.Claims;
using AutoMapper;

namespace RentalManagementSystem.Controllers
{
[Authorize]
[Route("Landlord/Request")]
public class RequestController : Controller
{
		private readonly IRequestService _requestService;
		private readonly RentalManagementContext _context;
		private readonly IMapper _mapper;

		public RequestController(
			IRequestService requestService, 
			RentalManagementContext context,
			IMapper mapper)
		{
			_requestService = requestService;
			_context = context;
			_mapper = mapper;
		}

[HttpGet]
[Route("")]
public async Task<IActionResult> Index(RequestStatus? status, string searchTerm)
{
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!int.TryParse(userIdStr, out var userId))
    {
        return Unauthorized("Invalid User ID.");
    }

    var requests = await _requestService.GetAllRequestsAsync(userId);
    var requestDtos = requests.ToList();

    // Filter by status and search term if provided
    var filteredRequests = requestDtos
        .Where(r => !status.HasValue || r.Status == status)
        .Where(r => string.IsNullOrEmpty(searchTerm) || 
                   r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                   r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var viewModels = filteredRequests.Select(r => new RequestViewModel
    {
        Id = r.Id,
        TenantName = r.TenantName,
        Title = r.Title,
        Description = r.Description,
        HouseNumber = r.HouseNumber, // Make sure this is mapped
        Priority = r.Priority,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        LandlordNotes = r.LandlordNotes
    }).ToList();

    var viewModel = new RequestListViewModel
    {
        Requests = viewModels,
        FilterStatus = status,
        SearchTerm = searchTerm,
        TotalRequests = requestDtos.Count,
        PendingRequests = requestDtos.Count(r => r.Status == RequestStatus.Pending)
    };

    return View("~/Views/Landlord/Request.cshtml", viewModel);
}

	[HttpGet]
	[Route("{id}")]
	public async Task<IActionResult> GetRequest(int id)
	{
		try
		{
			var request = await _requestService.GetRequestByIdAsync(id);
			return Ok(request);
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(ex.Message);
		}
	}

	[HttpPost]
	[Route("")]
	public async Task<IActionResult> Add([FromBody] CreateRequestDto dto)
	{
		try
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var request = await _requestService.CreateRequestAsync(dto);
			return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
		}
		catch (Exception ex)
		{
			// Log the error
			return StatusCode(500, "An error occurred while creating the request.");
		}
	}

[HttpPut]
[Route("{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [FromBody] UpdateRequestDto dto)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized("Invalid User ID.");
        }

        // Verify the request belongs to the landlord
        var request = await _context.Requests
            .Include(r => r.Tenant)
            .ThenInclude(t => t.House)
            .ThenInclude(h => h.Property)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound($"Request with ID {id} not found.");
        }

        if (request.Tenant.House.Property.UserId != userId)
        {
            return Unauthorized("You don't have permission to edit this request.");
        }

        var updatedRequest = await _requestService.UpdateRequestStatusAsync(id, dto);
        return Ok(updatedRequest);
    }
    catch (Exception ex)
    {
        // Log the error
        return StatusCode(500, $"An error occurred while updating the request: {ex.Message}");
    }
}

[HttpDelete]
[Route("{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    try
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized("Invalid User ID.");
        }

        // Verify the request belongs to the landlord
        var request = await _context.Requests
            .Include(r => r.Tenant)
            .ThenInclude(t => t.House)
            .ThenInclude(h => h.Property)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound($"Request with ID {id} not found.");
        }

        if (request.Tenant.House.Property.UserId != userId)
        {
            return Unauthorized("You don't have permission to delete this request.");
        }

        await _requestService.DeleteRequestAsync(id);
        return NoContent();
    }
    catch (Exception ex)
    {
        // Log the error
        return StatusCode(500, $"An error occurred while deleting the request: {ex.Message}");
    }
}
	[HttpGet]
	[Route("Check/{propertyId}/{tenantId}")]
	public async Task<IActionResult> CheckExistingRequest(int propertyId, int tenantId)
	{
		try
		{
			var existingRequest = await _context.Requests
				.AnyAsync(r => r.TenantId == tenantId && 
							  r.Status != RequestStatus.Completed);
			
			return Ok(existingRequest);
		}
		catch (Exception ex)
		{
			// Log the error
			return StatusCode(500, "An error occurred while checking for existing requests.");
		}
	}
}
}