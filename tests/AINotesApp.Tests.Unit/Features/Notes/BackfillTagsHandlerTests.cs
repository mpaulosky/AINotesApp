// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BackfillTagsHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.BackfillTags;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Features.Notes;

[ExcludeFromCodeCoverage]
public class BackfillTagsHandlerTests
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	private readonly BackfillTagsHandler _handler;

	public BackfillTagsHandlerTests()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new ApplicationDbContext(options);
		_aiService = Substitute.For<IAiService>();
		_handler = new BackfillTagsHandler(_context, _aiService);
	}

	[Fact]
	public async Task Processes_Only_Notes_Without_Tags_When_OnlyMissing_True()
	{
		// Arrange
		var ownerSubject = "user1";

		var note1 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 1", Content = "Content 1", Tags = null };

		var note2 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 2", Content = "Content 2", Tags = "tagged" };

		_context.Notes.AddRange(note1, note2);
		await _context.SaveChangesAsync();

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(ci => "generated-tags");

		var command = new BackfillTagsCommand { UserSubject = ownerSubject, OnlyMissing = true };

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.ProcessedCount.Should().Be(1);
		result.TotalNotes.Should().Be(1);
		result.Errors.Should().BeEmpty();
		(await _context.Notes.FindAsync(note1.Id))!.Tags.Should().Be("generated-tags");
		(await _context.Notes.FindAsync(note2.Id))!.Tags.Should().Be("tagged");
	}

	[Fact]
	public async Task Processes_All_Notes_When_OnlyMissing_False()
	{
		// Arrange
		var ownerSubject = "user2";

		var note3 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 3", Content = "Content 3", Tags = null };

		var note4 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 4", Content = "Content 4", Tags = "old" };

		_context.Notes.AddRange(note3, note4);
		await _context.SaveChangesAsync();

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(ci => "new-tags");

		var command = new BackfillTagsCommand { UserSubject = ownerSubject, OnlyMissing = false };

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.ProcessedCount.Should().Be(2);
		result.TotalNotes.Should().Be(2);
		result.Errors.Should().BeEmpty();
		(await _context.Notes.FindAsync(note3.Id))!.Tags.Should().Be("new-tags");
		(await _context.Notes.FindAsync(note4.Id))!.Tags.Should().Be("new-tags");
	}

	[Fact]
	public async Task Handles_AiService_Exception_And_Continues()
	{
		// Arrange
		var ownerSubject = "user3";

		var note5 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 5", Content = "Content 5", Tags = null };

		var note6 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = "Note 6", Content = "Content 6", Tags = null };

		_context.Notes.AddRange(note5, note6);
		await _context.SaveChangesAsync();

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(ci =>
				{
					var title = ci.ArgAt<string>(0);

					if (title == "Note 5")
					{
						throw new Exception("AI error");
					}

					return "tags-ok";
				});

		var command = new BackfillTagsCommand { UserSubject = ownerSubject, OnlyMissing = true };

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.ProcessedCount.Should().Be(1);
		result.TotalNotes.Should().Be(2);
		result.Errors.Should().ContainSingle(e => e.Contains("Note 5"));
		(await _context.Notes.FindAsync(note5.Id))!.Tags.Should().BeNull();
		(await _context.Notes.FindAsync(note6.Id))!.Tags.Should().Be("tags-ok");
	}

}