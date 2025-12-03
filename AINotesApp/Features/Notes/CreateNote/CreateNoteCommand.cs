namespace AINotesApp.Features.Notes.CreateNote;

/// <summary>
/// Command to create a new note
/// </summary>
public record CreateNoteCommand
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}
