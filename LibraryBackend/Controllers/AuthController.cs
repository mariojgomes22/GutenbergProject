using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;
using System.Security.Claims;

namespace LibraryBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(LibraryContext context) : ControllerBase
{
    private readonly LibraryContext _context = context;

    [HttpGet("me")]
    public async Task<ActionResult<Client>> GetCurrentUser()
    {
        var email = User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("upn")?.Value;

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _context.Clients.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // First login — cria o utilizador automaticamente com role User
            var name = User.FindFirst("name")?.Value ?? email;
            user = new Client { Name = name, Email = email, Role = "User" };
            _context.Clients.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }
}
