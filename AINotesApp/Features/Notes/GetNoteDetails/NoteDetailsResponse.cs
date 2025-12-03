namespace AINotesApp.Features.Notes.GetNoteDetails;

/// <summary>
/// Response containing note details
/// </summary>
public record NoteDetailsResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string UserId { get; init; } = string.Empty;
}
