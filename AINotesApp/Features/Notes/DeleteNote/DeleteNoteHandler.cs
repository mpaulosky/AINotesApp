using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.DeleteNote;

/// <summary>
/// Handler for deleting a note
/// </summary>
public class DeleteNoteHandler
{
    private readonly ApplicationDbContext _context;

    public DeleteNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteNoteResponse> HandleAsync(DeleteNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == command.Id && n.UserId == command.UserId, cancellationToken);

        if (note == null)
        {
            return new DeleteNoteResponse { Success = false };
        }

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteNoteResponse
        {
            Success = true,
            DeletedId = note.Id
        };
    }
}
