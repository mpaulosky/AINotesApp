namespace AINotesApp.Features.Notes.UpdateNote;

/// <summary>
/// Response after updating a note
/// </summary>
public record UpdateNoteResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}
