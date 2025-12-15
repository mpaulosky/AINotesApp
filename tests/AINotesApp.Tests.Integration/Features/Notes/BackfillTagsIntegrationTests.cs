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
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace AINotesApp.Tests.Integration.Features.Notes;

[ExcludeFromCodeCoverage]
public class BackfillTagsIntegrationTests : IAsyncLifetime
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	private readonly BackfillTagsHandler _handler;

	private readonly ServiceProvider _provider;

	public BackfillTagsIntegrationTests()
	{
		var services = new ServiceCollection();

		services.AddDbContext<ApplicationDbContext>(options =>
				options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

		services.AddScoped<IAiService>(_ => Substitute.For<IAiService>());
		_provider = services.BuildServiceProvider();
		_context = _provider.GetRequiredService<ApplicationDbContext>();
		_aiService = _provider.GetRequiredService<IAiService>();
		_handler = new BackfillTagsHandler(_context, _aiService);
	}

	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	public async Task DisposeAsync()
	{
		await _provider.DisposeAsync();
	}

	[Fact]
	public async Task BackfillTags_Processes_Multiple_Users_Isolated()
	{
		// Arrange
		var userA = "userA";
		var userB = "userB";

		var noteA1 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = userA, Title = "A1", Content = "A1 content", Tags = null };

		var noteA2 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = userA, Title = "A2", Content = "A2 content", Tags = null };

		var noteB1 = new Note
				{ Id = Guid.NewGuid(), OwnerSubject = userB, Title = "B1", Content = "B1 content", Tags = null };

		_context.Notes.AddRange(noteA1, noteA2, noteB1);
		await _context.SaveChangesAsync();

		_aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(ci => $"tags-{ci.ArgAt<string>(0)}");

		// Act
		var resultA = await _handler.Handle(new BackfillTagsCommand { UserSubject = userA }, CancellationToken.None);
		var resultB = await _handler.Handle(new BackfillTagsCommand { UserSubject = userB }, CancellationToken.None);

		// Assert
		resultA.ProcessedCount.Should().Be(2);
		resultA.TotalNotes.Should().Be(2);
		resultB.ProcessedCount.Should().Be(1);
		resultB.TotalNotes.Should().Be(1);
		(await _context.Notes.FindAsync(noteA1.Id))!.Tags.Should().Be("tags-A1");
		(await _context.Notes.FindAsync(noteA2.Id))!.Tags.Should().Be("tags-A2");
		(await _context.Notes.FindAsync(noteB1.Id))!.Tags.Should().Be("tags-B1");
	}

	[Fact]
	public async Task BackfillTags_Saves_Periodically()
	{
		// Arrange
		var ownerSubject = "userC";

		for (var i = 1; i <= 7; i++)
		{
			_context.Notes.Add(new Note
			{
					Id = Guid.NewGuid(), OwnerSubject = ownerSubject, Title = $"Note {i}", Content = $"Content {i}", Tags = null
			});
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