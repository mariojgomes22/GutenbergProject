using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;
using BCrypt.Net;
using Google.Apis.Auth;

namespace LibraryBackend.Controllers;

/// <summary>
/// Controller for handling user authentication and registration.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly LibraryContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AuthController(LibraryContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login request including email and password.</param>
    /// <returns>The authenticated client details.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<Client>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Clients
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Unauthorized("Invalid credentials");
        }

        return user;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request details.</param>
    /// <returns>The created client details.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<Client>> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Clients.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("Email already exists");
        }

        var client = new Client
        {
            Name = request.Name,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User" // Default role
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction("Login", new { }, client);
    }

    /// <summary>
    /// Authenticates or registers a user via Google Sign-In.
    /// </summary>
    /// <param name="request">The Google login request containing the ID token.</param>
    /// <returns>The authenticated client details.</returns>
    [HttpPost("google-login")]
    public async Task<ActionResult<Client>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            
            var user = await _context.Clients.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new Client
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    Password = "", // No password for Google users
                    Role = "User"
                };

                _context.Clients.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid Google Token: {ex.Message}");
        }
    }
}

/// <summary>
/// Data Transfer Object for login requests.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for registration requests.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// The user's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's chosen password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for Google login requests.
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// The Google ID token.
    /// </summary>
    public string IdToken { get; set; } = string.Empty;
}
