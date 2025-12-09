namespace AINotesApp.Data;

/// <summary>
/// Represents a note created by a user.
/// </summary>
public class Note
{
    /// <summary>
    /// Gets or sets the unique identifier for the note.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the note.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the note.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AI-generated summary of the note.
    /// </summary>
    public string? AiSummary { get; set; }

    /// <summary>
    /// Gets or sets the AI-generated tags for the note.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the AI vector embedding for semantic search.
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who owns this note.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who owns this note.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
