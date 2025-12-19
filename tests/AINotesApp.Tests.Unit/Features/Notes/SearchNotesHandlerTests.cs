// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SearchNotesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.SearchNotes;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Unit.Features.Notes;

[ExcludeFromCodeCoverage]
public class SearchNotesHandlerTests
{

	private ApplicationDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		return new ApplicationDbContext(options);
	}

	[Fact]
	public async Task Returns_Notes_Matching_SearchTerm()
	{
		// Arrange
		var db = CreateDbContext();

		db.Notes.AddRange(
				new Note { Id = Guid.NewGuid(), Title = "Test Note", Content = "Hello world", OwnerSubject = "user1" },
				new Note { Id = Guid.NewGuid(), Title = "Another", Content = "Something else", OwnerSubject = "user1" },
				new Note { Id = Guid.NewGuid(), Title = "Irrelevant", Content = "No match", OwnerSubject = "user2" }
		);

		await db.SaveChangesAsync();
		var handler = new SearchNotesHandler(db);
		var query = new SearchNotesQuery { UserSubject = "user1", SearchTerm = "test" };

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Notes.Should().ContainSingle(n => n.Title == "Test Note");
		result.TotalCount.Should().Be(1); // Only 1 note matches the search term 'test'
	}

	[Fact]
	public async Task Returns_Empty_When_No_Match()
	{
		var db = CreateDbContext();

		db.Notes.Add(new Note
		{ Id = Guid.NewGuid(), Title = "Test Note", Content = "Hello world", OwnerSubject = "user1" });

		await db.SaveChangesAsync();
		var handler = new SearchNotesHandler(db);
		var query = new SearchNotesQuery { UserSubject = "user1", SearchTerm = "notfound" };
		var result = await handler.Handle(query, CancellationToken.None);

		result.Notes.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task Returns_Only_Notes_For_User()
	{
		var db = CreateDbContext();

		db.Notes.AddRange(
				new Note { Id = Guid.NewGuid(), Title = "A", Content = "A", OwnerSubject = "user1" },
				new Note { Id = Guid.NewGuid(), Title = "B", Content = "B", OwnerSubject = "user2" }
		);

		await db.SaveChangesAsync();
		var handler = new SearchNotesHandler(db);
		var query = new SearchNotesQuery { UserSubject = "user1" };
		var result = await handler.Handle(query, CancellationToken.None);

		result.Notes.Should().ContainSingle(n => n.Title == "A");
		result.TotalCount.Should().Be(1);
	}

	[Fact]
	public async Task Supports_Pagination()
	{
		var db = CreateDbContext();

		for (var i = 1; i <= 15; i++)
		{
			db.Notes.Add(new Note { Id = Guid.NewGuid(), Title = $"Note {i}", Content = "", OwnerSubject = "user1" });
		}

		await db.SaveChangesAsync();
		var handler = new SearchNotesHandler(db);
		var query = new SearchNotesQuery { UserSubject = "user1", PageNumber = 2, PageSize = 5 };
		var result = await handler.Handle(query, CancellationToken.None);

		result.Notes.Should().HaveCount(5);
		result.TotalCount.Should().Be(15);
	}

}