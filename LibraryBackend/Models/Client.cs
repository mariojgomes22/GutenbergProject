namespace LibraryBackend.Models;

/// <summary>
/// Represents a library client or user.
/// </summary>
public class Client
{
    /// <summary>
    /// Unique identifier for the client.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The full name of the client.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the client (used for login).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The hashed password of the client.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The role of the client (e.g., "User" or "Admin").
    /// </summary>
    public string Role { get; set; } = "User";
}
