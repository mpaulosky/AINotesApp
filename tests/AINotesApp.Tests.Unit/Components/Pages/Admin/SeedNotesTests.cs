// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SeedNotesTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages.Admin;
using AINotesApp.Features.Notes.BackfillTags;
using AINotesApp.Features.Notes.SeedNotes;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace AINotesApp.Tests.Unit.Components.Pages.Admin;

/// <summary>
///   BUnit tests for the Admin / SeedNotes page.
/// </summary>
[ExcludeFromCodeCoverage]
public class SeedNotesTests : BunitContext
{

    private readonly FakeAuthenticationStateProvider _authProvider = new();

    private readonly Mock<IMediator> _mediator = new();

	public SeedNotesTests()
	{
		Services.AddSingleton(_mediator.Object);
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
 }

 private static IRenderedComponent<SeedNotes> RenderAuthorized(BunitContext ctx, FakeAuthenticationStateProvider auth, string subject)
 {
     auth.SetAuthorized(subject);
     return ctx.Render<SeedNotes>();
 }

 private static void ChangeNoteCount(IRenderedComponent<SeedNotes> cut, int value)
 {
     var input = cut.Find("#noteCount");
     input.Change(value);
 }

 [Fact]
 public void StartSeeding_ShowsError_WhenNotLoggedIn()
 {
		// Arrange: not authenticated by default
		var cut = Render<SeedNotes>();

		// Act
		var button = cut.Find("button.btn.btn-primary.btn-lg");
		button.Click();

		// Assert
        cut.Markup.Should().Contain("You must be logged in to seed notes.");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartSeeding_Unauthenticated_DoesNotCallMediatorOrShowSpinner()
    {
        // Arrange
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        cut.Markup.Should().NotContain("Creating Notes with AI...");
        cut.Markup.Should().NotContain("spinner-border text-primary");
    }

    [Fact]
    public void StartBackfill_ShowsError_WhenNotLoggedIn()
    {
		// Arrange: not authenticated by default
		var cut = Render<SeedNotes>();

		// Act
		var button = cut.Find("button.btn.btn-success.btn-lg");
		button.Click();

		// Assert
        cut.Markup.Should().Contain("You must be logged in to backfill tags.");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartBackfilling_Unauthenticated_DoesNotCallMediatorOrShowSpinner()
    {
        // Arrange
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        cut.Markup.Should().NotContain("Generating Tags...");
        cut.Markup.Should().NotContain("spinner-border text-success");
    }

    [Fact]
    public void StartSeeding_ShowsSuccess_WhenMediatorCreatesNotes()
    {
		// Arrange
		_authProvider.SetAuthorized("user-123");

		   _mediator
			   .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
			   .ReturnsAsync(new SeedNotesResponse
			   {
				   CreatedCount = 2,
				   CreatedNoteIds = [Guid.NewGuid(), Guid.NewGuid()],
				   Errors = []
			   });

		   var cut = Render<SeedNotes>();

		   // Act
		   var button = cut.Find("button.btn.btn-primary.btn-lg");
		   button.Click();

		   // Assert
     cut.Markup.Should().Contain("Successfully created 2 notes with AI summaries, tags, and embeddings!");
       }

	   [Theory]
	   [InlineData(null, "You must be logged in to seed notes.")]
	   [InlineData("", "You must be logged in to seed notes.")]
	   [Trait("Category", "Bunit")]
	   public void StartSeeding_NotLoggedIn_ShowsError(string? userId, string expectedError)
	   {
		   // Arrange
		   if (!string.IsNullOrEmpty(userId))
			   _authProvider.SetAuthorized(userId);
		   var cut = Render<SeedNotes>();

		   // Act
		   var button = cut.Find("button.btn.btn-primary.btn-lg");
		   button.Click();

		   // Assert
		   cut.Markup.Should().Contain(expectedError);
	   }

	   [Fact]
	   [Trait("Category", "Bunit")]
    public void StartSeeding_Success_ShowsSuccessMessage()
    {
		   // Arrange
		   _authProvider.SetAuthorized("user-123");
		   _mediator
			   .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
			   .ReturnsAsync(new SeedNotesResponse
			   {
				   CreatedCount = 2,
				   CreatedNoteIds = [Guid.NewGuid(), Guid.NewGuid()],
				   Errors = []
			   });
		   var cut = Render<SeedNotes>();

		   // Act
		   var button = cut.Find("button.btn.btn-primary.btn-lg");
		   button.Click();

		   // Assert
           cut.Markup.Should().Contain("Successfully created 2 notes with AI summaries, tags, and embeddings!");
       }

    [Theory]
    [InlineData(50)]
    [InlineData(3)]
    [InlineData(1)]
    [Trait("Category", "Bunit")]
    public void StartSeeding_UsesNoteCountFromInput_AndSendsCommand(int count)
    {
        // Arrange
        _authProvider.SetAuthorized("sub-abc");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse { CreatedCount = 1, CreatedNoteIds = [Guid.NewGuid()], Errors = [] });

        var cut = Render<SeedNotes>();

        if (count != 50)
        {
            ChangeNoteCount(cut, count);
        }

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        _mediator.Verify(m => m.Send(It.Is<SeedNotesCommand>(c => c.Count == count && c.UserSubject == "sub-abc"), It.IsAny<CancellationToken>()), Times.Once);
        cut.Markup.Should().Contain("Successfully created 1 notes");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartSeeding_Success_WithErrors_RendersErrorList()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse
            {
                CreatedCount = 2,
                CreatedNoteIds = [Guid.NewGuid(), Guid.NewGuid()],
                Errors = ["err1", "err2"]
            });

        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        cut.Markup.Should().Contain("Successfully created 2 notes");
        cut.Markup.Should().Contain("Some notes failed to create:");
        cut.Markup.Should().Contain("err1");
        cut.Markup.Should().Contain("err2");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartSeeding_ZeroCreated_ShowsApiKeyError()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse { CreatedCount = 0, CreatedNoteIds = [], Errors = [] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        cut.Markup.Should().Contain("Failed to create any notes. Please check your OpenAI API key.");
        cut.Markup.Should().NotContain("Successfully created");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartSeeding_Exception_ShowsErrorAndHidesSpinner()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Error seeding notes: boom"));
        cut.Markup.Should().NotContain("spinner-border text-primary");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void Reset_AfterSeedingSuccess_ReturnsToInitialForm()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse { CreatedCount = 1, CreatedNoteIds = [Guid.NewGuid()], Errors = [] });
        var cut = Render<SeedNotes>();

        // Act: seed -> success then click Reset
        cut.Find("button.btn.btn-primary.btn-lg").Click();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Successfully created 1 notes"));
        cut.Find("button.btn.btn-outline-secondary").Click();

        // Assert: initial form visible again
        cut.Markup.Should().Contain("Number of Notes to Create");
        cut.Markup.Should().Contain("Start Seeding Notes");
        cut.Markup.Should().NotContain("Successfully created");
        cut.Markup.Should().NotContain("Failed to create");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void Reset_AfterSeedingError_ReturnsToInitialForm()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse { CreatedCount = 0, CreatedNoteIds = [], Errors = [] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Failed to create any notes."));
        cut.Find("button.btn.btn-outline-secondary").Click();

        // Assert
        cut.Markup.Should().Contain("Number of Notes to Create");
        cut.Markup.Should().Contain("Start Seeding Notes");
        cut.Markup.Should().NotContain("Failed to create any notes.");
    }

		    [Theory]
   [InlineData(null, "You must be logged in to backfill tags.")]
   [InlineData("", "You must be logged in to backfill tags.")]
   [Trait("Category", "Bunit")]
   public void StartBackfilling_NotLoggedIn_ShowsError(string? userId, string expectedError)
   {
	   // Arrange
	   if (!string.IsNullOrEmpty(userId))
		   _authProvider.SetAuthorized(userId);
	   var cut = Render<SeedNotes>();

	   // Act
	   var button = cut.Find("button.btn.btn-success.btn-lg");
	   button.Click();

	   // Assert
	   cut.Markup.Should().Contain(expectedError);
   }

   [Fact]
   [Trait("Category", "Bunit")]
   public void StartBackfilling_Success_ShowsSuccessMessage()
   {
	   // Arrange
	   _authProvider.SetAuthorized("user-123");
	   _mediator
		   .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
		   .ReturnsAsync(new BackfillTagsResponse
		   {
			   ProcessedCount = 3,
			   TotalNotes = 3,
			   Errors = []
		   });
	   var cut = Render<SeedNotes>();

	   // Act
	   var button = cut.Find("button.btn.btn-success.btn-lg");
	   button.Click();

	   // Assert
    cut.Markup.Should().Contain("Successfully generated tags for 3 notes!");
   }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartBackfilling_SendsCommandWithSubjectAndOnlyMissing()
    {
        // Arrange
        _authProvider.SetAuthorized("sub-xyz");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackfillTagsResponse { ProcessedCount = 1, TotalNotes = 1, Errors = [] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();

        // Assert
        _mediator.Verify(m => m.Send(It.Is<BackfillTagsCommand>(c => c.UserSubject == "sub-xyz" && c.OnlyMissing), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartBackfilling_Success_WithErrors_RendersErrorList()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackfillTagsResponse { ProcessedCount = 2, TotalNotes = 2, Errors = ["b1", "b2"] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();

        // Assert
        cut.Markup.Should().Contain("Successfully generated tags for 2 notes!");
        cut.Markup.Should().Contain("Some notes failed:");
        cut.Markup.Should().Contain("b1");
        cut.Markup.Should().Contain("b2");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartBackfilling_ZeroProcessed_ShowsAllTaggedMessage()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackfillTagsResponse { ProcessedCount = 0, TotalNotes = 5, Errors = [] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();

        // Assert
        cut.Markup.Should().Contain("All notes already have tags!");
        cut.Markup.Should().NotContain("Error backfilling tags:");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void StartBackfilling_Exception_ShowsErrorAndHidesSpinner()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("oops"));
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();

        // Assert
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Error backfilling tags: oops"));
        cut.Markup.Should().NotContain("spinner-border text-success");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void ResetBackfill_FromSuccess_ReturnsToInitial()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackfillTagsResponse { ProcessedCount = 1, TotalNotes = 1, Errors = [] });
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Successfully generated tags for 1 notes!"));
        cut.Find("button.btn.btn-outline-secondary").Click();

        // Assert
        cut.Markup.Should().Contain("Generate Tags for Existing Notes");
        cut.Markup.Should().NotContain("Successfully generated tags");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void ResetBackfill_FromError_ReturnsToInitial()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bad"));
        var cut = Render<SeedNotes>();

        // Act
        cut.Find("button.btn.btn-success.btn-lg").Click();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Error backfilling tags:"));
        cut.Find("button.btn.btn-outline-secondary").Click();

        // Assert
        cut.Markup.Should().Contain("Generate Tags for Existing Notes");
        cut.Markup.Should().NotContain("Error backfilling tags:");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void InitialRender_ShowsDefaultNoteCountAndSections()
    {
        // Arrange & Act
        var cut = Render<SeedNotes>();

        // Assert default value and attributes
        var input = cut.Find("#noteCount");
        input.GetAttribute("value").Should().Be("50");
        input.GetAttribute("min").Should().Be("1");
        input.GetAttribute("max").Should().Be("50");

        // Important static sections
        cut.Markup.Should().Contain("Seed Demo Notes");
        cut.Markup.Should().Contain("Backfill Tags");
        cut.Markup.Should().Contain("Sample Topics Included");
    }

    [Fact]
    [Trait("Category", "Bunit")]
    public void Authentication_SubjectClaim_IsUsedInCommands()
    {
        // Arrange
        var subject = "subject-123";
        var cut = RenderAuthorized(this, _authProvider, subject);

        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse { CreatedCount = 1, CreatedNoteIds = [Guid.NewGuid()], Errors = [] });

        // Act
        cut.Find("button.btn.btn-primary.btn-lg").Click();

        // Assert
        _mediator.Verify(m => m.Send(It.Is<SeedNotesCommand>(c => c.UserSubject == subject), It.IsAny<CancellationToken>()), Times.Once);
    }
	 
}