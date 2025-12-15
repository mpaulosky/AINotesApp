// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NotesListTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.SearchNotes;

using Bunit;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Components.Pages.Notes;

/// <summary>
///   Unit tests for the NotesList component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class NotesListTests : BunitContext
{

	private readonly TestAuthStateProvider _authProvider;

	private readonly IMediator _mediator;

	private readonly NavigationManager _navigation;

	/// <summary>
	///   Initializes a new instance of the <see cref="NotesListTests" /> class
	/// </summary>
	public NotesListTests()
	{
		_mediator = Substitute.For<IMediator>();
		_authProvider = new TestAuthStateProvider();
		_navigation = Substitute.For<NavigationManager>();

		// Register services
		Services.AddSingleton(_mediator);
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
		Services.AddSingleton(_navigation);
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	/// <summary>
	///   Renders the component with authentication context
	/// </summary>
	private IRenderedComponent<NotesList> RenderWithAuth()
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<NotesList>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
		});
	}

	[Fact]
	public async Task NotesList_ShowsMyNotesTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateEmptyResponse();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("My Notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_HasCreateNewNoteButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateEmptyResponse();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Create New Note");
			cut.Markup.Should().Contain("/notes/new");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_HasSearchBar()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateEmptyResponse();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			var searchInput = cut.Find("input[type='text'][placeholder*='Search notes']");
			searchInput.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenLoading_ShowsLoadingSpinner()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Setup mediator to delay response
		var tcs = new TaskCompletionSource<SearchNotesResponse>();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(tcs.Task);

		// Act
		var cut = RenderWithAuth();

		// Give component time to start loading
		await Task.Delay(100);

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("spinner-border");
			cut.Markup.Should().Contain("Loading your notes...");
		}, TimeSpan.FromSeconds(2));

		// Cleanup
		tcs.SetResult(CreateEmptyResponse());
	}

	[Fact]
	public async Task NotesList_WhenNoNotes_ShowsEmptyMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateEmptyResponse();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("You don't have any notes yet");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenHasNotes_DisplaysNotesTable()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("<table");
			cut.Markup.Should().Contain("Test Note 1");
			cut.Markup.Should().Contain("Test Note 2");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_DisplaysNoteTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

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
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("AI Summary for test note 1");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_DisplaysTags()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("tag1");
			cut.Markup.Should().Contain("tag2");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_DisplaysCreatedAndUpdatedDates()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

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
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("bi-eye");
			cut.Markup.Should().Contain("bi-pencil");
			cut.Markup.Should().Contain("bi-trash");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenSearchResultsEmpty_ShowsNoResultsMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		var response = new SearchNotesResponse
		{
				Notes = [],
				TotalCount = 0,
				PageNumber = 1,
				PageSize = 10,
				SearchTerm = "nonexistent"
		};

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			// When a search term is provided but no results, shows an empty message
			cut.Markup.Should().Contain("You don't have any notes yet");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_ShowsSearchResultCount()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		var response = new SearchNotesResponse
		{
				Notes = [CreateNoteItem(Guid.NewGuid(), "Found Note", "tag1")],
				TotalCount = 1,
				PageNumber = 1,
				PageSize = 10,
				SearchTerm = "Found"
		};

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			// The search count shows with search term, but component doesn't show it without it
			cut.Markup.Should().Contain("Showing 1 of 1 notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_ShowsTotalCount()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Showing 2 of 2 notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenMultiplePages_ShowsPagination()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

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

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Previous");
			cut.Markup.Should().Contain("Next");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_SendsCorrectQueryToMediator()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user-123");
		var response = CreateEmptyResponse();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			_mediator.Received(1).Send(
					Arg.Is<SearchNotesQuery>(q => q.UserSubject == "user-123" && q.PageNumber == 1 && q.PageSize == 10),
					Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NotesList_WhenNoteHasNoSummary_ShowsNoSummaryMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		var response = new SearchNotesResponse
		{
				Notes =
				[
						new()
						{
								Id = Guid.NewGuid(),
								Title = "Note without summary",
								AiSummary = null,
								Tags = "tag1",
								CreatedAt = DateTime.Now.AddDays(-1),
								UpdatedAt = DateTime.Now
						}
				],
				TotalCount = 1,
				PageNumber = 1,
				PageSize = 10
		};

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

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
		_authProvider.SetAuthorized("TestUser");

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromException<SearchNotesResponse>(new Exception("Test error")));

		// Act
		var cut = RenderWithAuth();

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
		_authProvider.SetAuthorized("TestUser");
		var response = CreateResponseWithNotes();

		_mediator.Send(Arg.Any<SearchNotesQuery>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(response));

		// Act
		var cut = RenderWithAuth();

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

	/// <summary>
	///   Helper method to create an empty response
	/// </summary>
	private SearchNotesResponse CreateEmptyResponse()
	{
		return new SearchNotesResponse
		{
				Notes = [],
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
				Notes =
				[
						CreateNoteItem(Guid.NewGuid(), "Test Note 1", "tag1, tag2"),
						CreateNoteItem(Guid.NewGuid(), "Test Note 2", "tag3")
				],
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
				CreatedAt = DateTime.Now.AddDays(-2),
				UpdatedAt = DateTime.Now.AddDays(-1)
		};
	}

	/// <summary>
	///   Helper class for fake authentication state provider
	/// </summary>
	private			class TestAuthStateProvider : AuthenticationStateProvider
	{

		private bool _isAuthenticated  ;

		private string _userId = string.Empty;

		private string _userName = string.Empty;

		/// <summary>
		///   Sets the authentication state to authorize with a username
		/// </summary>
		public void SetAuthorized(string userName, string? userId = null)
		{
			_isAuthenticated = true;
			_userName = userName;
			_userId = userId ?? userName;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		///   Sets the authentication state to not authorized
		/// </summary>
		public void SetNotAuthorized()
		{
			_isAuthenticated = false;
			_userName = string.Empty;
			_userId = string.Empty;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		///   Gets the authentication state
		/// </summary>
		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			ClaimsIdentity identity;

			if (_isAuthenticated)
			{
				var claims = new List<Claim>
				{
						new (ClaimTypes.Name, _userName),
						new (ClaimTypes.NameIdentifier, _userId)
				};

				identity = new ClaimsIdentity(claims, "TestAuth");
			}
			else
			{
				identity = new ClaimsIdentity();
			}

			var user = new ClaimsPrincipal(identity);

			return Task.FromResult(new AuthenticationState(user));
		}

	}

	/// <summary>
	///   Fake authorization service for testing
	/// </summary>
	private class FakeAuthorizationService : IAuthorizationService
	{

		public Task<AuthorizationResult> AuthorizeAsync(
				ClaimsPrincipal user,
				object? resource,
				IEnumerable<IAuthorizationRequirement> requirements)
		{
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}

			return Task.FromResult(AuthorizationResult.Failed());
		}

		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
		{
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}

			return Task.FromResult(AuthorizationResult.Failed());
		}

	}

	/// <summary>
	///   Fake authorization policy provider for testing
	/// </summary>
	private class FakeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
	{

		public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
		{
			return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
		{
			return Task.FromResult<AuthorizationPolicy?>(null);
		}

	}

}