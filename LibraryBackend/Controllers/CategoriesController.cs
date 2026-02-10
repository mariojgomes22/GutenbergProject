using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers;

/// <summary>
/// Controller for managing book categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly LibraryContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CategoriesController(LibraryContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a list of all categories.
    /// </summary>
    /// <returns>A list of categories.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific category by ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The category details if found; otherwise, NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category == null ? NotFound() : category;
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <returns>The created category.</returns>
    [HttpPost]
    public async Task<ActionResult<Category>> PostCategory(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetCategory", new { id = category.Id }, category);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The ID of the category to update.</param>
    /// <param name="category">The updated category object.</param>
    /// <returns>NoContent if successful; otherwise, BadRequest or NotFound.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(int id, Category category)
    {
        if (id != category.Id) return BadRequest();

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <returns>NoContent if successful; otherwise, NotFound.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}
