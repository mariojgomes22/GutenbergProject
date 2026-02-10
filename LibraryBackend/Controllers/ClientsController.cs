using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;
using BCrypt.Net;

namespace LibraryBackend.Controllers;

/// <summary>
/// Controller for managing library clients (users).
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly LibraryContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientsController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClientsController(LibraryContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a list of all clients.
    /// </summary>
    /// <returns>A list of clients.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients.ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific client by ID.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>The client details if found; otherwise, NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        return client == null ? NotFound() : client;
    }

    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <param name="client">The client to create.</param>
    /// <returns>The created client.</returns>
    [HttpPost]
    public async Task<ActionResult<Client>> PostClient(Client client)
    {
        // Hash password for new clients
        client.Password = BCrypt.Net.BCrypt.HashPassword(client.Password);
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetClient", new { id = client.Id }, client);
    }

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="id">The ID of the client to update.</param>
    /// <param name="client">The updated client object.</param>
    /// <returns>NoContent if successful; otherwise, BadRequest or NotFound.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClient(int id, Client client)
    {
        if (id != client.Id) return BadRequest();

        var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (existingClient == null) return NotFound();

        // Password handling logic:
        // 1. If password field is empty, keep existing password.
        // 2. If password matches existing hash (handled by frontend sending back old hash), keep it.
        // 3. If password differs and is not empty, hash the new password.
        if (string.IsNullOrEmpty(client.Password))
        {
             client.Password = existingClient.Password;
        }
        else if (client.Password != existingClient.Password)
        {
             if (BCrypt.Net.BCrypt.Verify(client.Password, existingClient.Password))
             {
                 client.Password = existingClient.Password; // It was the old password provided in plain text or hash matches
             }
             else
             {
                 client.Password = BCrypt.Net.BCrypt.HashPassword(client.Password); // New password
             }
        }

        _context.Entry(client).State = EntityState.Modified;

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

    /// <summary>
    /// Deletes a client.
    /// </summary>
    /// <param name="id">The ID of the client to delete.</param>
    /// <returns>NoContent if successful; otherwise, NotFound.</returns>
    [HttpDelete("{id}")]
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
}
