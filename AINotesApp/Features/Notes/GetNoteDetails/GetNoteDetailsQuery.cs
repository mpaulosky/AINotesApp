namespace AINotesApp.Features.Notes.GetNoteDetails;

/// <summary>
/// Query to get details of a specific note
/// </summary>
public record GetNoteDetailsQuery
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
}
