// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiServiceEdgeCasesTests.cs
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
///   Edge case and boundary tests for OpenAI service
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenAiServiceEdgeCasesTests
{

	private readonly ApplicationDbContext _context;

	private readonly AiServiceOptions _options;

	public OpenAiServiceEdgeCasesTests()
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
	public async Task FindRelatedNotesAsync_WithZeroEmbedding_ReturnsNoResults()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var zeroEmbedding = new float[1536]; // All zeros

		// Seed some notes
		await SeedTestNotesAsync();

		// Act
		var result = await service.FindRelatedNotesAsync(zeroEmbedding, "user-id");

		// Assert
		result.Should().BeEmpty("zero embeddings have no meaningful similarity");
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNullEmbedding_ReturnsEmpty()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);

		// Act
		var result = await service.FindRelatedNotesAsync(null!, "user-id");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithEmptyUserId_ReturnsEmpty()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = new [] { 0.1f, 0.2f, 0.3f };

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, string.Empty);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNegativeTopN_UsesDefault()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();
		await SeedTestNotesAsync();

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1", null, -5);

		// Assert - Should use default value instead of throwing
		result.Should().NotBeNull();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithVeryHighTopN_ReturnsAvailableNotes()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();
		await SeedTestNotesAsync(); // Seeds only 3 notes

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1", null, 100);

		// Assert - Should return only available notes (max 3)
		result.Should().HaveCount(c => c <= 3);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ExcludesCurrentNote()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();
		var noteIds = await SeedTestNotesAsync();
		var currentNoteId = noteIds[0];

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1", currentNoteId, 10);

		// Assert
		result.Should().NotContain(currentNoteId);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_OnlyReturnsUserNotes()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();

		// Seed notes for different users
		await SeedTestNotesForMultipleUsers();

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1", null, 10);

		// Assert
		result.Should().NotBeEmpty();
		var notes = await _context.Notes.Where(n => result.Contains(n.Id)).ToListAsync();
		notes.Should().AllSatisfy(n => n.OwnerSubject.Should().Be("user-1"));
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNoNotesInDatabase_ReturnsEmpty()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNotesHavingNullEmbeddings_SkipsThem()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();

		// Seed notes with null embeddings
		_context.Notes.Add(new Note
		{
				Id = Guid.NewGuid(),
				Title = "No Embedding",
				Content = "Content",
				OwnerSubject = "user-1",
				Embedding = null,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
		});

		await _context.SaveChangesAsync();

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1");

		// Assert
		result.Should().BeEmpty();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(5)]
	[InlineData(10)]
	public async Task FindRelatedNotesAsync_RespectsTopNParameter(int topN)
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var embedding = CreateNormalizedEmbedding();
		await SeedManyTestNotes(15); // Seed more notes than topN

		// Act
		var result = await service.FindRelatedNotesAsync(embedding, "user-1", null, topN);

		// Assert
		result.Should().HaveCount(c => c <= topN);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_OrdersBySimilarity()
	{
		// Arrange
		var service = new OpenAiService(Options.Create(_options), _context);
		var searchEmbedding = new [] { 1.0f, 0.0f, 0.0f };

		// Seed notes with different embeddings
		var note1Id = Guid.NewGuid();
		var note2Id = Guid.NewGuid();
		var note3Id = Guid.NewGuid();

		_context.Notes.AddRange(
				new Note
				{
						Id = note1Id,
						Title = "Most Similar",
						Content = "Content",
						OwnerSubject = "user-1",
						Embedding = [ 0.9f, 0.1f, 0.0f ], // More similar
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				},
				new Note
				{
						Id = note2Id,
						Title = "Less Similar",
						Content = "Content",
						OwnerSubject = "user-1",
						Embedding = [ 0.5f, 0.5f, 0.0f ], // Less similar
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				},
				new Note
				{
						Id = note3Id,
						Title = "Least Similar",
						Content = "Content",
						OwnerSubject = "user-1",
						Embedding = [ 0.0f, 1.0f, 0.0f ], // Least similar
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				}
		);

		await _context.SaveChangesAsync();

		// Act
		var result = await service.FindRelatedNotesAsync(searchEmbedding, "user-1", null, 3);

		// Assert
		result.Should().HaveCount(c => c >= 2, "at least 2 notes should be similar enough");
		result[0].Should().Be(note1Id, "most similar should be first");
	}

	// Helper methods

	private async Task<List<Guid>> SeedTestNotesAsync()
	{
		var noteIds = new List<Guid>();
		var embedding1 = CreateNormalizedEmbedding();
		var embedding2 = CreateNormalizedEmbedding(0.2f, 0.3f);
		var embedding3 = CreateNormalizedEmbedding(0.3f, 0.4f);

		var notes = new[]
		{
				new Note
				{
						Id = Guid.NewGuid(),
						Title = "Test Note 1",
						Content = "Content 1",
						OwnerSubject = "user-1",
						Embedding = embedding1,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				},
				new Note
				{
						Id = Guid.NewGuid(),
						Title = "Test Note 2",
						Content = "Content 2",
						OwnerSubject = "user-1",
						Embedding = embedding2,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				},
				new Note
				{
						Id = Guid.NewGuid(),
						Title = "Test Note 3",
						Content = "Content 3",
						OwnerSubject = "user-1",
						Embedding = embedding3,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				}
		};

		_context.Notes.AddRange(notes);
		await _context.SaveChangesAsync();

		return notes.Select(n => n.Id).ToList();
	}

	private async Task SeedTestNotesForMultipleUsers()
	{
		var embedding = CreateNormalizedEmbedding();

		_context.Notes.AddRange(
				new Note
				{
						Id = Guid.NewGuid(),
						Title = "User 1 Note",
						Content = "Content",
						OwnerSubject = "user-1",
						Embedding = embedding,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				},
				new Note
				{
						Id = Guid.NewGuid(),
						Title = "User 2 Note",
						Content = "Content",
						OwnerSubject = "user-2",
						Embedding = embedding,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
				}
		);

		await _context.SaveChangesAsync();
	}

	private async Task SeedManyTestNotes(int count)
	{
		var notes = new List<Note>();

		for (var i = 0; i < count; i++)
		{
			notes.Add(new Note
			{
					Id = Guid.NewGuid(),
					Title = $"Test Note {i}",
					Content = $"Content {i}",
					OwnerSubject = "user-1",
					Embedding = CreateNormalizedEmbedding(i * 0.01f, i * 0.02f),
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
			});
		}

		_context.Notes.AddRange(notes);
		await _context.SaveChangesAsync();
	}

	private static float[] CreateNormalizedEmbedding(float offset1 = 0.1f, float offset2 = 0.2f)
	{
		var embedding = new float[1536];

		for (var i = 0; i < embedding.Length; i++)
		{
			embedding[i] = (i % 2 == 0 ? offset1 : offset2) + i * 0.001f;
		}

		return embedding;
	}

}