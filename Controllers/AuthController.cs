using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


public class AuthController : Controller
{
	private readonly RentalManagementContext _context;
	private readonly IAuthService _authService;
	private readonly ILogger<AuthController> _logger;

	public AuthController(RentalManagementContext context, IAuthService authService, ILogger<AuthController> logger)
	{
		_context = context;
		_authService = authService;
		_logger = logger;
	}

	[HttpGet]
	public IActionResult ChangePassword()
	{
		if (User.Identity.IsAuthenticated)

		{
			return RedirectToAction("Login");
		}
		return View(new ChangePasswordViewModel());
	}


	public IActionResult Login()
	{
		if (User.Identity.IsAuthenticated)
		{
			return RedirectToUserDashboard(User.FindFirst(ClaimTypes.Role)?.Value);
		}

		var model = new LoginViewModel();

		if (TempData["SuccessMessage"] != null)
		{
			ViewData["SuccessMessage"] = TempData["SuccessMessage"];
		}

		if (TempData["ErrorMessage"] != null)
		{
			ViewData["ErrorMessage"] = TempData["ErrorMessage"];
		}

		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var (user, errorMessage) = await _authService.ValidateLoginAsync(model.Email, model.Password);

		if (user == null)
		{
			ModelState.AddModelError(string.Empty, errorMessage);
			return View(model);
		}

		var authResponse = await _authService.GenerateAuthTokensAsync(user);
		var claims = new List<Claim>
	{
		new Claim(ClaimTypes.Name, user.FullName),
		new Claim(ClaimTypes.Email, user.Email),
		new Claim(ClaimTypes.Role, user.Role),
		new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
	};

		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		var authProperties = new AuthenticationProperties
		{
			IsPersistent = true,
			ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
		};

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			authProperties);

		Response.Cookies.Append("AccessToken", authResponse.Token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = authResponse.Expiration
		});

		Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = DateTime.UtcNow.AddDays(7)
		});

		return RedirectToUserDashboard(user.Role);
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		try
		{
			var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

			if (!string.IsNullOrEmpty(userEmail))
			{
				await _authService.LogoutUserAsync(userEmail);
			}


			await HttpContext.SignOutAsync();

			Response.Cookies.Delete("AccessToken");
			Response.Cookies.Delete("RefreshToken");
			Response.Cookies.Delete("Role");

			HttpContext.Session.Clear();

			_logger.LogInformation($"User {userEmail} logged out successfully");

			return RedirectToAction("Login", "Auth");
		}
		catch (Exception ex)
		{
			_logger.LogError($"Error during logout: {ex.Message}");
			return RedirectToAction("Login", "Auth");
		}
	}


	private IActionResult RedirectToUserDashboard(string role)
	{
		return Redirect(GetDashboardUrl(role));
	}

	private string GetDashboardUrl(string role)
	{
		return role?.ToLower() switch
		{
			"landlord" => "/Landlord/Dashboard",
			"tenant" => "/Tenant/Dashboard",
			_ => "/"
		};
	}


	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return Json(new { success = false, message = "Please fill in all required fields." });
		}

		try
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			var result = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

			return Json(new { success = result.success, message = result.message });
		}
		catch (Exception ex)
		{
			_logger.LogError($"Password change error: {ex.Message}");
			return Json(new { success = false, message = "An error occurred while changing the password." });
		}
	}
}