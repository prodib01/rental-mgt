// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RentalManagementContext _context; // Use your specific context
    private readonly AuthService _authService;

    // Corrected constructor to use RentalManagementContext instead of DbContext
    public AuthController(RentalManagementContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
    {
        // Check if the user already exists
        var existingUser = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == registrationDto.Email);
        if (existingUser != null)
            return BadRequest("User already exists.");

        // Create a new user with hashed password
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = registrationDto.Email,
            Role = registrationDto.Role,
            PasswordHash = passwordHasher.HashPassword(null, registrationDto.Password)
        };

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync();
        return Ok("Registration successful.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        // Find the user by email
        var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        // Verify the password
        var passwordHasher = new PasswordHasher<User>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginDto.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password.");

        // Generate a JWT token
        var token = _authService.GenerateJwtToken(user.Email, user.Role);
        return Ok(new { Token = token });
    }
}
