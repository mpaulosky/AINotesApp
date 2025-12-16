// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     RelatedNotesTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.GetRelatedNotes;

using Bunit;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Components.Pages.Notes;

/// <summary>
///   Tests for the RelatedNotes component.
/// </summary>
[ExcludeFromCodeCoverage]
public class RelatedNotesTests : BunitContext
{

	private readonly IMediator _mockMediator;

	private readonly Guid _testNoteId;

	private readonly string _testUserId;

	public RelatedNotesTests()
	{
		_mockMediator = Substitute.For<IMediator>();
		_testNoteId = Guid.NewGuid();
		_testUserId = "test-user-123";

		Services.AddSingleton(_mockMediator);
	}

	[Fact]
	public void RelatedNotes_HandlesException_GracefullyAndShowsEmptyList()
	{
		// Arrange
		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns<GetRelatedNotesResponse>(_ => throw new Exception("Database error"));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert - component should not crash and show no results
		cut.WaitForAssertion(() =>
		{
			cut.FindAll(".spinner-border").Should().BeEmpty();

			// After an error, the component should show the "no results" state since it has embedding
			var card = cut.Find(".card");
			card.Should().NotBeNull();
			var messageElement = cut.Find(".card-body.text-center.text-muted p");
			messageElement.TextContent.Should().Contain("No related notes found.");
		}, TimeSpan.FromSeconds(2));
	}

	/// <summary>
	///   Helper method to create a RelatedNoteItem for testing.
	/// </summary>
	private RelatedNoteItem CreateRelatedNoteItem(string title, string? summary, DateTime updatedAt)
	{
		return new RelatedNoteItem
		{
				Id = Guid.NewGuid(),
				Title = title,
				AiSummary = summary,
				UpdatedAt = updatedAt
		};
	}

	[Fact]
	public void RelatedNotes_RendersLoadingState_WhenInitializing()
	{
		// Arrange
		var tcs = new TaskCompletionSource<GetRelatedNotesResponse>();

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(tcs.Task);

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert - immediately check for loading spinner before the async completes
		var spinner = cut.Find(".spinner-border");
		spinner.Should().NotBeNull();
		spinner.ClassList.Should().Contain("spinner-border-sm");
		spinner.ClassList.Should().Contain("text-primary");

		// Complete the task
		tcs.SetResult(new GetRelatedNotesResponse { RelatedNotes = [] });
	}

	[Fact]
	public void RelatedNotes_DoesNotLoadNotes_WhenNoteIdIsEmpty()
	{
		// Arrange & Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, Guid.Empty)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll(".spinner-border").Should().BeEmpty();

			// With HasEmbedding=true but invalid params, it shows the "no results" message
			var card = cut.Find(".card");
			card.Should().NotBeNull();
			var message = cut.Find(".card-body.text-center.text-muted p");
			message.TextContent.Should().Contain("No related notes found.");
		}, TimeSpan.FromSeconds(2));

		_mockMediator.DidNotReceive().Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void RelatedNotes_DoesNotLoadNotes_WhenUserIdIsEmpty()
	{
		// Arrange & Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, string.Empty)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll(".spinner-border").Should().BeEmpty();

			// With HasEmbedding=true but invalid params, it shows the "no results" message
			var card = cut.Find(".card");
			card.Should().NotBeNull();
			var message = cut.Find(".card-body.text-center.text-muted p");
			message.TextContent.Should().Contain("No related notes found.");
		}, TimeSpan.FromSeconds(2));

		_mockMediator.DidNotReceive().Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void RelatedNotes_DoesNotLoadNotes_WhenHasEmbeddingIsFalse()
	{
		// Arrange & Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, false));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll(".spinner-border").Should().BeEmpty();
			cut.FindAll(".card").Should().BeEmpty();
		}, TimeSpan.FromSeconds(2));

		_mockMediator.DidNotReceive().Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void RelatedNotes_DisplaysRelatedNotes_WhenNotesExist()
	{
		// Arrange
		var relatedNote1 = CreateRelatedNoteItem("Related Note 1", "Summary 1", DateTime.UtcNow.AddDays(-1));
		var relatedNote2 = CreateRelatedNoteItem("Related Note 2", "Summary 2", DateTime.UtcNow.AddDays(-2));

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote1, relatedNote2]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var card = cut.Find(".card");
			card.Should().NotBeNull();

			var cardHeader = cut.Find(".card-header h5");
			cardHeader.TextContent.Should().Contain("Related Notes");

			var listItems = cut.FindAll(".list-group-item");
			listItems.Should().HaveCount(2);
			listItems[0].TextContent.Should().Contain("Related Note 1");
			listItems[1].TextContent.Should().Contain("Related Note 2");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DisplaysNoteTitle_ForEachRelatedNote()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Test Related Note Title", "Summary", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var title = cut.Find("h6");
			title.TextContent.Should().Be("Test Related Note Title");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DisplaysAiSummary_WhenSummaryExists()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", "This is an AI summary", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var summary = cut.Find("p.text-muted.small");
			summary.TextContent.Should().Be("This is an AI summary");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DoesNotDisplayAiSummary_WhenSummaryIsNull()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", null, DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("p.text-muted.small").Should().BeEmpty();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DoesNotDisplayAiSummary_WhenSummaryIsEmpty()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", "", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll("p.text-muted.small").Should().BeEmpty();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DisplaysUpdatedAtDate_FormattedCorrectly()
	{
		// Arrange
		var updateDate = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
		var relatedNote = CreateRelatedNoteItem("Note Title", "Summary", updateDate);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var dateText = cut.Find("small.text-muted");
			var expectedDate = updateDate.ToLocalTime().ToString("MMM d");
			dateText.TextContent.Trim().Should().Be(expectedDate);
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_CreatesLinkToNoteDetails_ForEachRelatedNote()
	{
		// Arrange
		var noteId = Guid.NewGuid();

		var relatedNote = new RelatedNoteItem
		{
				Id = noteId,
				Title = "Note Title",
				AiSummary = "Summary",
				UpdatedAt = DateTime.UtcNow
		};

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var link = cut.Find("a.list-group-item");
			link.GetAttribute("href").Should().Be($"/notes/{noteId}");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_AppliesCorrectCssClasses_ToRelatedNoteLinks()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", "Summary", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var link = cut.Find("a.list-group-item");
			link.ClassList.Should().Contain("list-group-item");
			link.ClassList.Should().Contain("list-group-item-action");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_ShowsNoResultsMessage_WhenNoRelatedNotesAndHasEmbedding()
	{
		// Arrange
		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = []
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var card = cut.Find(".card");
			card.Should().NotBeNull();

			var messageElement = cut.Find(".card-body.text-center.text-muted p");
			messageElement.TextContent.Should().Contain("No related notes found.");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DoesNotShowNoResultsMessage_WhenNoEmbedding()
	{
		// Arrange
		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = []
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, false));

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.FindAll(".card").Should().BeEmpty();
		}, TimeSpan.FromSeconds(2));

		_mockMediator.DidNotReceive().Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void RelatedNotes_ShowsInfoIcon_InNoResultsMessage()
	{
		// Arrange
		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = []
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var icon = cut.Find("i.bi-info-circle");
			icon.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_SendsCorrectQuery_ToMediator()
	{
		// Arrange
		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = []
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true)
				.Add(p => p.TopN, 10));

		// Assert
		cut.WaitForAssertion(() =>
		{
			_mockMediator.Received(1).Send(
					Arg.Is<GetRelatedNotesQuery>(q =>
							q.NoteId == _testNoteId &&
							q.UserSubject == _testUserId &&
							q.TopN == 10),
					Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_UsesDefaultTopN_WhenNotSpecified()
	{
		// Arrange
		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = []
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			_mockMediator.Received(1).Send(
					Arg.Is<GetRelatedNotesQuery>(q => q.TopN == 5),
					Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DisplaysRelatedNotesIcon_InHeader()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", "Summary", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var icon = cut.Find("i.bi-link-45deg");
			icon.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_AppliesCorrectCardStyling()
	{
		// Arrange
		var relatedNote = CreateRelatedNoteItem("Note Title", "Summary", DateTime.UtcNow);

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [relatedNote]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var card = cut.Find(".card.mt-4");
			card.Should().NotBeNull();

			var cardHeader = cut.Find(".card-header.bg-light");
			cardHeader.Should().NotBeNull();

			var listGroup = cut.Find(".list-group.list-group-flush");
			listGroup.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void RelatedNotes_DisplaysMultipleRelatedNotes_InCorrectOrder()
	{
		// Arrange
		var note1 = CreateRelatedNoteItem("First Note", "Summary 1", DateTime.UtcNow);
		var note2 = CreateRelatedNoteItem("Second Note", "Summary 2", DateTime.UtcNow.AddDays(-1));
		var note3 = CreateRelatedNoteItem("Third Note", "Summary 3", DateTime.UtcNow.AddDays(-2));

		var response = new GetRelatedNotesResponse
		{
				RelatedNotes = [note1, note2, note3]
		};

		_mockMediator.Send(Arg.Any<GetRelatedNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = Render<RelatedNotes>(parameters => parameters
				.Add(p => p.NoteId, _testNoteId)
				.Add(p => p.UserSubject, _testUserId)
				.Add(p => p.HasEmbedding, true));

		// Assert
		cut.WaitForAssertion(() =>
		{
			var listItems = cut.FindAll(".list-group-item");
			listItems.Should().HaveCount(3);

			var titles = cut.FindAll("h6");
			titles[0].TextContent.Should().Be("First Note");
			titles[1].TextContent.Should().Be("Second Note");
			titles[2].TextContent.Should().Be("Third Note");
		}, TimeSpan.FromSeconds(2));
	}

}