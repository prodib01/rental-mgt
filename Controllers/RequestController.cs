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
	public async Task<IActionResult> Edit(int id, [FromBody] UpdateRequestDto dto)
	{
		try
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var updatedRequest = await _requestService.UpdateRequestStatusAsync(id, dto);
			return Ok(updatedRequest);
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(ex.Message);
		}
		catch (Exception ex)
		{
			// Log the error
			return StatusCode(500, "An error occurred while updating the request.");
		}
	}

	[HttpDelete]
	[Route("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			await _requestService.DeleteRequestAsync(id);
			return NoContent();
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(ex.Message);
		}
		catch (Exception ex)
		{
			// Log the error
			return StatusCode(500, "An error occurred while deleting the request.");
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