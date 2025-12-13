using AINotesApp.Data;
using AINotesApp.Services.Ai;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AINotesApp.Tests.Unit.Services.Ai;

/// <summary>
/// Unit tests for OpenAiService - focusing on internal logic and edge cases.
/// Note: These tests don't make actual API calls to OpenAI.
/// </summary>
public class OpenAiServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly AiServiceOptions _options;

    public OpenAiServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(dbOptions);

        _options = new AiServiceOptions
        {
            ApiKey = "test-api-key",
            ChatModel = "gpt-4o-mini",
            EmbeddingModel = "text-embedding-3-small",
            MaxSummaryTokens = 150,
            RelatedNotesCount = 5,
            SimilarityThreshold = 0.7
        };
    }

    [Fact]
    public async Task FindRelatedNotesAsync_EmptyEmbedding_ReturnsEmptyList()
    {
        // Given
        var service = new OpenAiService(Options.Create(_options), _context);
        var emptyEmbedding = Array.Empty<float>();
        var userId = "user-123";

        // When
        var result = await service.FindRelatedNotesAsync(emptyEmbedding, userId);

        // Then
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindRelatedNotesAsync_NullEmbedding_ReturnsEmptyList()
    {
        // Given
        var service = new OpenAiService(Options.Create(_options), _context);
        float[]? nullEmbedding = null;
        var userId = "user-123";

        // When
        var result = await service.FindRelatedNotesAsync(nullEmbedding!, userId);

        // Then
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindRelatedNotesAsync_NoNotesInDatabase_ReturnsEmptyList()
    {
        // Given
        var service = new OpenAiService(Options.Create(_options), _context);
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var userId = "user-123";

        // When
        var result = await service.FindRelatedNotesAsync(embedding, userId);

        // Then
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindRelatedNotesAsync_WithMatchingNotes_ReturnsRelatedNotes()
    {
        // Given
        var userId = "user-123";
        var queryEmbedding = new float[] { 1.0f, 0.0f, 0.0f };

        var note1 = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Similar Note",
            Content = "Content",
            UserId = userId,
            Embedding = new float[] { 0.9f, 0.1f, 0.0f }, // High similarity
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var note2 = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Different Note",
            Content = "Content",
            UserId = userId,
            Embedding = new float[] { 0.0f, 1.0f, 0.0f }, // Low similarity
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.AddRange(note1, note2);
        await _context.SaveChangesAsync();

        var service = new OpenAiService(Options.Create(_options), _context);

        // When
        var result = await service.FindRelatedNotesAsync(queryEmbedding, userId);

        // Then
        result.Should().NotBeEmpty();
        result.Should().Contain(note1.Id);
    }

    [Fact]
    public async Task FindRelatedNotesAsync_ExcludesCurrentNote_WhenSpecified()
    {
        // Given
        var userId = "user-123";
        var currentNoteId = Guid.NewGuid();
        var queryEmbedding = new float[] { 1.0f, 0.0f };

        var currentNote = new Note
        {
            Id = currentNoteId,
            Title = "Current Note",
            Content = "Content",
            UserId = userId,
            Embedding = new float[] { 1.0f, 0.0f }, // Perfect match
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherNote = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Other Note",
            Content = "Content",
            UserId = userId,
            Embedding = new float[] { 0.9f, 0.1f }, // High similarity
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.AddRange(currentNote, otherNote);
        await _context.SaveChangesAsync();

        var service = new OpenAiService(Options.Create(_options), _context);

        // When
        var result = await service.FindRelatedNotesAsync(queryEmbedding, userId, currentNoteId);

        // Then
        result.Should().NotContain(currentNoteId);
        result.Should().Contain(otherNote.Id);
    }

    [Fact]
    public async Task FindRelatedNotesAsync_OnlyReturnsUserNotes_NotOtherUsers()
    {
        // Given
        var userId1 = "user-1";
        var userId2 = "user-2";
        var queryEmbedding = new float[] { 1.0f, 0.0f };

        var user1Note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "User 1 Note",
            Content = "Content",
            UserId = userId1,
            Embedding = new float[] { 1.0f, 0.0f },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user2Note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "User 2 Note",
            Content = "Content",
            UserId = userId2,
            Embedding = new float[] { 1.0f, 0.0f },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.AddRange(user1Note, user2Note);
        await _context.SaveChangesAsync();

        var service = new OpenAiService(Options.Create(_options), _context);

        // When
        var result = await service.FindRelatedNotesAsync(queryEmbedding, userId1);

        // Then
        result.Should().Contain(user1Note.Id);
        result.Should().NotContain(user2Note.Id);
    }

    [Fact]
    public async Task FindRelatedNotesAsync_RespectsTopNParameter()
    {
        // Given
        var userId = "user-123";
        var queryEmbedding = new float[] { 1.0f };

        // Add 10 notes with high similarity
        for (int i = 0; i < 10; i++)
        {
            _context.Notes.Add(new Note
            {
                Id = Guid.NewGuid(),
                Title = $"Note {i}",
                Content = "Content",
                UserId = userId,
                Embedding = new float[] { 0.95f },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var service = new OpenAiService(Options.Create(_options), _context);
        var topN = 3;

        // When
        var result = await service.FindRelatedNotesAsync(queryEmbedding, userId, topN: topN);

        // Then
        result.Should().HaveCount(topN);
    }

    [Fact]
    public async Task FindRelatedNotesAsync_SkipsNotesWithoutEmbeddings()
    {
        // Given
        var userId = "user-123";
        var queryEmbedding = new float[] { 1.0f };

        var noteWithEmbedding = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note with embedding",
            Content = "Content",
            UserId = userId,
            Embedding = new float[] { 0.9f },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var noteWithoutEmbedding = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note without embedding",
            Content = "Content",
            UserId = userId,
            Embedding = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notes.AddRange(noteWithEmbedding, noteWithoutEmbedding);
        await _context.SaveChangesAsync();

        var service = new OpenAiService(Options.Create(_options), _context);

        // When
        var result = await service.FindRelatedNotesAsync(queryEmbedding, userId);

        // Then
        result.Should().Contain(noteWithEmbedding.Id);
        result.Should().NotContain(noteWithoutEmbedding.Id);
    }
}
