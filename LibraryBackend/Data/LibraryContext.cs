using Microsoft.EntityFrameworkCore;
using LibraryBackend.Models;

namespace LibraryBackend.Data;

/// <summary>
/// Represents the database context for the library application, managing entity sets and database interaction.
/// </summary>
public class LibraryContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the collection of books.
    /// </summary>
    public DbSet<Book> Books { get; set; }

    /// <summary>
    /// Gets or sets the collection of clients (users).
    /// </summary>
    public DbSet<Client> Clients { get; set; }

    /// <summary>
    /// Gets or sets the collection of loans.
    /// </summary>
    public DbSet<Loan> Loans { get; set; }

    /// <summary>
    /// Gets or sets the collection of book categories.
    /// </summary>
    public DbSet<Category> Categories { get; set; }
}
