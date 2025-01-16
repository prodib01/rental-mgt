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
using RentalManagementSystem.Services;

namespace RentalManagementSystem.Controllers
{
	[Authorize]
	[Route("Landlord/Profile")]
	public class ProfileController : Controller
	{
		private readonly IProfileService _profileService;
		private readonly IBankService _bankService;
		private readonly RentalManagementContext _context;
		private readonly ILogger<AuthController> _logger;

		public ProfileController(IProfileService profileService, IBankService bankService, RentalManagementContext context, ILogger<AuthController> logger)
		{
			_profileService = profileService;
			_bankService = bankService;
			_context = context;
			_logger = logger;
		}

		[HttpGet("")]  // Empty string means this is the default route
		public async Task<IActionResult> Index()
		{
			var landlordId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			var profile = await _profileService.GetProfileByLandlordIdAsync(landlordId);
			var banks = await _bankService.LoadBanksAsync();

			var viewModel = new ProfileViewModel
			{
				Profile = profile,
				Banks = banks ?? new List<Models.Bank>()
			};

			return View("~/Views/Landlord/Profile.cshtml", viewModel);
		}


		// API endpoints
		[HttpPut("api")]
		public async Task<IActionResult> UpdateProfile([FromBody] CreateProfileDTO profileDTO)
		{
			try
			{
				var landlordId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
				var profile = await _profileService.UpdateProfileAsync(profileDTO, landlordId);
				return Ok(profile);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("api/banks")]
		public async Task<ActionResult<List<Bank>>> GetBanks()
		{
			var banks = await _bankService.LoadBanksAsync();
			return Ok(banks);
		}

		[HttpPost("api")]
		public async Task<ActionResult<Profile>> CreateProfile([FromBody] CreateProfileDTO profileDTO)
		{
			try
			{
				// Debug logging
				_logger.LogInformation($"Received profile creation request with bank: {profileDTO.Bank}");

				var landlordId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

				var existingProfile = await _profileService.GetProfileByLandlordIdAsync(landlordId);
				if (existingProfile != null)
				{
					return BadRequest("Profile already exists for this landlord.");
				}

				var profile = await _profileService.CreateProfileAsync(profileDTO, landlordId);
				return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, profile);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error creating profile: {ex.Message}");
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("api")]
		public async Task<ActionResult<Profile>> GetProfile()
		{
			var landlordId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			var profile = await _profileService.GetProfileByLandlordIdAsync(landlordId);

			if (profile == null)
			{
				return NotFound();
			}

			return Ok(profile);
		}
	}
}