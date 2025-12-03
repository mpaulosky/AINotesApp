using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.UpdateNote;

/// <summary>
/// Handler for updating an existing note
/// </summary>
public class UpdateNoteHandler
{
    private readonly ApplicationDbContext _context;

    public UpdateNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateNoteResponse?> HandleAsync(UpdateNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == command.Id && n.UserId == command.UserId, cancellationToken);

        if (note == null)
        {
            return null;
        }

        note.Title = command.Title;
        note.Content = command.Content;
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
