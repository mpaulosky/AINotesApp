using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.CreateNote;

/// <summary>
/// Handler for creating a new note
/// </summary>
public class CreateNoteHandler
{
    private readonly ApplicationDbContext _context;

    public CreateNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateNoteResponse> HandleAsync(CreateNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Content = command.Content,
            UserId = command.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateNoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        };
    }
}
