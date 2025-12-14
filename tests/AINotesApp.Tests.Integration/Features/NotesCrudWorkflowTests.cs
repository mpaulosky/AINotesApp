using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using AINotesApp.Features.Notes.CreateNote;
using AINotesApp.Features.Notes.DeleteNote;
using AINotesApp.Features.Notes.GetNoteDetails;
using AINotesApp.Features.Notes.UpdateNote;
using AINotesApp.Services.Ai;
using AINotesApp.Tests.Integration.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace AINotesApp.Tests.Integration.Features;

/// <summary>
/// Integration tests for complete CRUD workflow
/// </summary>
[ExcludeFromCodeCoverage]
public class NotesCrudWorkflowTests : IClassFixture<DatabaseFixture>
{
	private readonly DatabaseFixture _fixture;
	private const string TestUserId = "integration-test-user";

	public NotesCrudWorkflowTests(DatabaseFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task CompleteCrudWorkflow_CreateReadUpdateDelete_Succeeds()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();

		// Create
		var createHandler = new CreateNoteHandler(context, aiService);
		var createCommand = new CreateNoteCommand
		{
			Title = "Integration Test Note",
			Content = "Test content for integration",
			UserId = TestUserId
		};
		var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
		createResult.Should().NotBeNull();
		var noteId = createResult.Id;
		noteId.Should().NotBeEmpty();

		// Read
		var readHandler = new GetNoteDetailsHandler(context);
		var readQuery = new GetNoteDetailsQuery { Id = noteId, UserId = TestUserId };
		var readResult = await readHandler.Handle(readQuery, CancellationToken.None);
		readResult.Should().NotBeNull();
		readResult!.Title.Should().Be("Integration Test Note");
		readResult.Content.Should().Be("Test content for integration");

		// Update
		var updateHandler = new UpdateNoteHandler(context, aiService);
		var updateCommand = new UpdateNoteCommand
		{
			Id = noteId,
			Title = "Updated Title",
			Content = "Updated content",
			UserId = TestUserId
		};
		var updateResult = await updateHandler.Handle(updateCommand, CancellationToken.None);
		updateResult.Should().NotBeNull();
		updateResult.Title.Should().Be("Updated Title");
		updateResult.Content.Should().Be("Updated content");

		// Verify Update
		var verifyResult = await readHandler.Handle(readQuery, CancellationToken.None);
		verifyResult!.Title.Should().Be("Updated Title");
		verifyResult.Content.Should().Be("Updated content");
		verifyResult.UpdatedAt.Should().BeAfter(verifyResult.CreatedAt);

		// Delete
		var deleteHandler = new DeleteNoteHandler(context);
		var deleteCommand = new DeleteNoteCommand { Id = noteId, UserId = TestUserId };
		await deleteHandler.Handle(deleteCommand, CancellationToken.None);

		// Verify Deletion
		var deletedNote = await context.Notes.FindAsync(noteId);
		deletedNote.Should().BeNull();
	}

	[Fact]
	public async Task SequentialUpdates_LastWriteWins()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();

		// Create note
		var createHandler = new CreateNoteHandler(context, aiService);
		var createCommand = new CreateNoteCommand
		{
			Title = "Sequential Test",
			Content = "Initial content",
			UserId = TestUserId
		};
		var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
		var noteId = createResult.Id;

		// Perform sequential updates
		var updateHandler = new UpdateNoteHandler(context, aiService);

		await updateHandler.Handle(new UpdateNoteCommand
		{
			Id = noteId,
			Title = "Update 1",
			Content = "Content 1",
			UserId = TestUserId
		}, CancellationToken.None);

		await updateHandler.Handle(new UpdateNoteCommand
		{
			Id = noteId,
			Title = "Update 2",
			Content = "Content 2",
			UserId = TestUserId
		}, CancellationToken.None);

		// Verify final state
		var note = await context.Notes.FindAsync(noteId);

		note.Should().NotBeNull();
		note!.Title.Should().Be("Update 2");
		note.Content.Should().Be("Content 2");
	}

	[Fact]
	public async Task CreateMultipleNotes_AllPersisted_WithCorrectData()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();
		var createHandler = new CreateNoteHandler(context, aiService);

		// Act - Create 5 notes
		var noteIds = new List<Guid>();
		for (int i = 1; i <= 5; i++)
		{
			var command = new CreateNoteCommand
			{
				Title = $"Test Note {i}",
				Content = $"Content for note {i}",
				UserId = TestUserId
			};
			var result = await createHandler.Handle(command, CancellationToken.None);
			noteIds.Add(result.Id);
		}

		// Assert
		noteIds.Should().HaveCount(5);
		noteIds.Should().OnlyHaveUniqueItems();

		var allNotes = await context.Notes
				.Where(n => n.UserId == TestUserId)
				.OrderBy(n => n.Title)
				.ToListAsync();

		allNotes.Should().HaveCount(c => c >= 5);
		for (int i = 1; i <= 5; i++)
		{
			allNotes.Should().Contain(n => n.Title == $"Test Note {i}");
		}
	}

	[Fact]
	public async Task UpdateNote_ThatDoesNotExist_ReturnsNull()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();
		var updateHandler = new UpdateNoteHandler(context, aiService);
		var nonExistentId = Guid.NewGuid();

		// Act
		var result = await updateHandler.Handle(new UpdateNoteCommand
		{
			Id = nonExistentId,
			Title = "Updated Title",
			Content = "Updated Content",
			UserId = TestUserId
		}, CancellationToken.None);

		// Assert - Handler returns error response for non-existent note
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Message.Should().Contain("not found");
	}

	[Fact]
	public async Task DeleteNote_ThatDoesNotExist_DoesNotThrow()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var deleteHandler = new DeleteNoteHandler(context);
		var nonExistentId = Guid.NewGuid();

		// Act - Delete should be idempotent
		var act = async () => await deleteHandler.Handle(new DeleteNoteCommand
		{
			Id = nonExistentId,
			UserId = TestUserId
		}, CancellationToken.None);

		// Assert - Should not throw
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task ReadNote_WithWrongUserId_ReturnsNull()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();

		// Create note for one user
		var createHandler = new CreateNoteHandler(context, aiService);
		var createCommand = new CreateNoteCommand
		{
			Title = "User 1 Note",
			Content = "Content",
			UserId = "user-1"
		};
		var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
		var noteId = createResult.Id;

		// Try to read with different user
		var readHandler = new GetNoteDetailsHandler(context);
		var readQuery = new GetNoteDetailsQuery
		{
			Id = noteId,
			UserId = "user-2" // Different user
		};

		// Act
		var result = await readHandler.Handle(readQuery, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task UpdateNote_PreservesCreatedAt_UpdatesUpdatedAt()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();

		// Create note
		var createHandler = new CreateNoteHandler(context, aiService);
		var createCommand = new CreateNoteCommand
		{
			Title = "Test Note",
			Content = "Initial content",
			UserId = TestUserId
		};
		var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
		var noteId = createResult.Id;
		var originalCreatedAt = createResult.CreatedAt;

		// Wait a bit to ensure UpdatedAt will be different
		await Task.Delay(100);

		// Update note
		var updateHandler = new UpdateNoteHandler(context, aiService);
		var updateCommand = new UpdateNoteCommand
		{
			Id = noteId,
			Title = "Updated Title",
			Content = "Updated content",
			UserId = TestUserId
		};
		var updateResult = await updateHandler.Handle(updateCommand, CancellationToken.None);

		// Assert
		var note = await context.Notes.FindAsync(noteId);
		note!.CreatedAt.Should().Be(originalCreatedAt);
		note.UpdatedAt.Should().BeAfter(note.CreatedAt);
	}

	[Fact]
	public async Task CompleteWorkflow_WithAiEnhancements_StoresAllData()
	{
		// Arrange
		using var context = _fixture.CreateNewContext();
		var aiService = CreateMockAiService();

		// Create note with AI enhancements
		var createHandler = new CreateNoteHandler(context, aiService);
		var createCommand = new CreateNoteCommand
		{
			Title = "AI Enhanced Note",
			Content = "This is a test note that will be enhanced by AI services.",
			UserId = TestUserId
		};
		var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
		var noteId = createResult.Id;

		// Verify AI data was stored
		var readHandler = new GetNoteDetailsHandler(context);
		var readQuery = new GetNoteDetailsQuery { Id = noteId, UserId = TestUserId };
		var readResult = await readHandler.Handle(readQuery, CancellationToken.None);

		readResult.Should().NotBeNull();
		readResult!.AiSummary.Should().Be("Test summary");
		readResult.Tags.Should().Be("test,tag");

		// Verify embedding is stored
		var note = await context.Notes.FindAsync(noteId);
		note!.Embedding.Should().NotBeNull();
		note.Embedding.Should().HaveCountGreaterThan(0);
	}

	private static IAiService CreateMockAiService()
	{
		var aiService = Substitute.For<IAiService>();
		aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");
		aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("test,tag");
		aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
		return aiService;
	}
}
