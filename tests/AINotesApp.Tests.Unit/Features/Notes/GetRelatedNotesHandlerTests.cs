// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetRelatedNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using AINotesApp.Data;
using AINotesApp.Features.Notes.GetRelatedNotes;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Features.Notes;

public class GetRelatedNotesHandlerTests
{

	private ApplicationDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		return new ApplicationDbContext(options);
	}


	[Fact]
	public async Task Returns_Empty_If_Note_Not_Found()
	{
		var db = CreateDbContext();
		var ai = Substitute.For<IAiService>();
		var handler = new GetRelatedNotesHandler(db, ai);
		var query = new GetRelatedNotesQuery { NoteId = Guid.NewGuid(), UserSubject = "user1" };
		var result = await handler.Handle(query, CancellationToken.None);

		result.RelatedNotes.Should().BeEmpty();
	}

	[Fact]
	public async Task Returns_Empty_If_Embedding_Null()
	{
		var db = CreateDbContext();
		var note = new Note { Id = Guid.NewGuid(), OwnerSubject = "user1", Embedding = null };
		db.Notes.Add(note);
		await db.SaveChangesAsync();
		var ai = Substitute.For<IAiService>();
		var handler = new GetRelatedNotesHandler(db, ai);
		var query = new GetRelatedNotesQuery { NoteId = note.Id, UserSubject = "user1" };
		var result = await handler.Handle(query, CancellationToken.None);

		result.RelatedNotes.Should().BeEmpty();
	}

	[Fact]
	public async Task Returns_Related_Notes_From_AiService()
	{
		var db = CreateDbContext();

		var note1 = new Note
		{
				Id = Guid.NewGuid(), OwnerSubject = "user1", Embedding = [ 1, 2, 3 ], Title = "A", AiSummary = "S1",
				UpdatedAt = DateTime.UtcNow
		};

		var note2 = new Note
		{
				Id = Guid.NewGuid(), OwnerSubject = "user1", Embedding = [ 2, 3, 4 ], Title = "B", AiSummary = "S2",
				UpdatedAt = DateTime.UtcNow
		};

		db.Notes.AddRange(note1, note2);
		await db.SaveChangesAsync();
		var ai = Substitute.For<IAiService>();

		ai.FindRelatedNotesAsync(Arg.Any<float[]>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<int>(),
						Arg.Any<CancellationToken>())
				.Returns([note2.Id]);

		var handler = new GetRelatedNotesHandler(db, ai);
		var query = new GetRelatedNotesQuery { NoteId = note1.Id, UserSubject = "user1", TopN = 5 };
		var result = await handler.Handle(query, CancellationToken.None);

		result.RelatedNotes.Should().ContainSingle(n => n.Id == note2.Id);
	}

}