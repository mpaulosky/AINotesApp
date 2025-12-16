// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DeleteNoteHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.DeleteNote;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Unit.Features.Notes;

/// <summary>
///   Unit tests for DeleteNoteHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteNoteHandlerTests
{

	private readonly ApplicationDbContext _context;

	private readonly DeleteNoteHandler _handler;

	public DeleteNoteHandlerTests()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new ApplicationDbContext(options);
		_handler = new DeleteNoteHandler(_context);
	}

	[Fact]
	public async Task Handle_ReturnsFailure_WhenNoteNotFound()
	{
		var cmd = new DeleteNoteCommand { Id = Guid.NewGuid(), UserSubject = "user-x" };

		var result = await _handler.Handle(cmd, CancellationToken.None);

		result.Success.Should().BeFalse();
		result.Message.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task Handle_DeletesNote_WhenOwnedByUser()
	{
		var ownerSubject = "user-1";

		var note = new Note
		{
				Id = Guid.NewGuid(),
				Title = "t",
				Content = "c",
				OwnerSubject = ownerSubject,
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.Add(note);
		await _context.SaveChangesAsync();

		var cmd = new DeleteNoteCommand { Id = note.Id, UserSubject = ownerSubject };
		var result = await _handler.Handle(cmd, CancellationToken.None);

		result.Success.Should().BeTrue();
		(await _context.Notes.FindAsync(note.Id)).Should().BeNull();
	}

	[Fact]
	public async Task Handle_ReturnsFailure_WhenNoteOwnedByDifferentUser()
	{
		var note = new Note
		{
				Id = Guid.NewGuid(),
				Title = "t",
				Content = "c",
				OwnerSubject = "owner",
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UpdatedAt = DateTime.UtcNow
		};

		_context.Notes.Add(note);
		await _context.SaveChangesAsync();

		var cmd = new DeleteNoteCommand { Id = note.Id, UserSubject = "other" };
		var result = await _handler.Handle(cmd, CancellationToken.None);

		result.Success.Should().BeFalse();
		(await _context.Notes.FindAsync(note.Id)).Should().NotBeNull();
	}

}