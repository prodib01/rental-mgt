using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Services;
using System.Security.Claims;
using AutoMapper;
using System.Text.Json;

namespace RentalManagementSystem.Controllers
{
	[Route("Tenant/Requests")]
	public class TRequestController : Controller

	{
		private readonly IRequestService _requestService;
		private readonly RentalManagementContext _context;
		private readonly IMapper _mapper;

		public TRequestController(
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

			// Get requests specifically where TenantId = UserId
			var requests = await _context.Requests
				.Where(r => r.TenantId == userId) // Ensuring only requests of the logged-in user
				.ToListAsync();

			// Filter by status and search term if provided
			var filteredRequests = requests
				.Where(r => !status.HasValue || r.Status == status)
				.Where(r => string.IsNullOrEmpty(searchTerm) ||
						   r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
						   r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
				.ToList();

			var viewModels = filteredRequests.Select(r => new RequestViewModel
			{
				Id = r.Id,
				Title = r.Title,
				Description = r.Description,
				Priority = r.Priority,
				Status = r.Status,
				CreatedAt = r.CreatedAt,
				LandlordNotes = r.LandlordNotes,
			}).ToList();

			var viewModel = new RequestListViewModel
			{
				Requests = viewModels,
				FilterStatus = status,
				SearchTerm = searchTerm,
				TotalRequests = requests.Count,
				PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending),
			};

			return View("~/Views/Tenant/Requests.cshtml", viewModel);
		}

		[HttpPost]
		[Route("")]
		public async Task<IActionResult> Add([FromBody] CreateRequestDto dto)
		{
			try
			{
				// Log the incoming request for debugging
				Console.WriteLine($"Received request: {System.Text.Json.JsonSerializer.Serialize(dto)}");

				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();
					return BadRequest(new { errors });
				}

				var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!int.TryParse(userIdStr, out var userId))
				{
					return BadRequest(new { errors = new[] { "Invalid User ID." } });
				}

				dto.TenantId = userId;

				// Log the DTO after setting TenantId
				Console.WriteLine($"Processing request with TenantId {userId}");

				var request = await _requestService.CreateRequestAsync(dto);
				return Ok(new { success = true, data = request });
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error processing request: {ex}");
				return BadRequest(new { errors = new[] { ex.Message } });
			}
		}
	}

}