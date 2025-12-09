using AINotesApp.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.GetNoteDetails
{
    public record GetNoteDetailsQuery : IRequest<GetNoteDetailsResponse?>
    {
        public Guid Id { get; init; }
        public string UserId { get; init; } = string.Empty;
    }

    public class GetNoteDetailsHandler : IRequestHandler<GetNoteDetailsQuery, GetNoteDetailsResponse?>
    {
        private readonly ApplicationDbContext _context;

        public GetNoteDetailsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetNoteDetailsResponse?> Handle(GetNoteDetailsQuery request, CancellationToken cancellationToken)
        {
            var note = await _context.Notes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

            if (note == null)
            {
                return null;
            }

            return new GetNoteDetailsResponse
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                AiSummary = note.AiSummary,
                Tags = note.Tags,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }

    public record GetNoteDetailsResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string? AiSummary { get; init; }
        public string? Tags { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
