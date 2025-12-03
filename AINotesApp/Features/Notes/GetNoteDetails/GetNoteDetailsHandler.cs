using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.GetNoteDetails;

/// <summary>
/// Handler for getting note details
/// </summary>
public class GetNoteDetailsHandler
{
    private readonly ApplicationDbContext _context;

    public GetNoteDetailsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NoteDetailsResponse?> HandleAsync(GetNoteDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == query.Id && n.UserId == query.UserId, cancellationToken);

        if (note == null)
        {
            return null;
        }

        return new NoteDetailsResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Summary = note.Summary,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            UserId = note.UserId
        };
    }
}
