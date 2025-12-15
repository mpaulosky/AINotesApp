// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateNoteHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.CreateNote;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
///   Unit tests for CreateNoteHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateNoteHandlerTests
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	private readonly CreateNoteHandler _handler;

	public CreateNoteHandlerTests()
	{
		// Set up an in-memory database
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new ApplicationDbContext(options);
		_aiService = Substitute.For<IAiService>();
		_handler = new CreateNoteHandler(_context, _aiService);
	}

	[Fact]
	public async Task Handle_ValidCommand_CreatesNoteWithAiContent()
	{
		// Given
		var userSubject = "test-user-123";

		var command = new CreateNoteCommand
		{
				Title = "Test Note",
				Content = "This is test content for the note.",
				UserSubject = userSubject
		};

		var expectedSummary = "Test summary";
		var expectedTags = "test, note, content";
		var expectedEmbedding = new [] { 0.1f, 0.2f, 0.3f };

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
		result.Title.Should().Be(command.Title);
		result.Content.Should().Be(command.Content);
		result.AiSummary.Should().Be(expectedSummary);
		result.Tags.Should().Be(expectedTags);
		result.Id.Should().NotBeEmpty();
		result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

		var noteInDb = await _context.Notes.FirstOrDefaultAsync(n => n.Id == result.Id);
		noteInDb.Should().NotBeNull();
		noteInDb.Title.Should().Be(command.Title);
		noteInDb.Content.Should().Be(command.Content);
		noteInDb.OwnerSubject.Should().Be(userSubject);
		noteInDb.AiSummary.Should().Be(expectedSummary);
		noteInDb.Tags.Should().Be(expectedTags);
		noteInDb.Embedding.Should().BeEquivalentTo(expectedEmbedding);
	}

	[Fact]
	public async Task Handle_ValidCommand_CallsAiServicesInParallel()
	{
		// Given
		var command = new CreateNoteCommand
		{
				Title = "Test Note",
				Content = "Test content",
				UserSubject = "user-123"
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1, tag2");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f ]);

		// When
		await _handler.Handle(command, CancellationToken.None);

		// Then
		await _aiService.Received(1).GenerateSummaryAsync(command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateTagsAsync(command.Title, command.Content, Arg.Any<CancellationToken>());
		await _aiService.Received(1).GenerateEmbeddingAsync(command.Content, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyContent_StillCreatesNote()
	{
		// Given
		var command = new CreateNoteCommand
		{
				Title = "Empty Note",
				Content = "",
				UserSubject = "user-123"
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(string.Empty);

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(string.Empty);

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([]);

		// When
		var result = await _handler.Handle(command, CancellationToken.None);

		// Then
		result.Should().NotBeNull();
		result.Title.Should().Be(command.Title);
		result.Content.Should().BeEmpty();
		result.Id.Should().NotBeEmpty();
	}

	[Theory]
	[InlineData("", "Content", "user-123")]
	[InlineData("Title", "", "user-123")]
	[InlineData("Title", "Content", "")]
	public async Task Handle_VariousInputs_CreatesNoteSuccessfully(string title, string content, string userSubject)
	{
		// Given
		var command = new CreateNoteCommand
		{
				Title = title,
				Content = content,
				UserSubject = userSubject
		};

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tags");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f ]);

		// When
		var result = await _handler.Handle(command, CancellationToken.None);

		// Then
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
	}

}