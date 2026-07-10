using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly LibraryContext _context;

    public ClientsController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        var currentClient = await GetCurrentClientAsync();
        if (currentClient == null) return Unauthorized();

        if (currentClient.Role != "Admin" && currentClient.Id != id)
        {
            return Forbid();
        }

        var client = await _context.Clients.FindAsync(id);
        return client == null ? NotFound() : client;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Client>> PostClient(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetClient", new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutClient(int id, Client client)
    {
        if (id != client.Id) return BadRequest();

        var currentClient = await GetCurrentClientAsync();
        if (currentClient == null) return Unauthorized();

        if (currentClient.Role != "Admin" && currentClient.Id != id)
        {
            return Forbid();
        }

        var existing = await _context.Clients.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = client.Name;
        existing.Email = client.Email;

        if (currentClient.Role == "Admin")
        {
            // Only an Admin can change roles — never taken from a self-service edit.
            existing.Role = client.Role;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClientExists(id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }

    private async Task<Client?> GetCurrentClientAsync()
    {
        var email = User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("upn")?.Value;

        if (string.IsNullOrEmpty(email)) return null;

        return await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
    }
}
