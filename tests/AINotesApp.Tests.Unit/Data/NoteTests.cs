using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using FluentAssertions;

namespace AINotesApp.Tests.Unit.Data;

/// <summary>
/// Unit tests for Note entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteTests
{
    /// <summary>
    /// Verifies that the default values of a new Note are set correctly.
    /// </summary>
    [Fact]
    public void Note_DefaultValues_AreSetCorrectly()
    {
        // Given & When
        var note = new Note();

        // Then
        note.Id.Should().BeEmpty();
        note.Title.Should().BeEmpty();
        note.Content.Should().BeEmpty();
        note.AiSummary.Should().BeNull();
        note.Tags.Should().BeNull();
        note.Embedding.Should().BeNull();
        note.UserId.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that setting properties on a Note works as expected.
    /// </summary>
    [Fact]
    public void Note_SetProperties_WorksCorrectly()
    {
        // Given
        var noteId = Guid.NewGuid();
        var userId = "user-123";
        var title = "Test Note";
        var content = "Test Content";
        var summary = "Test Summary";
        var tags = "test, note";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var createdAt = DateTime.UtcNow;

        // When
        var note = new Note
        {
            Id = noteId,
            Title = title,
            Content = content,
            AiSummary = summary,
            Tags = tags,
            Embedding = embedding,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        // Then
        note.Id.Should().Be(noteId);
        note.Title.Should().Be(title);
        note.Content.Should().Be(content);
        note.AiSummary.Should().Be(summary);
        note.Tags.Should().Be(tags);
        note.Embedding.Should().BeEquivalentTo(embedding);
        note.UserId.Should().Be(userId);
        note.CreatedAt.Should().Be(createdAt);
        note.UpdatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Note_EmbeddingCanBeNull_ForNewNotes()
    {
        // Given & When
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "New Note",
            Content = "Content",
            UserId = "user-123",
            Embedding = null
        };

        // Then
        note.Embedding.Should().BeNull();
    }

    [Fact]
    public void Note_EmbeddingCanBeEmptyArray()
    {
        // Given & When
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note",
            Content = "Content",
            UserId = "user-123",
            Embedding = Array.Empty<float>()
        };

        // Then
        note.Embedding.Should().NotBeNull();
        note.Embedding.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Content")]
    [InlineData("Title", "")]
    [InlineData("", "")]
    public void Note_AllowsEmptyTitleOrContent(string title, string content)
    {
        // Given & When
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            UserId = "user-123"
        };

        // Then
        note.Title.Should().Be(title);
        note.Content.Should().Be(content);
    }

    [Fact]
    public void Note_AiSummaryAndTagsCanBeNull()
    {
        // Given & When
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Note",
            Content = "Content",
            UserId = "user-123",
            AiSummary = null,
            Tags = null
        };

        // Then
        note.AiSummary.Should().BeNull();
        note.Tags.Should().BeNull();
    }
}