using AINotesApp.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.ListNotes
{
    public record ListNotesQuery : IRequest<ListNotesResponse>
    {
        public string UserId { get; init; } = string.Empty;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class ListNotesHandler : IRequestHandler<ListNotesQuery, ListNotesResponse>
    {
        private readonly ApplicationDbContext _context;

        public ListNotesHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ListNotesResponse> Handle(ListNotesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Notes
                .AsNoTracking()
                .Where(n => n.UserId == request.UserId)
                .OrderByDescending(n => n.UpdatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var notes = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NoteListItem
                {
                    Id = n.Id,
                    Title = n.Title,
                    AiSummary = n.AiSummary,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new ListNotesResponse
            {
                Notes = notes,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }

    public record ListNotesResponse
    {
        public List<NoteListItem> Notes { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public record NoteListItem
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? AiSummary { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
