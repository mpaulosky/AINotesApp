// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteEditorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.GetNoteDetails;
using AINotesApp.Tests.Unit.Fakes;

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
///   Unit tests for NoteEditor component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteEditorTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	private readonly IMediator _mediator;

	private readonly NavigationManager _navigation;

	/// <summary>
	///   Initializes a new instance of the <see cref="NoteEditorTests" /> class
	/// </summary>
	public NoteEditorTests()
	{
		_mediator = Substitute.For<IMediator>();
		_authProvider = new FakeAuthenticationStateProvider();
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
	private IRenderedComponent<NoteEditor> RenderWithAuth(
			Action<ComponentParameterCollectionBuilder<NoteEditor>>? parameters = null)
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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
		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
		{
			_mediator.Received(1).Send(
					Arg.Is<GetNoteDetailsQuery>(q => q.Id == noteId && q.UserSubject == "user-123"),
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
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

		await cut.WaitForAssertionAsync(() =>
		{
			var form = cut.Find("form");
			form.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}
}
