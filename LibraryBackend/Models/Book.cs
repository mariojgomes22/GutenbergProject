namespace LibraryBackend.Models;

/// <summary>
/// Represents a book in the library inventory.
/// </summary>
public class Book
{
    /// <summary>
    /// Unique identifier for the book.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The author of the book.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The ISBN (International Standard Book Number) of the book.
    /// </summary>
    public string ISBN { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the book is currently available for loan.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>
    /// Foreign key for the associated category.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Navigation property for the book's category.
    /// </summary>
    public Category? Category { get; set; }
}
