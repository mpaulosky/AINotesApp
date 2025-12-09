using AINotesApp.Data;
using AINotesApp.Services.Ai;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.GetRelatedNotes
{
    /// <summary>
    /// Query to get related notes based on semantic similarity.
    /// </summary>
    public record GetRelatedNotesQuery : IRequest<GetRelatedNotesResponse>
    {
        /// <summary>
        /// Gets the ID of the note to find related notes for.
        /// </summary>
        public Guid NoteId { get; init; }

        /// <summary>
        /// Gets the user ID for authorization.
        /// </summary>
        public string UserId { get; init; } = string.Empty;

        /// <summary>
        /// Gets the number of related notes to return.
        /// </summary>
        public int TopN { get; init; } = 5;
    }

    /// <summary>
    /// Handler for finding related notes using AI embeddings.
    /// </summary>
    public class GetRelatedNotesHandler : IRequestHandler<GetRelatedNotesQuery, GetRelatedNotesResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public GetRelatedNotesHandler(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        /// <summary>
        /// Handles the query to find related notes.
        /// </summary>
        public async Task<GetRelatedNotesResponse> Handle(GetRelatedNotesQuery request, CancellationToken cancellationToken)
        {
            // Get the current note
            var currentNote = await _context.Notes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == request.NoteId && n.UserId == request.UserId, cancellationToken);

            if (currentNote == null || currentNote.Embedding == null)
            {
                return new GetRelatedNotesResponse();
            }

            // Find related notes using AI service
            var relatedNoteIds = await _aiService.FindRelatedNotesAsync(
                currentNote.Embedding,
                request.UserId,
                request.NoteId,
                request.TopN,
                cancellationToken);

            if (!relatedNoteIds.Any())
            {
                return new GetRelatedNotesResponse();
            }

            // Get the related notes with details
            var relatedNotes = await _context.Notes
                .AsNoTracking()
                .Where(n => relatedNoteIds.Contains(n.Id))
                .Select(n => new RelatedNoteItem
                {
                    Id = n.Id,
                    Title = n.Title,
                    AiSummary = n.AiSummary,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            // Sort by the order returned from AI service (by similarity)
            var sortedNotes = relatedNoteIds
                .Select(id => relatedNotes.FirstOrDefault(n => n.Id == id))
                .Where(n => n != null)
                .ToList();

            return new GetRelatedNotesResponse
            {
                RelatedNotes = sortedNotes!
            };
        }
    }

    /// <summary>
    /// Response containing related notes.
    /// </summary>
    public record GetRelatedNotesResponse
    {
        /// <summary>
        /// Gets the list of related notes.
        /// </summary>
        public List<RelatedNoteItem> RelatedNotes { get; init; } = new();
    }

    /// <summary>
    /// Information about a related note.
    /// </summary>
    public record RelatedNoteItem
    {
        /// <summary>
        /// Gets the note ID.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the note title.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the AI-generated summary.
        /// </summary>
        public string? AiSummary { get; init; }

        /// <summary>
        /// Gets the last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; init; }
    }
}
