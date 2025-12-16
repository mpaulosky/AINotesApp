// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BackfillTagsIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Features.Notes.BackfillTags;
using AINotesApp.Services;
using AINotesApp.Tests.Integration.Helpers;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace AINotesApp.Tests.Integration.Features.Notes;

/// <summary>
///   Integration tests for the BackfillTags feature.
/// </summary>
[ExcludeFromCodeCoverage]
public class BackfillTagsIntegrationTests : IAsyncLifetime
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	private readonly BackfillTagsHandler _handler;

	private readonly ServiceProvider _provider;

	/// <summary>
	///   Initializes a new instance of the <see cref="BackfillTagsIntegrationTests" /> class.
	/// </summary>
	public BackfillTagsIntegrationTests()
	{
		var services = new ServiceCollection();

		services.AddDbContext<ApplicationDbContext>(options =>
				options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

		services.AddScoped<IAiService>(_ => MockAiServiceHelper.CreateMockAiService());
		_provider = services.BuildServiceProvider();
		_context = _provider.GetRequiredService<ApplicationDbContext>();
		_aiService = _provider.GetRequiredService<IAiService>();
		_handler = new BackfillTagsHandler(_context, _aiService);
	}

	/// <summary>
	///   Called immediately after the class has been created, before it is used.
	/// </summary>
	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	///   Called when an object is no longer needed. Called just before the class is disposed.
	/// </summary>
	public async Task DisposeAsync()
	{
		await _provider.DisposeAsync();
	}

	/// <summary>
	///   Verifies that backfill processes multiple users' notes in isolation.
	/// </summary>
	[Fact]
	public async Task BackfillTags_Processes_Multiple_Users_Isolated()
	{
		// Arrange
		var userA = "userA";
		var userB = "userB";

		var noteA1 = NoteTestDataBuilder.CreateDefault()
			.WithOwnerSubject(userA)
			.WithTitle("A1")
			.WithContent("A1 content")
			.Build();

		var noteA2 = NoteTestDataBuilder.CreateDefault()
			.WithOwnerSubject(userA)
			.WithTitle("A2")
			.WithContent("A2 content")
			.Build();

		var noteB1 = NoteTestDataBuilder.CreateDefault()
			.WithOwnerSubject(userB)
			.WithTitle("B1")
			.WithContent("B1 content")
			.Build();

		_context.Notes.AddRange(noteA1, noteA2, noteB1);
		await _context.SaveChangesAsync();

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(ci => "tags-" + ci.ArgAt<string>(0));

		// Act
		var resultA = await _handler.Handle(new BackfillTagsCommand { UserSubject = userA }, CancellationToken.None);
		var resultB = await _handler.Handle(new BackfillTagsCommand { UserSubject = userB }, CancellationToken.None);

		// Assert
		resultA.ProcessedCount.Should().Be(2);
		resultA.TotalNotes.Should().Be(2);
		resultB.ProcessedCount.Should().Be(1);
		resultB.TotalNotes.Should().Be(1);

		var foundA1 = await _context.Notes.FindAsync(noteA1.Id);
		var foundA2 = await _context.Notes.FindAsync(noteA2.Id);
		var foundB1 = await _context.Notes.FindAsync(noteB1.Id);
		foundA1.Should().NotBeNull("noteA1 should exist in the database");
		foundA2.Should().NotBeNull("noteA2 should exist in the database");
		foundB1.Should().NotBeNull("noteB1 should exist in the database");
		foundA1.Tags.Should().Be("tags-A1");
		foundA2.Tags.Should().Be("tags-A2");
		foundB1.Tags.Should().Be("tags-B1");
	}

	[Fact]
	public async Task BackfillTags_Saves_Periodically()
	{
		// Arrange
		var ownerSubject = "userC";


		for (var i = 1; i <= 7; i++)
		{
		    _context.Notes.Add(NoteTestDataBuilder.CreateDefault()
			    .WithOwnerSubject(ownerSubject)
			    .WithTitle($"Note {i}")
			    .WithContent($"Content {i}")
			    .Build());
		}

		await _context.SaveChangesAsync();


		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(ci => "bulk-tags");

		// Act

		var result = await _handler.Handle(new BackfillTagsCommand { UserSubject = ownerSubject }, CancellationToken.None);

		// Assert
		result.ProcessedCount.Should().Be(7);
		result.TotalNotes.Should().Be(7);
		result.Errors.Should().BeEmpty();
		var tags = await _context.Notes.Where(n => n.OwnerSubject == ownerSubject).Select(n => n.Tags).ToListAsync();
		tags.Should().AllBeEquivalentTo("bulk-tags");
	}

}