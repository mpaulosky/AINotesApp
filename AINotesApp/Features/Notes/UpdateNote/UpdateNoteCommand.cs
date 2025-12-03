namespace AINotesApp.Features.Notes.UpdateNote;

/// <summary>
/// Command to update an existing note
/// </summary>
public record UpdateNoteCommand
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}
