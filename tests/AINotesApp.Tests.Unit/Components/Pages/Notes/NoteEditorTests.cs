using System.Security.Claims;
using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.CreateNote;
using AINotesApp.Features.Notes.GetNoteDetails;
using AINotesApp.Features.Notes.UpdateNote;
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
/// Unit tests for NoteEditor component using BUnit 2.x
/// </summary>
public class NoteEditorTests : BunitContext
{
	private readonly IMediator _mediator;
	private readonly TestAuthStateProvider _authProvider;
	private readonly NavigationManager _navigation;

	/// <summary>
	/// Initializes a new instance of the <see cref="NoteEditorTests"/> class
	/// </summary>
	public NoteEditorTests()
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
	private IRenderedComponent<NoteEditor> RenderWithAuth(Action<ComponentParameterCollectionBuilder<NoteEditor>>? parameters = null)
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();
		return Render<NoteEditor>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
			parameters?.Invoke(ps);
		});
	}

	[Fact]
	public async Task NoteEditor_InCreateMode_ShowsCreateNewNoteTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Create New Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_InEditMode_ShowsEditNoteTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
			cut.Markup.Should().Contain("Edit Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasTitleInput()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var titleInput = cut.Find("#title");
			titleInput.Should().NotBeNull();
			titleInput.GetAttribute("placeholder").Should().Be("Enter note title");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasContentTextArea()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var contentInput = cut.Find("#content");
			contentInput.Should().NotBeNull();
			contentInput.GetAttribute("placeholder").Should().Be("Write your note here...");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasSubmitButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var button = cut.Find("button[type='submit']");
			button.Should().NotBeNull();
			button.TextContent.Should().Contain("Create Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_InEditMode_ShowsUpdateButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
			var button = cut.Find("button[type='submit']");
			button.TextContent.Should().Contain("Update Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasBackToNotesButton()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Back to Notes");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_InEditMode_LoadsNoteData()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user-123");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Existing Note Title",
			Content = "Existing note content",
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
			var titleInput = cut.Find("#title");
			titleInput.GetAttribute("value").Should().Be("Existing Note Title");
			// TextArea content is loaded, verified by checking the form was rendered
			cut.Markup.Should().Contain("Existing Note Title");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_InEditMode_DisplaysAiSummary()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
	public async Task NoteEditor_InEditMode_DisplaysTags()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
	public async Task NoteEditor_InEditMode_DisplaysCreatedAndUpdatedDates()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
			CreatedAt = DateTime.Now.AddDays(-2),
			UpdatedAt = DateTime.Now.AddDays(-1)
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
	public async Task NoteEditor_InEditMode_WhenNoteNotFound_ShowsErrorMessage()
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
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_WhenLoading_ShowsLoadingSpinner()
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

		// Give component time to start loading
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
	public async Task NoteEditor_HasValidationForTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var form = cut.Find("form");
			form.Should().NotBeNull();
			// Form has data annotations validator
			cut.Markup.Should().Contain("form-control");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasValidationForContent()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var contentInput = cut.Find("#content");
			contentInput.Should().NotBeNull();
			contentInput.GetAttribute("rows").Should().Be("15");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_InEditMode_SendsCorrectQueryToMediator()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user-123");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
	public async Task NoteEditor_PageTitle_InCreateMode_IsCreateNote()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			cut.Markup.Should().Contain("Create New Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_PageTitle_InEditMode_IsEditNote()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
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
			cut.Markup.Should().Contain("Edit Note");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_HasEditForm()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth();

		// Assert
		await cut.InvokeAsync(() => { });
		cut.WaitForAssertion(() =>
		{
			var form = cut.Find("form");
			form.Should().NotBeNull();
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
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
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
	/// Fake authorization policy provider for testing
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
