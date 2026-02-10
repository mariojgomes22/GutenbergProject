using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers;

/// <summary>
/// Controller for managing books in the library.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly LibraryContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BooksController(LibraryContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a paginated list of books, optionally filtered by a search term.
    /// </summary>
    /// <param name="search">The search term to filter by title, author, or ISBN.</param>
    /// <param name="page">The page number (default is 1).</param>
    /// <param name="pageSize">The number of items per page (default is 10).</param>
    /// <returns>A paginated result containing the list of books.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Book>>> GetBooks(string? search, int page = 1, int pageSize = 10)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.ISBN.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<Book>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Retrieves a specific book by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <returns>The book details if found; otherwise, NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
            
        if (book == null) return NotFound();
        return book;
    }

    /// <summary>
    /// Adds a new book to the library.
    /// </summary>
    /// <param name="book">The book to add.</param>
    /// <returns>The created book.</returns>
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetBook", new { id = book.Id }, book);
    }

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    /// <param name="id">The ID of the book to update.</param>
    /// <param name="book">The updated book object.</param>
    /// <returns>NoContent if successful; otherwise, BadRequest or NotFound.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.Id) return BadRequest();
        _context.Entry(book).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) {
            if (!BookExists(id)) return NotFound();
            else throw;
        }
        return NoContent();
    }

    /// <summary>
    /// Deletes a book from the library.
    /// </summary>
    /// <param name="id">The ID of the book to delete.</param>
    /// <returns>NoContent if successful; otherwise, NotFound.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The collection of items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// The total number of items matching the query.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }
}
