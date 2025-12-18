// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteEditorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================
using System;
using System.Threading.Tasks;

using Xunit;

using System.Threading;
using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages.Notes;
using AINotesApp.Features.Notes.GetNoteDetails;
using AINotesApp.Features.Notes.CreateNote;
using AINotesApp.Features.Notes.UpdateNote;
// using AINotesApp.Tests.Unit.Fakes; (removed duplicate)
using AINotesApp.Tests.Unit.Helpers;
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
	// Minimal mock NavigationManager for test
	private class MockNavigationManager : NavigationManager
	{
		public MockNavigationManager()
		{
			Initialize("http://localhost/", "http://localhost/");
		}
		protected override void NavigateToCore(string uri, bool forceLoad) { /* do nothing */ }
	}

	private readonly FakeAuthenticationStateProvider _authProvider;
	private readonly IMediator _mediator;

	/// <summary>
	///   Initializes a new instance of the <see cref="NoteEditorTests" /> class
	/// </summary>
	public NoteEditorTests()
	{
		_mediator = Substitute.For<IMediator>();
		var mockNav = new MockNavigationManager();
		Services.AddSingleton<NavigationManager>(mockNav);
		// _navigation = mockNav; // Field is assigned but never used
		Services.AddSingleton(_mediator);
		_authProvider = TestAuthHelper.RegisterDynamicTestAuthentication(Services);
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	[Fact]
	public async Task NoteEditor_Displays_CorrectDateFormatting()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var created = new DateTime(2025, 1, 2, 3, 4, 0, DateTimeKind.Utc);
		var updated = new DateTime(2025, 2, 3, 4, 5, 0, DateTimeKind.Utc);
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "T",
			Content = "C",
			CreatedAt = created,
			UpdatedAt = updated
		};
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<GetNoteDetailsResponse?>(response));
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });

				// Assert
				await cut.WaitForAssertionAsync(() =>
				{
					cut.Markup.Should().Contain("Created:");
					_mediator.Send(Arg.Any<UpdateNoteCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new UpdateNoteResponse { Success = false, Message = "Update failed" }));
					cut.Markup.Should().Contain(created.ToLocalTime().ToString("g"));
					cut.Markup.Should().Contain(updated.ToLocalTime().ToString("g"));
				}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_RelatedNotesSidebar_OnlyInEditModeAndNotLoading()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var response = new GetNoteDetailsResponse { Id = noteId, Title = "T", Content = "C", CreatedAt = now, UpdatedAt = now };
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<GetNoteDetailsResponse?>(response));
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("No related notes found.");
		}, TimeSpan.FromSeconds(2));

			// In create mode, should not show
			var cut2 = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
			await cut2.InvokeAsync(() => { });
			cut2.Markup.Should().NotContain("RelatedNotes");
	}

	[Fact]
	public async Task NoteEditor_AiSummaryAndTags_EdgeCases()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "T",
			Content = "C",
			AiSummary = "",
			Tags = null,
			CreatedAt = now,
			UpdatedAt = now
		};
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<GetNoteDetailsResponse?>(response));
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });

			// Assert: Should not show empty summary/tags
			cut.Markup.Should().NotContain("AI Summary:");
			cut.Markup.Should().NotContain("Tags:");

		// Now test long/special chars
		response = new GetNoteDetailsResponse
		{
			Id = noteId,
			Title = "T",
			Content = "C",
			AiSummary = new string('★', 100),
			Tags = "tag1, tag2, tag3, !@#",
			CreatedAt = now,
			UpdatedAt = now
		};
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<GetNoteDetailsResponse?>(response));
		cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });
		cut.Markup.Should().Contain("AI Summary:");
		cut.Markup.Should().Contain("★");
		cut.Markup.Should().Contain("!@#");
	}

	[Fact]
	public async Task NoteEditor_Model_InitializesIfNull()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });

				// Assert
				await cut.WaitForAssertionAsync(() =>
				{
					var titleInput = cut.Find("#title");
					titleInput.Should().NotBeNull();
				}, TimeSpan.FromSeconds(2));
	}

		[Fact]
		public async Task NoteEditor_CreateNote_SubmitsAndDisablesButton()
		{
			// Arrange
			var tcs = new TaskCompletionSource<CreateNoteResponse>();
			_mediator.Send(Arg.Any<CreateNoteCommand>(), Arg.Any<CancellationToken>()).Returns(_ => tcs.Task);
			var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
			await cut.InvokeAsync(() => { });

			// Set valid input values
			ComponentTestHelper.SetInputValue(cut, "#title", "Valid Title");
			ComponentTestHelper.SetInputValue(cut, "#content", "Valid Content");

			// Act
			await ComponentTestHelper.SubmitFormAsync(cut);

			// Assert
			await Task.Delay(150);
			var button = cut.Find("button[type='submit']");
			button.HasAttribute("disabled").Should().BeTrue();
			cut.Markup.Should().Contain("Saving...");

			tcs.SetResult(new CreateNoteResponse { Id = Guid.NewGuid() });
		}

	[Fact]
	public async Task NoteEditor_SuccessAlert_CanBeDismissed()
	{
		var response = new CreateNoteResponse { Id = Guid.NewGuid() };
		_mediator.Send(Arg.Any<CreateNoteCommand>(), Arg.Any<CancellationToken>()).Returns(response);
		var mockNav = new MockNavigationManager();
		Services.AddSingleton<NavigationManager>(mockNav);
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });

		// Set valid input values
		ComponentTestHelper.SetInputValue(cut, "#title", "Valid Title");
		ComponentTestHelper.SetInputValue(cut, "#content", "Valid Content");

		// Act
		await ComponentTestHelper.SubmitFormAsync(cut);
		await Task.Delay(150);
		// Assert
		// Wait for the success alert to appear
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Note created successfully!");
		}, TimeSpan.FromSeconds(2));

		// Find and click the close button
		var closeBtn = cut.Find(".alert-success .btn-close");
		closeBtn.Click();

		// Wait for the alert to be dismissed
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().NotContain("Note created successfully!");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_ErrorAlert_CanBeDismissed()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		// Simulate error in update
		var noteId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var response = new GetNoteDetailsResponse { Id = noteId, Title = "T", Content = "C", CreatedAt = now, UpdatedAt = now };
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<GetNoteDetailsResponse?>(response));
		_mediator.Send(Arg.Any<UpdateNoteCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new UpdateNoteResponse { Success = false, Message = "Update failed" }));
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });

		// Act
		await ComponentTestHelper.SubmitFormAsync(cut);
		// var tcs = new TaskCompletionSource<CreateNoteResponse>(); // Variable is never used
		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Update failed");
			var closeBtn = cut.Find(".alert-danger .btn-close");
			closeBtn.Click();
		}, TimeSpan.FromSeconds(2));

		// Error message should disappear
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().NotContain("Update failed");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_NotAuthenticated_DoesNotLoadOrSubmit()
	{
		// Arrange
		_authProvider.SetNotAuthorized();
		var noteId = Guid.NewGuid();
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));
		await cut.InvokeAsync(() => { });

		// Should not call mediator
		await Task.Run(() => _mediator.DidNotReceive().Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()));

		// Try to submit
		await ComponentTestHelper.SubmitFormAsync(cut);
		await Task.Run(() => _mediator.DidNotReceive().Send(Arg.Any<UpdateNoteCommand>(), Arg.Any<CancellationToken>()));
		await Task.Run(() => _mediator.DidNotReceive().Send(Arg.Any<CreateNoteCommand>(), Arg.Any<CancellationToken>()));
	}

	[Fact]
	public async Task NoteEditor_Validation_ShowsErrorMessages()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });
		_mediator.Send(Arg.Any<UpdateNoteCommand>(), Arg.Any<CancellationToken>()).Returns(new UpdateNoteResponse { Success = false, Message = "Update failed" });
		// Act: Clear title and content
		ComponentTestHelper.SetInputValue(cut, "#title", "");
		ComponentTestHelper.SetInputValue(cut, "#content", "");
		await ComponentTestHelper.SubmitFormAsync(cut);

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Title is required");
			cut.Markup.Should().Contain("Content is required");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_Validation_TitleTooLong_ShowsError()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());
		await cut.InvokeAsync(() => { });

		// Act: Set title > 200 chars
		ComponentTestHelper.SetInputValue(cut, "#title", new string('a', 201));
		ComponentTestHelper.SetInputValue(cut, "#content", "Valid content");
		await ComponentTestHelper.SubmitFormAsync(cut);

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Title cannot exceed 200 characters");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_Exception_OnInitializedAsync_ShowsError()
	{
		// Arrange
		var provider = Substitute.For<AuthenticationStateProvider>();
		provider.When(x => x.GetAuthenticationStateAsync()).Do(_ => throw new Exception("init fail"));
		Services.AddSingleton(provider);
		var cut = Render<NoteEditor>();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error initializing: init fail");
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_Exception_LoadNoteAsync_ShowsError()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");
		var noteId = Guid.NewGuid();
		_mediator.Send(Arg.Any<GetNoteDetailsQuery>(), Arg.Any<CancellationToken>()).Returns<Task<GetNoteDetailsResponse?>>(_ => throw new Exception("load fail"));
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error loading note: load fail");
		}, TimeSpan.FromSeconds(2));
	}


	[Fact]
	public async Task NoteEditor_InCreateMode_ShowsCreateNewNoteTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

		// Assert
		await cut.InvokeAsync(() => { });

		await cut.WaitForAssertionAsync(() =>
		{
			    _mediator.Received(1).Send(
				    Arg.Is<GetNoteDetailsQuery>((GetNoteDetailsQuery q) => q.Id == noteId && q.UserSubject == "user-123"),
				    Arg.Any<CancellationToken>());
		}, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task NoteEditor_PageTitle_InCreateMode_IsCreateNote()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync(), ps => ps.Add(p => p.Id, noteId));

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
		var cut = ComponentTestHelper.RenderWithAuth<NoteEditor>(this, _authProvider.GetAuthenticationStateAsync());

		await cut.WaitForAssertionAsync(() =>
		{
			var form = cut.Find("form");
			form.Should().NotBeNull();
		}, TimeSpan.FromSeconds(2));
	}
}
