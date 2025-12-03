namespace AINotesApp.Features.Notes.CreateNote;

/// <summary>
/// Response after creating a note
/// </summary>
public record CreateNoteResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
