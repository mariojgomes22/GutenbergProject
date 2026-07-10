using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers;

/// <summary>
/// Controller for managing book loans.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class LoansController : ControllerBase
{
    private readonly LibraryContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoansController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public LoansController(LibraryContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a list of loans, optionally filtered by client ID.
    /// </summary>
    /// <param name="clientId">Optional client ID to filter loans.</param>
    /// <returns>A list of loans.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Loan>>> GetLoans(int? clientId)
    {
        var currentClient = await GetCurrentClientAsync();
        if (currentClient == null) return Unauthorized();

        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Client)
            .AsQueryable();

        if (currentClient.Role != "Admin")
        {
            // Users only ever see their own loans, regardless of what clientId was requested.
            query = query.Where(l => l.ClientId == currentClient.Id);
        }
        else if (clientId.HasValue)
        {
            query = query.Where(l => l.ClientId == clientId.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Creates a new loan (borrows a book).
    /// </summary>
    /// <param name="loan">The loan details.</param>
    /// <returns>The created loan.</returns>
    [HttpPost]
    public async Task<ActionResult<Loan>> PostLoan(Loan loan)
    {
        var currentClient = await GetCurrentClientAsync();
        if (currentClient == null) return Unauthorized();

        if (currentClient.Role != "Admin")
        {
            // Users can only request loans for themselves, never on behalf of someone else.
            loan.ClientId = currentClient.Id;
        }

        // Check if book is available
        var book = await _context.Books.FindAsync(loan.BookId);
        if (book == null || !book.IsAvailable)
        {
            return BadRequest("Book is not available.");
        }

        // Check if client exists
        var client = await _context.Clients.FindAsync(loan.ClientId);
        if (client == null)
        {
            return BadRequest("Client not found.");
        }

        book.IsAvailable = false;
        loan.LoanDate = DateTime.UtcNow;
        loan.ReturnDate = null;
        
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetLoans", new { id = loan.Id }, loan);
    }

    /// <summary>
    /// Marks a loan as returned.
    /// </summary>
    /// <param name="id">The ID of the loan to return.</param>
    /// <returns>NoContent if successful; otherwise, BadRequest or NotFound.</returns>
    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnLoan(int id)
    {
        var currentClient = await GetCurrentClientAsync();
        if (currentClient == null) return Unauthorized();

        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        if (currentClient.Role != "Admin" && loan.ClientId != currentClient.Id)
        {
            return Forbid();
        }

        if (loan.ReturnDate != null)
        {
            return BadRequest("Loan already returned.");
        }

        loan.ReturnDate = DateTime.UtcNow;
        
        var book = await _context.Books.FindAsync(loan.BookId);
        if (book != null)
        {
            book.IsAvailable = true;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Resolves the Client record for the currently authenticated user.
    /// </summary>
    private async Task<Client?> GetCurrentClientAsync()
    {
        var email = User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("upn")?.Value;

        if (string.IsNullOrEmpty(email)) return null;

        return await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
    }
}
