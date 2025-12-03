namespace AINotesApp.Features.Notes.DeleteNote;

/// <summary>
/// Command to delete a note
/// </summary>
public record DeleteNoteCommand
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
}
