// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NotesDatabaseIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Tests.Integration.Database;
using AINotesApp.Tests.Integration.Helpers;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Integration.Features;

/// <summary>
///   Integration tests for Note database operations.
/// </summary>
[ExcludeFromCodeCoverage]
public class NotesDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{

	private readonly DatabaseFixture _fixture;

	public NotesDatabaseIntegrationTests(DatabaseFixture fixture)
	{
		_fixture = fixture;
	}

    /// <summary>
    ///   Verifies that a note can be created and saved to the database successfully.
    /// </summary>
    [Fact]
	public async Task CreateNote_SavesSuccessfully_ToDatabase()
	{
		// Given
		await using var context = _fixture.CreateNewContext();

		var note = NoteTestDataBuilder.CreateDefault()
			.WithTitle("Integration Test Note")
			.WithContent("Content for integration test")
			.WithOwnerSubject("test-user-123")
			.Build();

		// When
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Then
		var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
		savedNote.Should().NotBeNull();
		savedNote.Title.Should().Be("Integration Test Note");
		savedNote.Content.Should().Be("Content for integration test");
		savedNote.OwnerSubject.Should().Be("test-user-123");
	}

    /// <summary>
    ///   Verifies that updating a note works correctly in the database.
    /// </summary>
    [Fact]
	public async Task UpdateNote_UpdatesSuccessfully_InDatabase()
	{
		// Given
		await using var context = _fixture.CreateNewContext();

		var note = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Original Title")
				.WithContent("Original Content")
				.Build();

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
		updatedNote.Title.Should().Be("Updated Title");
		updatedNote.Content.Should().Be("Updated Content");
	}

    /// <summary>
    ///   Verifies that a note can be deleted from the database.
    /// </summary>
    [Fact]
	public async Task DeleteNote_RemovesSuccessfully_FromDatabase()
	{
		// Given
		await using var context = _fixture.CreateNewContext();

		var note = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Note to Delete")
				.WithContent("Content")
				.Build();

		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// When
		context.Notes.Remove(note);
		await context.SaveChangesAsync();

		// Then
		var deletedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
		deletedNote.Should().BeNull();
	}

    /// <summary>
    ///   Verifies that querying notes by user ID returns only the notes for that user.
    /// </summary>
    [Fact]
	public async Task QueryNotes_ByUserId_ReturnsOnlyUserNotes()
	{
		// Given
		await using var context = _fixture.CreateNewContext();
		var ownerSubject1 = "user-1";
		var ownerSubject2 = "user-2";

		context.Notes.AddRange(
				NoteTestDataBuilder.CreateDefault().WithTitle("User 1 Note 1").WithOwnerSubject(ownerSubject1).Build(),
				NoteTestDataBuilder.CreateDefault().WithTitle("User 1 Note 2").WithOwnerSubject(ownerSubject1).Build(),
				NoteTestDataBuilder.CreateDefault().WithTitle("User 2 Note 1").WithOwnerSubject(ownerSubject2).Build()
		);

		await context.SaveChangesAsync();

		// When
		var user1Notes = await context.Notes
				.Where(n => n.OwnerSubject == ownerSubject1)
				.ToListAsync();

		// Then
		user1Notes.Should().HaveCount(2);
		user1Notes.Should().AllSatisfy(n => n.OwnerSubject.Should().Be(ownerSubject1));
	}

    /// <summary>
    ///   Verifies that a note with an embedding can be stored and retrieved correctly.
    /// </summary>
    [Fact]
	public async Task Note_WithEmbedding_StoresAndRetrievesCorrectly()
	{
		// Given
		await using var context = _fixture.CreateNewContext();
		var embedding = new [] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

		var note = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Note with Embedding")
				.WithContent("Content")
				.WithEmbedding(embedding)
				.Build();

		// When
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Then
		var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
		savedNote.Should().NotBeNull();
		savedNote.Embedding.Should().NotBeNull();
		savedNote.Embedding.Should().BeEquivalentTo(embedding);
	}

    /// <summary>
    ///   Verifies that a note with a null embedding can be stored correctly.
    /// </summary>
    [Fact]
	public async Task Note_WithNullEmbedding_StoresCorrectly()
	{
		// Given
		await using var context = _fixture.CreateNewContext();

		var note = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Note without Embedding")
				.WithContent("Content")
				.Build();

		// When
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Then
		var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
		savedNote.Should().NotBeNull();
		savedNote.Embedding.Should().BeNull();
	}

    /// <summary>
    ///   Verifies that a note with AI-generated summary and tags can be stored correctly.
    /// </summary>
    [Fact]
	public async Task Note_WithAiSummaryAndTags_StoresCorrectly()
	{
		// Given
		await using var context = _fixture.CreateNewContext();

		var note = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Note with AI content")
				.WithContent("Content")
				.WithAiSummary("This is an AI-generated summary")
				.WithTags("ai, test, integration")
				.Build();

		// When
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Then
		var savedNote = await context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
		savedNote.Should().NotBeNull();
		savedNote.AiSummary.Should().Be("This is an AI-generated summary");
		savedNote.Tags.Should().Be("ai, test, integration");
	}

    /// <summary>
    ///   Verifies that notes are returned in the correct order when queried by updated date.
    /// </summary>
    [Fact]
	public async Task QueryNotes_OrderedByUpdatedAt_ReturnsCorrectOrder()
	{
		// Given
		await using var context = _fixture.CreateNewContext();
		var ownerSubject = "test-user";
		var now = DateTime.UtcNow;

		var note1 = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Oldest")
				.WithOwnerSubject(ownerSubject)
				.WithCreatedAt(now)
				.WithUpdatedAt(now.AddHours(-3))
				.Build();

		var note2 = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Middle")
				.WithOwnerSubject(ownerSubject)
				.WithCreatedAt(now)
				.WithUpdatedAt(now.AddHours(-2))
				.Build();

		var note3 = NoteTestDataBuilder.CreateDefault()
				.WithTitle("Newest")
				.WithOwnerSubject(ownerSubject)
				.WithCreatedAt(now)
				.WithUpdatedAt(now.AddHours(-1))
				.Build();

		context.Notes.AddRange(note1, note2, note3);
		await context.SaveChangesAsync();

		// When
		var notes = await context.Notes
				.Where(n => n.OwnerSubject == ownerSubject)
				.OrderByDescending(n => n.UpdatedAt)
				.ToListAsync();

		// Then
		notes.Should().HaveCount(3);
		notes[0].Title.Should().Be("Newest");
		notes[1].Title.Should().Be("Middle");
		notes[2].Title.Should().Be("Oldest");
	}

}