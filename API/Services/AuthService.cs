using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.Models;
using BCrypt.Net;

namespace UrlShortner.API.Services;

public class AuthService : IAuthService
{
    private readonly UrlShortnerDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UrlShortnerDbContext context,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Validate email format
        if (!IsValidEmail(request.Email))
        {
            return null;
        }

        // Validate password strength (min 8 chars)
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return null;
        }

        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Check if username already exists
        var existingUsername = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUsername != null)
        {
            throw new InvalidOperationException("Username already taken");
        }

        // Hash password with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // Save first to get auto-generated ID

        // Generate tokens AFTER saving to ensure user.Id is populated
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

        await _context.SaveChangesAsync(); // Save refresh token

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return null;
        }

        // Verify password with BCrypt
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        // Find user by refresh token
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null)
        {
            return null;
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return null;
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
