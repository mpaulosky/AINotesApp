using AINotesApp.Data;
using AINotesApp.Features.Notes.UpdateNote;
using AINotesApp.Services.Ai;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
/// Unit tests for UpdateNoteHandler.
/// </summary>
public class UpdateNoteHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IAiService _aiService;
    private readonly UpdateNoteHandler _handler;

    public UpdateNoteHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _aiService = Substitute.For<IAiService>();
        _handler = new UpdateNoteHandler(_context, _aiService);
    }

    [Fact]
    public async Task Handle_ExistingNote_UpdatesSuccessfully()
    {
        // Given
        var userId = "test-user-123";
        var existingNote = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Content = "Original Content",
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        _context.Notes.Add(existingNote);
        await _context.SaveChangesAsync();

        var command = new UpdateNoteCommand
        {
            Id = existingNote.Id,
            Title = "Updated Title",
            Content = "Updated Content",
            UserId = userId
        };

        var expectedSummary = "Updated summary";
        var expectedTags = "updated, tags";
        var expectedEmbedding = new float[] { 0.5f, 0.6f };

        _aiService.GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>())
            .Returns(expectedSummary);
        _aiService.GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>())
            .Returns(expectedTags);
        _aiService.GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>())
            .Returns(expectedEmbedding);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Title.Should().Be(command.Title);
        result.Content.Should().Be(command.Content);
        result.AiSummary.Should().Be(expectedSummary);
        result.Tags.Should().Be(expectedTags);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var noteInDb = await _context.Notes.FirstOrDefaultAsync(n => n.Id == existingNote.Id);
        noteInDb.Should().NotBeNull();
        noteInDb!.Title.Should().Be(command.Title);
        noteInDb.Content.Should().Be(command.Content);
        noteInDb.Embedding.Should().BeEquivalentTo(expectedEmbedding);
    }

    [Fact]
    public async Task Handle_NonExistentNote_ReturnsFailure()
    {
        // Given
        var command = new UpdateNoteCommand
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Content = "Content",
            UserId = "user-123"
        };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WrongUserId_ReturnsFailure()
    {
        // Given
        var correctUserId = "correct-user";
        var wrongUserId = "wrong-user";
        
        var existingNote = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Content = "Original Content",
            UserId = correctUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Notes.Add(existingNote);
        await _context.SaveChangesAsync();

        var command = new UpdateNoteCommand
        {
            Id = existingNote.Id,
            Title = "Updated Title",
            Content = "Updated Content",
            UserId = wrongUserId
        };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("access denied");
    }

    [Fact]
    public async Task Handle_UpdateNote_RegeneratesAiContent()
    {
        // Given
        var userId = "user-123";
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Old",
            Content = "Old",
            UserId = userId,
            AiSummary = "Old summary",
            Tags = "old, tags",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        var command = new UpdateNoteCommand
        {
            Id = note.Id,
            Title = "New",
            Content = "New",
            UserId = userId
        };

        _aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("New summary");
        _aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("new, tags");
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.1f });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
        await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
        await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
    }
}
