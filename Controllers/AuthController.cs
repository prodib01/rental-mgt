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
            ModelState.AddModelError(string.Empty, errorMessage ?? "Invalid login attempt.");
            return View(model);
        }

        var tokenResponse = await _authService.GenerateAuthTokensAsync(user);

        // Store the JWT in a secure cookie
        Response.Cookies.Append("JWT", tokenResponse.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        // Redirect based on user role, with a default 'Unauthorized' role fallback
        var redirectAction = user.Role switch
        {
            "Landlord" => "Dashboard",
            "Tenant" => "Dashboard",
            _ => "Unauthorized" // Redirect to a generic or unauthorized page if the role is unknown
        };

        return RedirectToAction(redirectAction, user.Role);
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
