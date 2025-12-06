using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.GetNoteDetails;

/// <summary>
/// Query to get details of a specific note
/// </summary>
public record GetNoteDetailsQuery : IRequest<GetNoteDetailsResponse?>
{
    /// <summary>
    /// ID of the note to retrieve
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the user requesting the note
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Response containing note details
/// </summary>
public record GetNoteDetailsResponse
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
    /// Content of the note
    /// </summary>
    public string Content { get; init; } = string.Empty;

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

    /// <summary>
    /// ID of the user who owns the note
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Handler for getting note details
/// </summary>
public class GetNoteDetailsHandler : IRequestHandler<GetNoteDetailsQuery, GetNoteDetailsResponse?>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNoteDetailsHandler"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public GetNoteDetailsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the get note details query
    /// </summary>
    /// <param name="query">The query containing the note ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the note details, or null if not found</returns>
    public async Task<GetNoteDetailsResponse?> Handle(GetNoteDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == query.Id && n.UserId == query.UserId, cancellationToken);

        if (note == null)
        {
            return null;
        }

        return new GetNoteDetailsResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Summary = note.Summary,
            Tags = note.Tags,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            UserId = note.UserId
        };
    }
}
