// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Services;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AINotesApp.Tests.Unit.Services.Ai;

/// <summary>
///   Unit tests for OpenAiService - focusing on internal logic and edge cases.
///   Note: These tests don't make actual API calls to OpenAI.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenAiServiceTests
{

	private readonly ApplicationDbContext _context;

	private readonly AiServiceOptions _options;

	public OpenAiServiceTests()
	{
		var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
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
		var userSubject = "user-123";

		// When
		var result = await service.FindRelatedNotesAsync(emptyEmbedding, userSubject);

		// Then
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_NullEmbedding_ReturnsEmptyList()
	{
		// Given
		var service = new OpenAiService(Options.Create(_options), _context);
		float[]? nullEmbedding = null;
		var userSubject = "user-123";

		// When
		var result = await service.FindRelatedNotesAsync(nullEmbedding!, userSubject);

		// Then
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_NoNotesInDatabase_ReturnsEmptyList()
	{
		// Given
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = new [] { 0.1f, 0.2f, 0.3f };
		var userSubject = "user-123";

		// When
		var result = await service.FindRelatedNotesAsync(embedding, userSubject);

		// Then
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithMatchingNotes_ReturnsRelatedNotes()
	{
		// Given
		var userSubject = "user-123";
		var queryEmbedding = new [] { 1.0f, 0.0f, 0.0f };

		var note1 = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Similar Note",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = [ 0.9f, 0.1f, 0.0f ], // High similarity
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		var note2 = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Different Note",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = [ 0.0f, 1.0f, 0.0f ], // Low similarity
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.AddRange(note1, note2);
		await _context.SaveChangesAsync();

		var service = new OpenAiService(Options.Create(_options), _context);

		// When
		var result = await service.FindRelatedNotesAsync(queryEmbedding, userSubject);

		// Then
		result.Should().NotBeEmpty();
		result.Should().Contain(note1.Id);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ExcludesCurrentNote_WhenSpecified()
	{
		// Given
		var userSubject = "user-123";
		var currentNoteId = Guid.NewGuid();
		var queryEmbedding = new [] { 1.0f, 0.0f };

		var currentNote = new Note
		{
				Id = currentNoteId,
				Title = "Current Note",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = [ 1.0f, 0.0f ], // Perfect match
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		var otherNote = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Other Note",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = [ 0.9f, 0.1f ], // High similarity
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.AddRange(currentNote, otherNote);
		await _context.SaveChangesAsync();

		var service = new OpenAiService(Options.Create(_options), _context);

		// When
		var result = await service.FindRelatedNotesAsync(queryEmbedding, userSubject, currentNoteId);

		// Then
		result.Should().NotContain(currentNoteId);
		result.Should().Contain(otherNote.Id);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_OnlyReturnsUserNotes_NotOtherUsers()
	{
		// Given
		var ownerSubject1 = "user-1";
		var ownerSubject2 = "user-2";
		var queryEmbedding = new [] { 1.0f, 0.0f };

		var user1Note = new Note
		{
				Id = Guid.NewGuid(),
				Title = "User 1 Note",
				Content = "Content",
				OwnerSubject = ownerSubject1,
				Embedding = [ 1.0f, 0.0f ],
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		var user2Note = new Note
		{
				Id = Guid.NewGuid(),
				Title = "User 2 Note",
				Content = "Content",
				OwnerSubject = ownerSubject2,
				Embedding = [ 1.0f, 0.0f ],
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.AddRange(user1Note, user2Note);
		await _context.SaveChangesAsync();

		var service = new OpenAiService(Options.Create(_options), _context);

		// When
		var result = await service.FindRelatedNotesAsync(queryEmbedding, ownerSubject1);

		// Then
		result.Should().Contain(user1Note.Id);
		result.Should().NotContain(user2Note.Id);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_RespectsTopNParameter()
	{
		// Given
		var userSubject = "user-123";
		var queryEmbedding = new [] { 1.0f };

		// Add 10 notes with high similarity
		for (var i = 0; i < 10; i++)
		{
			_context.Notes.Add(new Note
			{
					Id = Guid.NewGuid(),
					Title = $"Note {i}",
					Content = "Content",
					OwnerSubject = userSubject,
					Embedding = [ 0.95f ],
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
			});
		}

		await _context.SaveChangesAsync();

		var service = new OpenAiService(Options.Create(_options), _context);
		var topN = 3;

		// When
		var result = await service.FindRelatedNotesAsync(queryEmbedding, userSubject, topN: topN);

		// Then
		result.Should().HaveCount(topN);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_SkipsNotesWithoutEmbeddings()
	{
		// Given
		var userSubject = "user-123";
		var queryEmbedding = new [] { 1.0f };

		var noteWithEmbedding = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Note with embedding",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = [ 0.9f ],
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		var noteWithoutEmbedding = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Note without embedding",
				Content = "Content",
				OwnerSubject = userSubject,
				Embedding = null,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.AddRange(noteWithEmbedding, noteWithoutEmbedding);
		await _context.SaveChangesAsync();

		var service = new OpenAiService(Options.Create(_options), _context);

		// When
		var result = await service.FindRelatedNotesAsync(queryEmbedding, userSubject);

		// Then
		result.Should().Contain(noteWithEmbedding.Id);
		result.Should().NotContain(noteWithoutEmbedding.Id);
	}

}