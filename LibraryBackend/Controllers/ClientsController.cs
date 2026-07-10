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
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        return client == null ? NotFound() : client;
    }

    [HttpPost]
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

        if (!await _context.Clients.AnyAsync(c => c.Id == id)) return NotFound();

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
