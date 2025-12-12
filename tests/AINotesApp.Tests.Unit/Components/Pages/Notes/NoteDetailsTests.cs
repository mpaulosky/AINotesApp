using System.Security.Claims;
using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.GetNoteDetails;
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
/// Unit tests for NoteDetails component using BUnit 2.x
/// </summary>
public class NoteDetailsTests : BunitContext
{
	private readonly IMediator _mediator;
	private readonly TestAuthStateProvider _authProvider;
	private readonly NavigationManager _navigation;

	/// <summary>
	/// Initializes a new instance of the <see cref="NoteDetailsTests"/> class
	/// </summary>
	public NoteDetailsTests()
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
	/// Renders the component with authentication context
	/// </summary>
	/// <param name="parameters">Component parameters</param>
	/// <returns>The rendered component</returns>
	private IRenderedComponent<NoteDetails> RenderWithAuth(Action<ComponentParameterCollectionBuilder<NoteDetails>>? parameters = null)
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();
		return Render<NoteDetails>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
			parameters?.Invoke(ps);
		});
	}

	[Fact]
	public async Task NoteDetails_WhenLoading_ShowsLoadingSpinner()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();

		// Setup mediator to delay response
		var tcs = new TaskCompletionSource<GetNoteDetailsResponse?>();
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(tcs.Task);

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Give the component time to start loading
		await Task.Delay(100);

		// Assert
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("spinner-border");
			cut.Markup.Should().Contain("Loading note...");
		}, TimeSpan.FromSeconds(2));

		// Cleanup
		tcs.SetResult(null);
	}

	[Fact]
	public async Task NoteDetails_WhenNoteNotFound_ShowsErrorMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(null));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Note not found or access denied.");
			cut.Markup.Should().Contain("Back to Notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenNoteExists_DisplaysNoteDetails()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note Title",
			Content = "Test note content",
			CreatedAt = DateTime.Now.AddDays(-1),
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Test Note Title");
			cut.Markup.Should().Contain("Test note content");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenNoteHasAiSummary_DisplaysAiSummary()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			AiSummary = "This is an AI-generated summary",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("AI Summary:");
			cut.Markup.Should().Contain("This is an AI-generated summary");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenNoteHasTags_DisplaysTags()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			Tags = "tag1, tag2, tag3",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Tags:");
			cut.Markup.Should().Contain("tag1");
			cut.Markup.Should().Contain("tag2");
			cut.Markup.Should().Contain("tag3");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_ShowsEditButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Edit");
			cut.Markup.Should().Contain($"/notes/{noteId}/edit");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_ShowsBackButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Back");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_DisplaysCreatedAndUpdatedDates()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var createdDate = DateTime.Now.AddDays(-2);
		var updatedDate = DateTime.Now.AddDays(-1);

		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			CreatedAt = createdDate,
			UpdatedAt = updatedDate
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Created:");
			cut.Markup.Should().Contain("Updated:");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenExceptionOccurs_ShowsErrorMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromException<GetNoteDetailsResponse?>(new Exception("Test error")));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Error loading note:");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenNoteHasNoTags_DoesNotShowTagsSection()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			Tags = null,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Test Note");
			cut.Markup.Should().NotContain("Tags:");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_WhenNoteHasNoAiSummary_DoesNotShowAiSummarySection()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			AiSummary = null,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Test Note");
			cut.Markup.Should().NotContain("AI Summary:");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_SendsCorrectQueryToMediator()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user-123");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Content",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			_mediator.Received(1).Send(
				Arg.Is<GetNoteDetailsQuery>(q => q.Id == noteId && q.UserId == "user-123"),
				Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteDetails_PageTitle_ReflectsNoteTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "My Important Note",
			Content = "Content",
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<GetNoteDetailsResponse?>(response));

		// Act
		var cut = RenderWithAuth(ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("My Important Note");
		}, TimeSpan.FromSeconds(2));
	}

	/// <summary>
	/// Helper class for fake authentication state provider
	/// </summary>
	public class TestAuthStateProvider : AuthenticationStateProvider
	{
		private bool _isAuthenticated = false;
		private string _userName = string.Empty;
		private string _userId = string.Empty;

		/// <summary>
		/// Sets the authentication state to authorized with a user name
		/// </summary>
		/// <param name="userName">The user name to use</param>
		/// <param name="userId">The user ID to use</param>
		public void SetAuthorized(string userName, string? userId = null)
		{
			_isAuthenticated = true;
			_userName = userName;
			_userId = userId ?? userName;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		/// Sets the authentication state to not authorized
		/// </summary>
		public void SetNotAuthorized()
		{
			_isAuthenticated = false;
			_userName = string.Empty;
			_userId = string.Empty;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		/// Gets the authentication state
		/// </summary>
		/// <returns>The authentication state</returns>
		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			ClaimsIdentity identity;

			if (_isAuthenticated)
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, _userName),
					new Claim(ClaimTypes.NameIdentifier, _userId)
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
	/// Fake authorization service for testing
	/// </summary>
	private class FakeAuthorizationService : IAuthorizationService
	{
		/// <summary>
		/// Authorizes the user based on requirements
		/// </summary>
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
		{
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}
			return Task.FromResult(AuthorizationResult.Failed());
		}

		/// <summary>
		/// Authorizes the user based on policy name
		/// </summary>
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
	/// Fake authorization policy provider for testing
	/// </summary>
	private class FakeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
	{
		/// <summary>
		/// Gets the default authorization policy
		/// </summary>
		public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
		{
			return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		/// <summary>
		/// Gets the authorization policy by name
		/// </summary>
		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		/// <summary>
		/// Gets the fallback authorization policy
		/// </summary>
		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
		{
			return Task.FromResult<AuthorizationPolicy?>(null);
		}
	}
}
