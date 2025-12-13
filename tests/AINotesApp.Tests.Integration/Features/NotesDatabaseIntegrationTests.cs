using AINotesApp.Data;
using AINotesApp.Tests.Integration.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Integration.Features;

/// <summary>
/// Integration tests for Note database operations.
/// </summary>
public class NotesDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public NotesDatabaseIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateNote_SavesSuccessfully_ToDatabase()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Integration Test Note",
            Content = "Content for integration test",
            UserId = "test-user-123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // When
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Then
        var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        savedNote.Should().NotBeNull();
        savedNote!.Title.Should().Be("Integration Test Note");
        savedNote.Content.Should().Be("Content for integration test");
        savedNote.UserId.Should().Be("test-user-123");
    }

    [Fact]
    public async Task UpdateNote_UpdatesSuccessfully_InDatabase()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Content = "Original Content",
            UserId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Detach to simulate a new request
        context.Entry(note).State = EntityState.Detached;

        // When
        var noteToUpdate = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        noteToUpdate!.Title = "Updated Title";
        noteToUpdate.Content = "Updated Content";
        noteToUpdate.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Then
        var updatedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        updatedNote.Should().NotBeNull();
        updatedNote!.Title.Should().Be("Updated Title");
        updatedNote.Content.Should().Be("Updated Content");
    }

    [Fact]
    public async Task DeleteNote_RemovesSuccessfully_FromDatabase()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note to Delete",
            Content = "Content",
            UserId = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // When
        context.Notes.Remove(note);
        await context.SaveChangesAsync();

        // Then
        var deletedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        deletedNote.Should().BeNull();
    }

    [Fact]
    public async Task QueryNotes_ByUserId_ReturnsOnlyUserNotes()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var userId1 = "user-1";
        var userId2 = "user-2";

        context.Notes.AddRange(
            new Note { Id = Guid.NewGuid(), Title = "User 1 Note 1", UserId = userId1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { Id = Guid.NewGuid(), Title = "User 1 Note 2", UserId = userId1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { Id = Guid.NewGuid(), Title = "User 2 Note 1", UserId = userId2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // When
        var user1Notes = await context.Notes
            .Where(n => n.UserId == userId1)
            .ToListAsync();

        // Then
        user1Notes.Should().HaveCount(2);
        user1Notes.Should().AllSatisfy(n => n.UserId.Should().Be(userId1));
    }

    [Fact]
    public async Task Note_WithEmbedding_StoresAndRetrievesCorrectly()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var embedding = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note with Embedding",
            Content = "Content",
            UserId = "test-user",
            Embedding = embedding,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // When
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Then
        var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        savedNote.Should().NotBeNull();
        savedNote!.Embedding.Should().NotBeNull();
        savedNote.Embedding.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public async Task Note_WithNullEmbedding_StoresCorrectly()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note without Embedding",
            Content = "Content",
            UserId = "test-user",
            Embedding = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // When
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Then
        var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        savedNote.Should().NotBeNull();
        savedNote!.Embedding.Should().BeNull();
    }

    [Fact]
    public async Task Note_WithAiSummaryAndTags_StoresCorrectly()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note with AI content",
            Content = "Content",
            UserId = "test-user",
            AiSummary = "This is an AI-generated summary",
            Tags = "ai, test, integration",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // When
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Then
        var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        savedNote.Should().NotBeNull();
        savedNote!.AiSummary.Should().Be("This is an AI-generated summary");
        savedNote.Tags.Should().Be("ai, test, integration");
    }

    [Fact]
    public async Task QueryNotes_OrderedByUpdatedAt_ReturnsCorrectOrder()
    {
        // Given
        using var context = _fixture.CreateNewContext();
        var userId = "test-user";
        var now = DateTime.UtcNow;

        var note1 = new Note { Id = Guid.NewGuid(), Title = "Oldest", UserId = userId, CreatedAt = now, UpdatedAt = now.AddHours(-3) };
        var note2 = new Note { Id = Guid.NewGuid(), Title = "Middle", UserId = userId, CreatedAt = now, UpdatedAt = now.AddHours(-2) };
        var note3 = new Note { Id = Guid.NewGuid(), Title = "Newest", UserId = userId, CreatedAt = now, UpdatedAt = now.AddHours(-1) };

        context.Notes.AddRange(note1, note2, note3);
        await context.SaveChangesAsync();

        // When
        var notes = await context.Notes
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();

        // Then
        notes.Should().HaveCount(3);
        notes[0].Title.Should().Be("Newest");
        notes[1].Title.Should().Be("Middle");
        notes[2].Title.Should().Be("Oldest");
    }
}
