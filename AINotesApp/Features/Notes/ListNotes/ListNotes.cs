using AINotesApp.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.ListNotes
{
    /// <summary>
    /// Query to list notes for a user with pagination.
    /// </summary>
    public record ListNotesQuery : IRequest<ListNotesResponse>
    {
        /// <summary>
        /// Gets the user ID whose notes are being listed.
        /// </summary>
        public string UserId { get; init; } = string.Empty;
        /// <summary>
        /// Gets the page number for pagination.
        /// </summary>
        public int PageNumber { get; init; } = 1;
        /// <summary>
        /// Gets the page size for pagination.
        /// </summary>
        public int PageSize { get; init; } = 10;
    }

    /// <summary>
    /// Handler for listing notes for a user with pagination.
    /// </summary>
    public class ListNotesHandler : IRequestHandler<ListNotesQuery, ListNotesResponse>
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListNotesHandler"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ListNotesHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Handles the query to list notes for a user.
        /// </summary>
        /// <param name="request">The query parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response containing the list of notes and pagination info.</returns>
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