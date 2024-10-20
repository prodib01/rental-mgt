using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For EF Core methods like FirstOrDefaultAsync


namespace RentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly RentalManagementContext _context;

        public AuthController(AuthService authService, RentalManagementContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
        {
            // Validate user credentials
            if (request.UserType == "Tenant")
            {
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Email == request.Username);
                
                if (tenant == null) return Unauthorized();
            }
            else if (request.UserType == "Landlord")
            {
                var landlord = await _context.Landlords
                    .FirstOrDefaultAsync(l => l.Email == request.Username);
                
                if (landlord == null) return Unauthorized();
            }

            // Generate JWT token
            var token = _authService.GenerateJwtToken(request.Username, request.UserType);
            return Ok(new { Token = token });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } // Email or username
        public string UserType { get; set; } // "Tenant" or "Landlord"
    }
}
