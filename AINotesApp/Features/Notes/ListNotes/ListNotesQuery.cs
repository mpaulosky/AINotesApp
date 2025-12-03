namespace AINotesApp.Features.Notes.ListNotes;

/// <summary>
/// Query to list all notes for a user
/// </summary>
public record ListNotesQuery
{
    public string UserId { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}
