// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetNoteDetailsHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.GetNoteDetails;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
///   Unit tests for GetNoteDetailsHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetNoteDetailsHandlerTests
{

	private readonly ApplicationDbContext _context;

	private readonly GetNoteDetailsHandler _handler;

	public GetNoteDetailsHandlerTests()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new ApplicationDbContext(options);
		_handler = new GetNoteDetailsHandler(_context);
	}

	[Fact]
	public async Task Handle_ReturnsNull_WhenNoteNotFound()
	{
		var query = new GetNoteDetailsQuery { Id = Guid.NewGuid(), UserSubject = "user-1" };

		var result = await _handler.Handle(query, CancellationToken.None);

		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_ReturnsDetails_WhenNoteExistsForUser()
	{
		var ownerSubject = "user-1";

		var note = new Note
		{
				Id = Guid.NewGuid(),
				Title = "Title",
				Content = "Content",
				AiSummary = "Summary",
				Tags = "tag1,tag2",
				OwnerSubject = ownerSubject,
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.Add(note);
		await _context.SaveChangesAsync();

		var query = new GetNoteDetailsQuery { Id = note.Id, UserSubject = ownerSubject };

		var result = await _handler.Handle(query, CancellationToken.None);

		result.Should().NotBeNull();
		result.Id.Should().Be(note.Id);
		result.Title.Should().Be("Title");
		result.Content.Should().Be("Content");
		result.AiSummary.Should().Be("Summary");
		result.Tags.Should().Be("tag1,tag2");
	}

}