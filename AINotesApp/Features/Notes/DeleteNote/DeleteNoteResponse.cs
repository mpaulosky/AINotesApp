namespace AINotesApp.Features.Notes.DeleteNote;

/// <summary>
/// Response after deleting a note
/// </summary>
public record DeleteNoteResponse
{
    public bool Success { get; init; }
    public Guid? DeletedId { get; init; }
}
