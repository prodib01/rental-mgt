using Microsoft.AspNetCore.Mvc;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }
            ViewData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model);
        }

        Console.WriteLine($"Attempting login for email: {model.Email}");

        var (user, errorMessage) = await _authService.ValidateLoginAsync(model.Email, model.Password);

        if (user == null)
        {
            Console.WriteLine($"Login failed: {errorMessage}");
            ModelState.AddModelError(string.Empty, errorMessage ?? "Invalid login attempt.");
            return View(model);
        }

        Console.WriteLine("Login successful, generating tokens");
        var tokenResponse = await _authService.GenerateAuthTokensAsync(user);

        // Store the JWT in a secure cookie
        Response.Cookies.Append("JWT", tokenResponse.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        Console.WriteLine($"Redirecting user with role: {user.Role}");

        // Optionally, store the user's role in session or claims for easier access
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserId", user.Id.ToString());

        var redirectAction = user.Role switch
        {
            "Landlord" => "Dashboard",
            "Tenant" => "Dashboard",
            _ => "Unauthorized"
        };

        return RedirectToAction(redirectAction, user.Role);
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
