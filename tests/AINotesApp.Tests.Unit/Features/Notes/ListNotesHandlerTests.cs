using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using AINotesApp.Features.Notes.ListNotes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
/// Unit tests for ListNotesHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class ListNotesHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly ListNotesHandler _handler;

    public ListNotesHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new ListNotesHandler(_context);
    }

    [Fact]
    public async Task Handle_NoNotes_ReturnsEmptyList()
    {
        // Given
        var query = new ListNotesQuery
        {
            UserId = "user-123",
            PageNumber = 1,
            PageSize = 10
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Notes.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNotes_ReturnsCorrectPage()
    {
        // Given
        var userId = "user-123";
        var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Note 1", UserId = userId, CreatedAt = DateTime.UtcNow.AddHours(-3), UpdatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = Guid.NewGuid(), Title = "Note 2", UserId = userId, CreatedAt = DateTime.UtcNow.AddHours(-2), UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Note 3", UserId = userId, CreatedAt = DateTime.UtcNow.AddHours(-1), UpdatedAt = DateTime.UtcNow.AddHours(-2) }
        };

        _context.Notes.AddRange(notes);
        await _context.SaveChangesAsync();

        var query = new ListNotesQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Notes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(1);
        // Should be ordered by UpdatedAt descending
        result.Notes[0].Title.Should().Be("Note 2");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Given
        var userId = "user-123";
        for (int i = 0; i < 15; i++)
        {
            _context.Notes.Add(new Note
            {
                Id = Guid.NewGuid(),
                Title = $"Note {i}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(-i),
                UpdatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await _context.SaveChangesAsync();

        var query = new ListNotesQuery
        {
            UserId = userId,
            PageNumber = 2,
            PageSize = 5
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Notes.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsOnlyUserNotes()
    {
        // Given
        var userId1 = "user-1";
        var userId2 = "user-2";

        _context.Notes.AddRange(
            new Note { Id = Guid.NewGuid(), Title = "User 1 Note", UserId = userId1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { Id = Guid.NewGuid(), Title = "User 2 Note", UserId = userId2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var query = new ListNotesQuery
        {
            UserId = userId1,
            PageNumber = 1,
            PageSize = 10
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Notes.Should().HaveCount(1);
        result.Notes[0].Title.Should().Be("User 1 Note");
    }

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(2, 5, 5)]
    [InlineData(3, 5, 2)]
    [InlineData(4, 5, 0)]
    public async Task Handle_VariousPagination_ReturnsCorrectCounts(int pageNumber, int pageSize, int expectedCount)
    {
        // Given
        var userId = "user-123";
        for (int i = 0; i < 12; i++)
        {
            _context.Notes.Add(new Note
            {
                Id = Guid.NewGuid(),
                Title = $"Note {i}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var query = new ListNotesQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Notes.Should().HaveCount(expectedCount);
        result.TotalCount.Should().Be(12);
    }

    [Fact]
    public async Task Handle_WithAiSummary_IncludesSummaryInResults()
    {
        // Given
        var userId = "user-123";
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note with Summary",
            Content = "Content",
            AiSummary = "This is a summary",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        var query = new ListNotesQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10
        };

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Notes.Should().HaveCount(1);
        result.Notes[0].AiSummary.Should().Be("This is a summary");
    }
}
