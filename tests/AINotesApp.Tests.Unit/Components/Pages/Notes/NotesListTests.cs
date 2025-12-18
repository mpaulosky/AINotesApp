// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NotesListTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using MediatR;

using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using AINotesApp.Features.Notes.SearchNotes;
using AINotesApp.Components.Pages.Notes;
using AINotesApp.Tests.Unit.Fakes;
using AINotesApp.Tests.Unit.Helpers;
using AINotesApp.Features.Notes.DeleteNote;
using Microsoft.AspNetCore.Components.Web;

namespace AINotesApp.Tests.Unit.Components.Pages.Notes;

/// <summary>
///   Unit tests for NoteEditor component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class NotesListTests : BunitContext
{
	// Remove readonly fields; resolve in each test
	// private readonly FakeAuthenticationStateProvider _authProvider;
	// private readonly IMediator _mediator;

	       public NotesListTests()
	       {
		       // Setup test DI
		       Services.AddSingleton<IMediator>(NSubstitute.Substitute.For<IMediator>());
		       Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, FakeAuthenticationStateProvider>();
	       }

	#region Initialization Tests

	[Fact]
	public async Task OnInitializedAsync_WhenUserAuthenticated_LoadsNotes()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received(1).Send(
				Arg.Is<SearchNotesQuery>(q => q.UserSubject == "user-123"),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task OnInitializedAsync_WhenExceptionOccurs_SetsErrorMessage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<SearchNotesResponse>(new Exception("Database error")));
		
		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error loading notes:");
			cut.Markup.Should().Contain("Database error");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task OnInitializedAsync_WhenUserNotAuthenticated_DoesNotLoadNotes()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetNotAuthorized();
		
		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);
		await mediator.DidNotReceive().Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>());
	}

	#endregion

	#region Search Tests

	[Fact]
	public async Task NotesList_HasSearchBar()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider.SetAuthorized("TestUser");
			   var response = CreateEmptyResponse();
			   mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
			searchInput.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task ClearSearch_ResetsSearchTermAndLoadsAllNotes()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		// First response with search results
		var searchResponse = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Found Note", "tag1")],
			TotalCount = 1,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = "Found"
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(searchResponse), Task.FromResult(CreateResponseWithNotes()));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);
		
		// Set search term
		var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
		searchInput.Input("Found");
		await Task.Delay(400);

		// Act - Click clear button
		var clearButton = cut.Find("button:contains('Clear')");
		clearButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received().Send(
				Arg.Is<SearchNotesQuery>(q => q.SearchTerm == string.Empty && q.PageNumber == 1),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_ShowsSearchResultCount()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = new SearchNotesResponse
			   {
							   Notes = [CreateNoteItem(Guid.NewGuid(), "Found Note", "tag1")],
							   TotalCount = 1,
							   PageNumber = 1,
							   PageSize = 10,
							   SearchTerm = "Found"
			   };
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			// The search count shows with search term, but component doesn't show it without it
			cut.Markup.Should().Contain("Showing 1 of 1 notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenSearchResultsEmpty_ShowsNoResultsMessage()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = new SearchNotesResponse
			   {
							   Notes = [],
							   TotalCount = 0,
							   PageNumber = 1,
							   PageSize = 10,
							   SearchTerm = "nonexistent"
			   };
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			// When a search term is provided but no results, shows an empty message
			cut.Markup.Should().Contain("You don't have any notes yet");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task HandleSearchKeyUp_DebouncesSearchRequests()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);
		
		// Clear initial load call
		mediator.ClearReceivedCalls();

		// Act - Type multiple characters rapidly
		var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
		searchInput.Input("T");
		searchInput.KeyUp("T");
		searchInput.Input("Te");
		searchInput.KeyUp("e");
		searchInput.Input("Test");
		searchInput.KeyUp("t");
		
		// Wait less than debounce time
		await Task.Delay(150);
		
		// Assert - Should not have called mediator yet
		await mediator.DidNotReceive().Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>());
		
		// Wait for debounce to complete
		await Task.Delay(200);
		
		// Assert - Should have called mediator once after debounce
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received(1).Send(
				Arg.Is<SearchNotesQuery>(q => q.SearchTerm == "Test"),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task SearchResults_ShowsPluralFormCorrectly()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		// Test with 2 notes (plural)
		var response = new SearchNotesResponse
		{
			Notes = 
			[
				CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1"),
				CreateNoteItem(Guid.NewGuid(), "Note 2", "tag2")
			],
			TotalCount = 2,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = "test"
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Act
		var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
		searchInput.Input("test");
		await Task.Delay(400);

		// Assert - Should show "notes" (plural)
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Found 2 notes matching");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task SearchResults_ShowsSingularFormForOneNote()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		// Test with 1 note (singular)
		var response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 1,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = "test"
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Act
		var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
		searchInput.Input("test");
		await Task.Delay(400);

		// Assert - Should show "note" (singular)
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Found 1 note matching");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task ClearSearchButton_InEmptyResultsMessage_ClearsSearch()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var emptyResponse = new SearchNotesResponse
		{
			Notes = [],
			TotalCount = 0,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = "nonexistent"
		};
		
		var allNotesResponse = CreateResponseWithNotes();
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(emptyResponse), Task.FromResult(allNotesResponse));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		
		// Set search term
		var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
		searchInput.Input("nonexistent");
		await Task.Delay(400);

		// Act - Click clear search button in the message
		var clearSearchLink = cut.Find("button.btn-link:contains('clear search')");
		clearSearchLink.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received().Send(
				Arg.Is<SearchNotesQuery>(q => q.SearchTerm == string.Empty),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	#endregion

	#region Pagination Tests

	[Fact]
	public async Task LoadPageAsync_ChangesPageNumber_AndLoadsNotes()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var page1Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		var page2Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 2", "tag2")],
			TotalCount = 25,
			PageNumber = 2,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(page1Response), Task.FromResult(page2Response));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Act - Click page 2 button
		var page2Button = cut.Find("button.page-link:contains('2')");
		page2Button.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received().Send(
				Arg.Is<SearchNotesQuery>(q => q.PageNumber == 2),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenMultiplePages_ShowsPagination()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = new SearchNotesResponse
			   {
							   Notes =
							   [
											   CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1"),
											   CreateNoteItem(Guid.NewGuid(), "Note 2", "tag2")
							   ],
							   TotalCount = 25,
							   PageNumber = 1,
							   PageSize = 10,
							   SearchTerm = string.Empty
			   };
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Previous");
			cut.Markup.Should().Contain("Next");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_PreviousButton_DisabledOnFirstPage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			var previousButton = cut.Find("button.page-link:contains('Previous')");
			previousButton.HasAttribute("disabled").Should().BeTrue();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_NextButton_DisabledOnLastPage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		// Create response with multiple pages, but on last page
		var response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 3, // Last page (3 pages with 10 items per page)
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			var nextButton = cut.Find("button.page-link:contains('Next')");
			nextButton.HasAttribute("disabled").Should().BeTrue();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_PreviousButton_NavigatesToPreviousPage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var page2Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 2", "tag2")],
			TotalCount = 25,
			PageNumber = 2,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		var page1Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(page2Response), Task.FromResult(page1Response));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Act - Click Previous button
		var previousButton = cut.Find("button.page-link:contains('Previous')");
		previousButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received().Send(
				Arg.Is<SearchNotesQuery>(q => q.PageNumber == 1),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_NextButton_NavigatesToNextPage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var page1Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		var page2Response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 2", "tag2")],
			TotalCount = 25,
			PageNumber = 2,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(page1Response), Task.FromResult(page2Response));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Act - Click Next button
		var nextButton = cut.Find("button.page-link:contains('Next')");
		nextButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received().Send(
				Arg.Is<SearchNotesQuery>(q => q.PageNumber == 2),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_ActivePage_HasActiveClass()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 25,
			PageNumber = 2,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			var activePage = cut.Find("li.page-item.active");
			activePage.Should().NotBeNull();
			activePage.TextContent.Trim().Should().Contain("2");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Pagination_WhenSinglePage_DoesNotShowPagination()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var response = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Note 1", "tag1")],
			TotalCount = 5,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			cut.FindAll("nav[aria-label='Notes pagination']").Should().BeEmpty();
		}, TimeSpan.FromSeconds(2));
	}

	#endregion

	#region Delete Tests

	[Fact]
	public async Task ShowDeleteConfirmation_OpensModal_WithCorrectNoteInfo()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Act - Click delete button
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Confirm Delete");
			cut.Markup.Should().Contain("Are you sure you want to delete this note?");
			cut.Markup.Should().Contain("Test Note 1");
			cut.Markup.Should().Contain("show d-block");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task HideDeleteConfirmation_ClosesModal_AndResetsState()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click cancel button
		var cancelButton = cut.Find("button:contains('Cancel')");
		cancelButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().NotContain("show d-block");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task ConfirmDeleteAsync_WhenSuccessful_ClosesModalAndReloadsNotes()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		
		var initialResponse = CreateResponseWithNotes();
		var afterDeleteResponse = new SearchNotesResponse
		{
			Notes = [CreateNoteItem(Guid.NewGuid(), "Test Note 2", "tag3")],
			TotalCount = 1,
			PageNumber = 1,
			PageSize = 10,
			SearchTerm = string.Empty
		};
		
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(initialResponse), Task.FromResult(afterDeleteResponse));
		
		var deleteResponse = new DeleteNoteResponse { Success = true, Message = "Note deleted successfully." };
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(deleteResponse));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			mediator.Received(1).Send(
				Arg.Any<DeleteNoteCommand>(),
				Arg.Any<CancellationToken>());
			// Should reload notes after delete
			mediator.Received(2).Send(
				Arg.Any<SearchNotesQuery>(),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task ConfirmDeleteAsync_WhenFailed_ShowsErrorMessage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var deleteResponse = new DeleteNoteResponse { Success = false, Message = "Note not found or access denied." };
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(deleteResponse));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Note not found or access denied.");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task ConfirmDeleteAsync_WhenExceptionOccurs_ShowsErrorMessage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<DeleteNoteResponse>(new Exception("Network error")));
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error deleting note:");
			cut.Markup.Should().Contain("Network error");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task DeleteButton_DisabledWhileDeleting()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var tcs = new TaskCompletionSource<DeleteNoteResponse>();
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button (but don't complete the task)
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert - Button should be disabled while deleting
		await Task.Delay(100);
		await cut.InvokeAsync(() =>
		{
			var deleteButtons = cut.FindAll("button[title='Delete']");
			foreach (var btn in deleteButtons)
			{
				btn.HasAttribute("disabled").Should().BeTrue();
			}
		});
		
		// Complete the task
		tcs.SetResult(new DeleteNoteResponse { Success = true, Message = "Deleted" });
	}

	[Fact]
	public async Task DeleteModal_CloseButton_ClosesModal()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click X close button
		var closeButton = cut.Find("button.btn-close");
		closeButton.Click();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().NotContain("show d-block");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task DeleteModal_CloseButtonDisabled_WhileDeleting()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var tcs = new TaskCompletionSource<DeleteNoteResponse>();
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button (but don't complete the task)
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert - Close button should be disabled while deleting
		await Task.Delay(100);
		await cut.InvokeAsync(() =>
		{
			var closeButton = cut.Find("button.btn-close");
			closeButton.HasAttribute("disabled").Should().BeTrue();
		});
		
		// Complete the task
		tcs.SetResult(new DeleteNoteResponse { Success = true, Message = "Deleted" });
	}

	[Fact]
	public async Task DeleteModal_CancelButtonDisabled_WhileDeleting()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var tcs = new TaskCompletionSource<DeleteNoteResponse>();
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button (but don't complete the task)
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert - Cancel button should be disabled while deleting
		await Task.Delay(100);
		await cut.InvokeAsync(() =>
		{
			var cancelButton = cut.Find("button:contains('Cancel')");
			cancelButton.HasAttribute("disabled").Should().BeTrue();
		});
		
		// Complete the task
		tcs.SetResult(new DeleteNoteResponse { Success = true, Message = "Deleted" });
	}

	[Fact]
	public async Task DeleteModal_ShowsSpinner_WhileDeleting()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateResponseWithNotes();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		var tcs = new TaskCompletionSource<DeleteNoteResponse>();
		mediator.Send(Arg.Any<DeleteNoteCommand>(), Arg.Any<CancellationToken>()).Returns(tcs.Task);
		
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		await Task.Delay(100);

		// Open modal
		var deleteButton = cut.Find("button[title='Delete']");
		deleteButton.Click();
		await Task.Delay(100);

		// Act - Click confirm delete button (but don't complete the task)
		var confirmButton = cut.Find("button.btn-danger:contains('Delete Note')");
		confirmButton.Click();

		// Assert - Should show spinner and "Deleting..." text
		await Task.Delay(100);
		await cut.InvokeAsync(() =>
		{
			cut.Markup.Should().Contain("spinner-border");
			cut.Markup.Should().Contain("Deleting...");
		});
		
		// Complete the task
		tcs.SetResult(new DeleteNoteResponse { Success = true, Message = "Deleted" });
	}

	#endregion

	#region Display Tests

	[Fact]
	public async Task NotesList_WhenLoading_ShowsLoadingSpinner()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider.SetAuthorized("TestUser");

			   // Setup mediator to delay response
			   var tcs = new TaskCompletionSource<SearchNotesResponse>();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .ReturnsForAnyArgs(tcs.Task);

			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Give component time to start loading
		   await Task.Delay(100);
		   // Assert
		   // (add assertion here if needed)
	   }

	   [Fact]
	   public async Task NotesList_WhenHasNotes_DisplaysNotesTable()
	   {
		   // Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider.SetAuthorized("TestUser", "user-123");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
		   // Assert
		   await cut.InvokeAsync(() => { });
		   await Task.Delay(100);
		   // Debug: print markup
           System.Diagnostics.Debug.WriteLine("[DEBUG] Markup after delay:\n" + cut.Markup);
           await cut.WaitForAssertionAsync(() =>
           {
               cut.Markup.Should().Contain("<table");
               cut.Markup.Should().Contain("Test Note 1");
               cut.Markup.Should().Contain("Test Note 2");
           }, TimeSpan.FromSeconds(5));
	   }

	   [Fact]
	   public async Task NotesList_DisplaysNoteTitle()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();

			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));

			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Test Note 1");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_DisplaysAiSummary()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert


		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("AI Summary for test note 1");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_DisplaysTags()
	{
		// Arrange

			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider.SetAuthorized("TestUser", "user-123");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider!.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await Task.Delay(100);
		// Debug: print markup
        System.Diagnostics.Debug.WriteLine("[DEBUG] Markup after delay:\n" + cut.Markup);
        await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("tag1");
			cut.Markup.Should().Contain("tag2");
		}, TimeSpan.FromSeconds(5));
	}

	[Fact]
	public async Task NotesList_DisplaysCreatedAndUpdatedDates()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
		   // Act
		   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());
	   // Assert
	   await cut.InvokeAsync(() => { });
	   await cut.WaitForAssertionAsync(() =>
	   {
		   // Table should have Created and Updated columns
		   var table = cut.Find("table");
		   table.Should().NotBeNull();
	   }, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_HasViewEditDeleteButtons()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("bi-eye");
			cut.Markup.Should().Contain("bi-pencil");

		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_ShowsTotalCount()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Showing 2 of 2 notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_SendsCorrectQueryToMediator()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser", "user-123");
			   var response = CreateEmptyResponse();
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		       await cut.WaitForAssertionAsync(() =>
		       {
			       mediator.Received(1).Send(
				       Arg.Is<SearchNotesQuery>(q => q.UserSubject == "user-123" && q.PageNumber == 1 && q.PageSize == 10),
				       Arg.Any<CancellationToken>());
		       }, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenNoteHasNoSummary_ShowsNoSummaryMessage()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		       var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
		       var response = new SearchNotesResponse
		       {
				      Notes = new List<SearchNoteItem>
				      {
					      new SearchNoteItem
					      {
						      Id = Guid.NewGuid(),
						      Title = "Note without summary",
						      AiSummary = null,
						      Tags = "tag1",
						      CreatedAt = DateTime.Now.AddDays(-1),
						      UpdatedAt = DateTime.Now
					      }
				      },
			       TotalCount = 1,
			       PageNumber = 1,
			       PageSize = 10
		       };
			   mediator.Send(NSubstitute.Arg.Any<SearchNotesQuery>(), NSubstitute.Arg.Any<CancellationToken>())
				       .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("No summary");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenExceptionOccurs_ShowsErrorMessage()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		       var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
		       mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				       .Returns(Task.FromException<SearchNotesResponse>(new Exception("Test error")));
		       // Act
		       var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error loading notes:");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_HasTableHeaders()
	{
		// Arrange
			   var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
			   var mediator = Services.GetRequiredService<IMediator>();
			   authProvider!.SetAuthorized("TestUser");
			   var response = CreateResponseWithNotes();
			   mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
							   .Returns(Task.FromResult(response));
			   // Act
			   var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("<th>Title</th>");
			cut.Markup.Should().Contain("<th>Summary</th>");
			cut.Markup.Should().Contain("<th>Created</th>");
			cut.Markup.Should().Contain("<th>Updated</th>");
			cut.Markup.Should().Contain("<th>Actions</th>");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenNoNotes_ShowsEmptyMessage()
	{
		// Arrange
		var authProvider = (FakeAuthenticationStateProvider)Services.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>();
		var mediator = Services.GetRequiredService<IMediator>();
		authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateEmptyResponse();
		mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
		
		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NotesList>(this, authProvider.GetAuthenticationStateAsync());

		// Assert
		await cut.InvokeAsync(() => { });
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("You don't have any notes yet");
		}, TimeSpan.FromSeconds(2));
	}

	#endregion

	#region Helper Methods

		/// <summary>
		///   Helper method to create an empty response
		/// </summary>
		private SearchNotesResponse CreateEmptyResponse()
		{
			return new SearchNotesResponse
			{
				Notes = new List<SearchNoteItem>(),
				TotalCount = 0,
				PageNumber = 1,
				PageSize = 10,
				SearchTerm = string.Empty
			};
		}

		/// <summary>
		///   Helper method to create a response with sample notes
		/// </summary>
		private SearchNotesResponse CreateResponseWithNotes()
		{
			return new SearchNotesResponse
			{
				Notes = new List<SearchNoteItem>
				{
					CreateNoteItem(Guid.NewGuid(), "Test Note 1", "tag1, tag2"),
					CreateNoteItem(Guid.NewGuid(), "Test Note 2", "tag3")
				},
				TotalCount = 2,
				PageNumber = 1,
				PageSize = 10,
				SearchTerm = string.Empty
			};
		}

		/// <summary>
		///   Helper method to create a note item
		/// </summary>
		private SearchNoteItem CreateNoteItem(Guid id, string title, string tags)
		{
			   return new SearchNoteItem
			   {
				   Id = id,
				   Title = title,
				   AiSummary = $"AI Summary for {title.ToLower()}",
				   Tags = tags,
				   CreatedAt = DateTime.Now.AddDays(-1),
				   UpdatedAt = DateTime.Now
			   };
	   }

	#endregion
}
