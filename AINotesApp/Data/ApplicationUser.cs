using Microsoft.AspNetCore.Identity;

namespace AINotesApp.Data;

/// <summary>
/// Application user with notes collection
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Collection of notes owned by this user
    /// </summary>
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}