// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Data;

/// <summary>
///   Unit tests for Note entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteTests
{

    /// <summary>
    ///   Verifies that the default values of a new Note are set correctly.
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
		note.OwnerSubject.Should().BeEmpty();
	}

    /// <summary>
    ///   Verifies that setting properties on a Note works as expected.
    /// </summary>
    [Fact]
	public void Note_SetProperties_WorksCorrectly()
	{
		// Given
		var noteId = Guid.NewGuid();
		var ownerSubject = "user-123";
		var title = "Test Note";
		var content = "Test Content";
		var summary = "Test Summary";
		var tags = "test, note";
		var embedding = new [] { 0.1f, 0.2f, 0.3f };
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
				OwnerSubject = ownerSubject,
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
		note.OwnerSubject.Should().Be(ownerSubject);
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
				OwnerSubject = "user-123",
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
				OwnerSubject = "user-123",
				Embedding = []
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
				OwnerSubject = "user-123"
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
				OwnerSubject = "user-123",
				AiSummary = null,
				Tags = null
		};

		// Then
		note.AiSummary.Should().BeNull();
		note.Tags.Should().BeNull();
	}

	[Fact]
	public void Note_Owner_CanBeNull()
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Owner = null
		};

		// Then
		note.Owner.Should().BeNull();
	}

	[Fact]
	public void Note_Owner_CanBeSet()
	{
		// Given
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123456789",
			Name = "John Doe",
			Email = "john.doe@example.com"
		};

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = appUser.Auth0Subject,
			Owner = appUser
		};

		// Then
		note.Owner.Should().NotBeNull();
		note.Owner.Should().BeSameAs(appUser);
		note.OwnerSubject.Should().Be(appUser.Auth0Subject);
	}

	[Fact]
	public void Note_CreatedAt_CanBeSet()
	{
		// Given
		var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			CreatedAt = createdAt
		};

		// Then
		note.CreatedAt.Should().Be(createdAt);
	}

	[Fact]
	public void Note_UpdatedAt_CanBeSet()
	{
		// Given
		var updatedAt = new DateTime(2024, 1, 20, 15, 45, 0, DateTimeKind.Utc);

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			UpdatedAt = updatedAt
		};

		// Then
		note.UpdatedAt.Should().Be(updatedAt);
	}

	[Fact]
	public void Note_UpdatedAt_CanBeDifferentFromCreatedAt()
	{
		// Given
		var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
		var updatedAt = new DateTime(2024, 1, 20, 15, 45, 0, DateTimeKind.Utc);

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			CreatedAt = createdAt,
			UpdatedAt = updatedAt
		};

		// Then
		note.CreatedAt.Should().Be(createdAt);
		note.UpdatedAt.Should().Be(updatedAt);
		note.UpdatedAt.Should().BeAfter(note.CreatedAt);
	}

	[Fact]
	public void Note_PropertiesCanBeModified_AfterInitialization()
	{
		// Given
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Original Title",
			Content = "Original Content",
			OwnerSubject = "user-123"
		};

		// When
		note.Title = "Modified Title";
		note.Content = "Modified Content";
		note.AiSummary = "AI Generated Summary";
		note.Tags = "updated, modified";

		// Then
		note.Title.Should().Be("Modified Title");
		note.Content.Should().Be("Modified Content");
		note.AiSummary.Should().Be("AI Generated Summary");
		note.Tags.Should().Be("updated, modified");
	}

	[Fact]
	public void Note_EmbeddingCanBeModified_AfterInitialization()
	{
		// Given
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Embedding = new[] { 0.1f, 0.2f, 0.3f }
		};

		// When
		note.Embedding = new[] { 0.4f, 0.5f, 0.6f, 0.7f };

		// Then
		note.Embedding.Should().NotBeNull();
		note.Embedding.Should().HaveCount(4);
		note.Embedding.Should().BeEquivalentTo(new[] { 0.4f, 0.5f, 0.6f, 0.7f });
	}

	[Fact]
	public void Note_Embedding_CanContainNegativeValues()
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Embedding = new[] { -0.5f, 0.0f, 0.5f, -1.0f, 1.0f }
		};

		// Then
		note.Embedding.Should().NotBeNull();
		note.Embedding.Should().Contain(-0.5f);
		note.Embedding.Should().Contain(-1.0f);
	}

	[Fact]
	public void Note_Embedding_CanContainZeroValues()
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Embedding = new[] { 0.0f, 0.0f, 0.0f }
		};

		// Then
		note.Embedding.Should().NotBeNull();
		note.Embedding.Should().AllSatisfy(v => v.Should().Be(0.0f));
	}

	[Fact]
	public void Note_Embedding_CanBeLargeArray()
	{
		// Given
		var largeEmbedding = new float[1536]; // Common dimension for OpenAI embeddings
		for (var i = 0; i < largeEmbedding.Length; i++)
		{
			largeEmbedding[i] = (float)i / 1536;
		}

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Embedding = largeEmbedding
		};

		// Then
		note.Embedding.Should().NotBeNull();
		note.Embedding.Should().HaveCount(1536);
	}

	[Theory]
	[InlineData("tag1")]
	[InlineData("tag1, tag2")]
	[InlineData("tag1, tag2, tag3")]
	[InlineData("important, work, project")]
	public void Note_Tags_AcceptsVariousFormats(string tags)
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Tags = tags
		};

		// Then
		note.Tags.Should().Be(tags);
	}

	[Fact]
	public void Note_Tags_CanBeEmptyString()
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			Tags = string.Empty
		};

		// Then
		note.Tags.Should().NotBeNull();
		note.Tags.Should().BeEmpty();
	}

	[Fact]
	public void Note_AiSummary_CanBeEmptyString()
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			AiSummary = string.Empty
		};

		// Then
		note.AiSummary.Should().NotBeNull();
		note.AiSummary.Should().BeEmpty();
	}

	[Fact]
	public void Note_AiSummary_CanContainLongText()
	{
		// Given
		var longSummary = string.Join(" ", Enumerable.Repeat("This is a long AI summary.", 100));

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123",
			AiSummary = longSummary
		};

		// Then
		note.AiSummary.Should().NotBeNull();
		note.AiSummary.Should().HaveLength(longSummary.Length);
	}

	[Fact]
	public void Note_WithAllProperties_MaintainsValues()
	{
		// Given
		var noteId = Guid.NewGuid();
		var title = "Complete Note";
		var content = "Complete Content";
		var aiSummary = "AI Summary";
		var tags = "complete, test, note";
		var embedding = new[] { 0.1f, 0.2f, 0.3f };
		var ownerSubject = "auth0|123456789";
		var createdAt = DateTime.UtcNow.AddDays(-1);
		var updatedAt = DateTime.UtcNow;
		var owner = new AppUser
		{
			Auth0Subject = ownerSubject,
			Name = "Test User",
			Email = "test@example.com"
		};

		// When
		var note = new Note
		{
			Id = noteId,
			Title = title,
			Content = content,
			AiSummary = aiSummary,
			Tags = tags,
			Embedding = embedding,
			OwnerSubject = ownerSubject,
			CreatedAt = createdAt,
			UpdatedAt = updatedAt,
			Owner = owner
		};

		// Then
		note.Id.Should().Be(noteId);
		note.Title.Should().Be(title);
		note.Content.Should().Be(content);
		note.AiSummary.Should().Be(aiSummary);
		note.Tags.Should().Be(tags);
		note.Embedding.Should().BeEquivalentTo(embedding);
		note.OwnerSubject.Should().Be(ownerSubject);
		note.CreatedAt.Should().Be(createdAt);
		note.UpdatedAt.Should().Be(updatedAt);
		note.Owner.Should().BeSameAs(owner);
	}

	[Fact]
	public void Note_OwnerSubject_CanBeModified()
	{
		// Given
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123"
		};

		// When
		note.OwnerSubject = "user-456";

		// Then
		note.OwnerSubject.Should().Be("user-456");
	}

	[Fact]
	public void Note_Id_CanBeModified()
	{
		// Given
		var originalId = Guid.NewGuid();
		var note = new Note
		{
			Id = originalId,
			Title = "Note",
			Content = "Content",
			OwnerSubject = "user-123"
		};

		// When
		var newId = Guid.NewGuid();
		note.Id = newId;

		// Then
		note.Id.Should().Be(newId);
		note.Id.Should().NotBe(originalId);
	}

	[Theory]
	[InlineData("auth0|123456789")]
	[InlineData("google-oauth2|123456789")]
	[InlineData("github|123456789")]
	[InlineData("custom-user-id")]
	public void Note_OwnerSubject_AcceptsValidFormats(string ownerSubject)
	{
		// Given & When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note",
			Content = "Content",
			OwnerSubject = ownerSubject
		};

		// Then
		note.OwnerSubject.Should().Be(ownerSubject);
	}

	[Fact]
	public void Note_Owner_NavigationProperty_DefaultsToNull()
	{
		// Given & When
		var note = new Note();

		// Then
		note.Owner.Should().BeNull();
	}

	[Fact]
	public void Note_TimestampsDefault_ToDefaultDateTime()
	{
		// Given & When
		var note = new Note();

		// Then
		note.CreatedAt.Should().Be(default(DateTime));
		note.UpdatedAt.Should().Be(default(DateTime));
	}

	[Fact]
	public void Note_Content_CanContainLongText()
	{
		// Given
		var longContent = string.Join("\n", Enumerable.Repeat("This is a very long note content paragraph.", 1000));

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Long Note",
			Content = longContent,
			OwnerSubject = "user-123"
		};

		// Then
		note.Content.Should().NotBeNull();
		note.Content.Should().HaveLength(longContent.Length);
	}

	[Fact]
	public void Note_Content_CanContainSpecialCharacters()
	{
		// Given
		var specialContent = "Special chars: @#$%^&*()_+-=[]{}|;:',.<>?/~`\n\t\"\\";

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Special Note",
			Content = specialContent,
			OwnerSubject = "user-123"
		};

		// Then
		note.Content.Should().Be(specialContent);
	}

	[Fact]
	public void Note_Title_CanContainSpecialCharacters()
	{
		// Given
		var specialTitle = "Note: 2024 Q1 Results (Important!)";

		// When
		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = specialTitle,
			Content = "Content",
			OwnerSubject = "user-123"
		};

		// Then
		note.Title.Should().Be(specialTitle);
	}

}