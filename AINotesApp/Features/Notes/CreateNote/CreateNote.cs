using AINotesApp.Data;

using MediatR;

namespace AINotesApp.Features.Notes.CreateNote;

public static class CreateNote
{

    /// <summary>
    /// Command to create a new note
    /// </summary>
    public record Command : IRequest<Response>
    {
        /// <summary>
        /// Title of the note
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Content of the note
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// Tags for the note (comma-separated)
        /// </summary>
        public string? Tags { get; init; }

        /// <summary>
        /// ID of the user creating the note
        /// </summary>
        public string UserId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Response after creating a note
    /// </summary>
    public record Response
    {
        /// <summary>
        /// ID of the created note
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Title of the created note
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Content of the created note
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// When the note was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }
    }

    /// <summary>
    /// Handler for creating a new note
    /// </summary>
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Handles the create note command
        /// </summary>
        /// <param name="reqeest">The command containing note details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing the created note details</returns>
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken = default)
        {
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                UserId = request.UserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }
    }
}
