namespace LibraryBackend.Models;

/// <summary>
/// Represents a book category or genre.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
