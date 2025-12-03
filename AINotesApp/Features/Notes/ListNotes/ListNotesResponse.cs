namespace AINotesApp.Features.Notes.ListNotes;

/// <summary>
/// Response containing a list of notes
/// </summary>
public record ListNotesResponse
{
    public List<NoteListItem> Notes { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Individual note item in the list
/// </summary>
public record NoteListItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
