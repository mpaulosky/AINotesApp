using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.DeleteNote;

/// <summary>
/// Command to delete a note
/// </summary>
public record DeleteNoteCommand : IRequest<DeleteNoteResponse>
{
    /// <summary>
    /// ID of the note to delete
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the user deleting the note
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Response after deleting a note
/// </summary>
public record DeleteNoteResponse
{
    /// <summary>
    /// Indicates whether the deletion was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// ID of the deleted note, if successful
    /// </summary>
    public Guid? DeletedId { get; init; }
}

/// <summary>
/// Handler for deleting a note
/// </summary>
public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, DeleteNoteResponse>
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNoteHandler"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public DeleteNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the delete note command
    /// </summary>
    /// <param name="request">The command containing the note ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response indicating success or failure of the deletion</returns>
    public async Task<DeleteNoteResponse> Handle(DeleteNoteCommand request, CancellationToken cancellationToken = default)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

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
