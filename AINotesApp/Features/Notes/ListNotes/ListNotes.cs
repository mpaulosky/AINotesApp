using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.ListNotes;

/// <summary>
/// Query to list all notes for a user
/// </summary>
public record ListNotesQuery : IRequest<ListNotesResponse>
{
    /// <summary>
    /// ID of the user whose notes to list
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Optional search term to filter notes
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Sort field (Title, CreatedAt, UpdatedAt)
    /// </summary>
    public string SortBy { get; init; } = "UpdatedAt";

    /// <summary>
    /// Sort direction (Ascending or Descending)
    /// </summary>
    public bool IsDescending { get; init; } = true;
}

/// <summary>
/// Response containing a list of notes
/// </summary>
public record ListNotesResponse
{
    /// <summary>
    /// List of notes
    /// </summary>
    public List<NoteListItem> Notes { get; init; } = new();

    /// <summary>
    /// Total number of notes matching the query
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Individual note item in the list
/// </summary>
public record NoteListItem
{
    /// <summary>
    /// ID of the note
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Title of the note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Optional summary of the note
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Tags for the note (comma-separated)
    /// </summary>
    public string? Tags { get; init; }

    /// <summary>
    /// When the note was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// When the note was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Handler for listing notes
/// </summary>
public class ListNotesHandler : IRequestHandler<ListNotesQuery, ListNotesResponse>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListNotesHandler"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public ListNotesHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the list notes query
    /// </summary>
    /// <param name="query">The query containing filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing paginated list of notes</returns>
    public async Task<ListNotesResponse> Handle(ListNotesQuery request, CancellationToken cancellationToken = default)
    {
        var notesQuery = _context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == request.UserId);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            notesQuery = notesQuery.Where(n =>
                n.Title.Contains(request.SearchTerm) ||
                n.Content.Contains(request.SearchTerm));
        }

        var totalCount = await notesQuery.CountAsync(cancellationToken);

        // Apply sorting
        notesQuery = request.SortBy.ToLower() switch
        {
            "title" => request.IsDescending ? notesQuery.OrderByDescending(n => n.Title) : notesQuery.OrderBy(n => n.Title),
            "createdat" => request.IsDescending ? notesQuery.OrderByDescending(n => n.CreatedAt) : notesQuery.OrderBy(n => n.CreatedAt),
            _ => request.IsDescending ? notesQuery.OrderByDescending(n => n.UpdatedAt) : notesQuery.OrderBy(n => n.UpdatedAt)
        };

        var notes = await notesQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NoteListItem
            {
                Id = n.Id,
                Title = n.Title,
                Summary = n.Summary,
                Tags = n.Tags,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new ListNotesResponse
        {
            Notes = notes,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
