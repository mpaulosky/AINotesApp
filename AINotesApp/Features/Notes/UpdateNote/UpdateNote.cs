using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.UpdateNote;

/// <summary>
/// request to update an existing note
/// </summary>
public record UpdateNoterequest : IRequest<UpdateNoteResponse>
{
    /// <summary>
    /// ID of the note to update
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Updated title of the note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Updated content of the note
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Updated tags for the note (comma-separated)
    /// </summary>
    public string? Tags { get; init; }

    /// <summary>
    /// ID of the user updating the note
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Response after updating a note
/// </summary>
public record UpdateNoteResponse
{
    /// <summary>
    /// ID of the updated note
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Updated title of the note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Updated content of the note
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// When the note was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Handler for updating an existing note
/// </summary>
public class UpdateNoteHandler : IRequestHandler<UpdateNoterequest, UpdateNoteResponse>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateNoteHandler"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public UpdateNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the update note request
    /// </summary>
    /// <param name="request">The request containing updated note details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the updated note details, or null if note not found</returns>
    public async Task<UpdateNoteResponse> Handle(UpdateNoterequest request, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

        if (note == null)
        {
            throw new KeyNotFoundException("Note not found or you don't have permission to edit it.");
        }

        note.Title = request.Title;
        note.Content = request.Content;
        note.Tags = request.Tags;
        note.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateNoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            UpdatedAt = note.UpdatedAt
        };
    }
}
