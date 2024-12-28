using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


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

    // GET: /Auth/Login
    public IActionResult Login()
    {
        // If user is already authenticated, redirect to appropriate dashboard
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
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError($"ModelState Error: {error.ErrorMessage}");
            }
            return View(model);
        }

        var (user, errorMessage) = await _authService.ValidateLoginAsync(model.Email, model.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            _logger.LogWarning($"Login failed for email: {model.Email}. Reason: {errorMessage}");
            return View(model);
        }

        var authResponse = await _authService.GenerateAuthTokensAsync(user);

        // Set tokens in cookies
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

        _logger.LogInformation($"User {user.Email} logged in successfully with role: {user.Role}");
        TempData["SuccessMessage"] = "Login successful!";
        
        // Redirect based on user role
        return RedirectToUserDashboard(user.Role);
    }

[HttpPost]
public async Task<IActionResult> Logout()
{
    try
    {
        // Get the current user's email from claims
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(userEmail))
        {
            // Optionally, perform server-side token revocation if you store tokens
            await _authService.LogoutUserAsync(userEmail);
        }

        // Clear client-side authentication cookies
        Response.Cookies.Delete("AccessToken", new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            HttpOnly = true
        });

        Response.Cookies.Delete("RefreshToken", new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            HttpOnly = true
        });

        // Clear any additional session or custom cookies
        Response.Cookies.Delete("Role");

        // Clear session data
        HttpContext.Session.Clear();

        _logger.LogInformation($"User {userEmail} logged out successfully");
        
        TempData["SuccessMessage"] = "You have been successfully logged out.";
        return RedirectToAction("Login");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error during logout: {ex.Message}");
        TempData["ErrorMessage"] = "An error occurred during logout.";
        return RedirectToAction("Login");
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
}