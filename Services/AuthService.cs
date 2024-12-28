using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RentalManagementSystem.DTOs;


public interface IAuthService
{
    Task<User> RegisterUserAsync(string email, string password, string role);
    Task<User> FindUserByEmailAsync(string email);
    Task<bool> VerifyPasswordAsync(User user, string password);
    Task<string> GenerateJwtTokenAsync(string email, string role);
    Task<AuthResponse> GenerateAuthTokensAsync(User user);
    Task<AuthResponse> ValidateAndGenerateTokensAsync(string refreshToken);
    Task<(User user, string errorMessage)> ValidateLoginAsync(string email, string password);
    Task<bool> LogoutUserAsync(string email);
}

public class AuthService : IAuthService
{
    private readonly RentalManagementContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(RentalManagementContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Registers a new user with hashed password and basic validation
    public async Task<User> RegisterUserAsync(string email, string password, string role)
    {
        if (!IsPasswordValid(password))
        {
            throw new ArgumentException("Password does not meet complexity requirements.");
        }

        var user = new User
        {
            Email = email,
            Role = role,
            LastLoginDate = null,
            FailedLoginAttempts = 0
        };

        // Use BCrypt for password hashing
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    // Finds a user by their email
    public async Task<User> FindUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidOperationException($"No user found with email {email}");
    }
    // Verifies the user's password against the stored hash
    public async Task<bool> VerifyPasswordAsync(User user, string password)
    {
        if (user == null || string.IsNullOrEmpty(password))
            return false;

        try
        {
            // Use BCrypt for password verification
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception)
        {
            // Log the error if needed
            return false;
        }
    }

    // Generates a JWT token for authentication
    public async Task<string> GenerateJwtTokenAsync(string email, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key not configured")));
        Console.WriteLine(_configuration["Jwt:Key"]);

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generates a short-lived access token and refresh token for the user
    public async Task<AuthResponse> GenerateAuthTokensAsync(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(30);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = HashRefreshToken(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Role = user.Role,
            Expiration = expiration
        };
    }

    public async Task<(User user, string errorMessage)> ValidateLoginAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return (null, "User not found.");
        }

        if (user.FailedLoginAttempts >= 5)
        {
            return (null, "Account locked due to too many failed login attempts.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return (null, "Invalid user account.");
        }

        // Use the BCrypt verification method
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            user.FailedLoginAttempts += 1;
            await _context.SaveChangesAsync();
            return (null, "Invalid password.");
        }

        user.LastLoginDate = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        await _context.SaveChangesAsync();

        return (user, null);
    }

    public async Task<AuthResponse> ValidateAndGenerateTokensAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        return await GenerateAuthTokensAsync(user);
    }

    public async Task<bool> LogoutUserAsync(string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                // Clear refresh token data
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                
                // Update last logout time if you want to track it
                user.LastLoginDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool IsPasswordValid(string password)
    {
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        return regex.IsMatch(password);
    }

    // Generates a secure refresh token
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Hashes the refresh token for secure storage
    private string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashBytes);
    }

    // Verifies the refresh token by comparing its hashed value
    private bool VerifyHashedRefreshToken(string hashedToken, string refreshToken)
    {
        return hashedToken == HashRefreshToken(refreshToken);
    }
}


