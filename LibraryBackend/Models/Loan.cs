namespace LibraryBackend.Models;

/// <summary>
/// Represents a loan record of a book to a client.
/// </summary>
public class Loan
{
    /// <summary>
    /// Unique identifier for the loan.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key for the loaned book.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Navigation property for the loaned book.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Foreign key for the client who borrowed the book.
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// Navigation property for the client.
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// The date and time when the loan started.
    /// </summary>
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the book was returned (null if not yet returned).
    /// </summary>
    public DateTime? ReturnDate { get; set; }
}
