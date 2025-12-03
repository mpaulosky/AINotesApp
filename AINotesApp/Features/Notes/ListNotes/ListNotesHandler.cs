using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.ListNotes;

/// <summary>
/// Handler for listing notes
/// </summary>
public class ListNotesHandler
{
    private readonly ApplicationDbContext _context;

    public ListNotesHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ListNotesResponse> HandleAsync(ListNotesQuery query, CancellationToken cancellationToken = default)
    {
        var notesQuery = _context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            notesQuery = notesQuery.Where(n =>
                n.Title.Contains(query.SearchTerm) ||
                n.Content.Contains(query.SearchTerm));
        }

        var totalCount = await notesQuery.CountAsync(cancellationToken);

        var notes = await notesQuery
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NoteListItem
            {
                Id = n.Id,
                Title = n.Title,
                Summary = n.Summary,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new ListNotesResponse
        {
            Notes = notes,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
