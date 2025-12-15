// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SeedNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.SeedNotes;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
///   Unit tests for SeedNotesHandler
/// </summary>
[ExcludeFromCodeCoverage]
public class SeedNotesHandlerTests
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	private readonly SeedNotesHandler _handler;

	public SeedNotesHandlerTests()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
				.Options;

		_context = new ApplicationDbContext(options);
		_aiService = Substitute.For<IAiService>();
		_handler = new SeedNotesHandler(_context, _aiService);
	}

	[Fact]
	public async Task Handle_WithValidUserId_CreatesMultipleNotes()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 10 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1,tag2");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CreatedCount.Should().Be(10);
		var notes = await _context.Notes.Where(n => n.OwnerSubject == ownerSubject).ToListAsync();
		notes.Should().HaveCount(10);
		notes.Should().AllSatisfy(n => n.AiSummary.Should().Be("Test summary"));
	}

	[Fact]
	public async Task Handle_WhenAiServiceFails_ContinuesWithoutAiEnhancements()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 3 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException<string>(new Exception("AI service unavailable")));

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException<string>(new Exception("AI service unavailable")));

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException<float[]>(new Exception("AI service unavailable")));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CreatedCount.Should().Be(0, "should fail gracefully when AI service throws");
		result.Errors.Should().HaveCountGreaterThan(0);
		result.Errors.Should().AllSatisfy(e => e.Should().Contain("AI service unavailable"));
	}

	[Fact]
	public async Task Handle_CreatesNotesWithDiverseTopics()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 15 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		var notes = await _context.Notes.Where(n => n.OwnerSubject == ownerSubject).ToListAsync();
		var uniqueTitles = notes.Select(n => n.Title).Distinct().Count();
		uniqueTitles.Should().Be(notes.Count, "each note should have a unique title");
	}

	[Fact]
	public async Task Handle_RespectCountParameter()
	{
		// Arrange
		var ownerSubject = "test-user";
		var requestedCount = 5;
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = requestedCount };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CreatedCount.Should().Be(requestedCount);
		result.CreatedNoteIds.Should().HaveCount(requestedCount);
	}

	[Fact]
	public async Task Handle_SetsCreatedDateInPast()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 5 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		var notes = await _context.Notes.Where(n => n.OwnerSubject == ownerSubject).ToListAsync();

		notes.Should().AllSatisfy(n =>
		{
			n.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
			n.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
		});
	}

	[Fact]
	public async Task Handle_CallsAllAiServiceMethods()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 2 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _aiService.Received(2).GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(2).GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _aiService.Received(2).GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithDefaultCount_Creates50Notes()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject }; // Uses default Count = 50

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CreatedCount.Should().Be(50);
	}

	[Fact]
	public async Task Handle_SavesNotesInBatches()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 12 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CreatedCount.Should().Be(12);

		// Verify all notes were saved
		var notes = await _context.Notes.Where(n => n.OwnerSubject == ownerSubject).ToListAsync();
		notes.Should().HaveCount(12);
	}

	[Fact]
	public async Task Handle_ReturnsCreatedNoteIds()
	{
		// Arrange
		var ownerSubject = "test-user";
		var command = new SeedNotesCommand { UserSubject = ownerSubject, Count = 3 };

		_aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("tag1");

		_aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([ 0.1f, 0.2f, 0.3f ]);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CreatedNoteIds.Should().HaveCount(3);
		result.CreatedNoteIds.Should().AllSatisfy(id => id.Should().NotBeEmpty());

		// Verify all IDs exist in a database
		foreach (var id in result.CreatedNoteIds)
		{
			var note = await _context.Notes.FindAsync(id);
			note.Should().NotBeNull();
		}
	}

}