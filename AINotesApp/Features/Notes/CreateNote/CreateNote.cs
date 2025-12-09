using AINotesApp.Data;
using AINotesApp.Services.Ai;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.CreateNote
{
    /// <summary>
    /// Command to create a new note.
    /// </summary>
    public record CreateNoteCommand : IRequest<CreateNoteResponse>
    {
        /// <summary>
        /// Gets the title of the note.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the content of the note.
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// Gets the user ID who owns the note.
        /// </summary>
        public string UserId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Handler for creating a new note with AI-generated summary, tags, and embeddings.
    /// </summary>
    public class CreateNoteHandler : IRequestHandler<CreateNoteCommand, CreateNoteResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public CreateNoteHandler(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        /// <summary>
        /// Handles the command to create a note.
        /// </summary>
        public async Task<CreateNoteResponse> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            // Generate AI summary, tags, and embedding in parallel
            var summaryTask = _aiService.GenerateSummaryAsync(request.Content, cancellationToken);
            var tagsTask = _aiService.GenerateTagsAsync(request.Title, request.Content, cancellationToken);
            var embeddingTask = _aiService.GenerateEmbeddingAsync(request.Content, cancellationToken);

            await Task.WhenAll(summaryTask, tagsTask, embeddingTask);

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                AiSummary = await summaryTask,
                Tags = await tagsTask,
                Embedding = await embeddingTask,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateNoteResponse
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                AiSummary = note.AiSummary,
                Tags = note.Tags,
                CreatedAt = note.CreatedAt
            };
        }
    }

    /// <summary>
    /// Response after creating a note.
    /// </summary>
    public record CreateNoteResponse
    {
        /// <summary>
        /// Gets the ID of the created note.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the title of the note.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the content of the note.
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// Gets the AI-generated summary.
        /// </summary>
        public string? AiSummary { get; init; }

        /// <summary>
        /// Gets the AI-generated tags.
        /// </summary>
        public string? Tags { get; init; }

        /// <summary>
        /// Gets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; init; }
    }
}
