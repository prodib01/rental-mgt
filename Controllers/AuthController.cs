// Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;  // Add this if it's missing

public class AuthController : Controller
{
    private readonly RentalManagementContext _context;
    private readonly IAuthService _authService;

    public AuthController(RentalManagementContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // GET: /Auth/Login (For rendering the login form view)
    public IActionResult Login()
    {
        var model = new LoginViewModel();

        // Pass any success or error message to the view
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

    // POST: /Auth/Login (For processing login form submission)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // Return back with validation errors if any
        }

        if (model == null)
        {
            ModelState.AddModelError(string.Empty, "Model cannot be null");
            return View(model);  // Show error if model is null
        }

        var (user, errorMessage) = await _authService.ValidateLoginAsync(model.Email, model.Password);
        if (user == null)
        {
            TempData["ErrorMessage"] = errorMessage; // Store error message in TempData
            return RedirectToAction("Login");  // Redirect to Login page to show error message
        }

        var authTokens = await _authService.GenerateAuthTokensAsync(user);

        // Store success message in TempData
        TempData["SuccessMessage"] = "Login successful!";

        // Optionally store tokens in cookies or session
        return RedirectToAction("Dashboard", "Home"); // Redirect to a dashboard page after login
    }

    // Keep the existing API methods for registration, refresh token, etc.
    [HttpPost("register-tenant")]
    public async Task<IActionResult> RegisterTenant(UserRegistrationDto registrationDto)
    {
        var existingTenant = await _context.Users.FirstOrDefaultAsync(u => u.Email == registrationDto.Email && u.Role == "Tenant");
        if (existingTenant != null)
            return BadRequest("Tenant already exists.");

        var user = await _authService.RegisterUserAsync(registrationDto.Email, registrationDto.Password, "Tenant");
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> ApiLogin([FromBody] UserLoginDto loginDto)
    {
        var (user, errorMessage) = await _authService.ValidateLoginAsync(loginDto.Email, loginDto.Password);
        if (user == null)
        {
            return Unauthorized(new { message = errorMessage });
        }

        var authTokens = await _authService.GenerateAuthTokensAsync(user);

        var userDto = new UserDto
        {
            Email = user.Email,
            Role = user.Role,
            Token = authTokens.Token,
            RefreshToken = authTokens.RefreshToken,
            Expiration = authTokens.Expiration
        };

        return Ok(userDto);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["X-Refresh-Token"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("Refresh token is required.");

        var authResponse = await _authService.ValidateAndGenerateTokensAsync(refreshToken);
        if (authResponse == null)
            return Unauthorized(new { message = "Invalid refresh token." });

        Response.Cookies.Append("X-Refresh-Token", authResponse.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(authResponse);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("X-Refresh-Token");
        return Ok(new { message = "Logged out successfully" });
    }
}
