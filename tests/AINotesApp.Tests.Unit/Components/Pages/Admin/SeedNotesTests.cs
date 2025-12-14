using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using AINotesApp.Components.Pages.Admin;
using AINotesApp.Features.Notes.BackfillTags;
using AINotesApp.Features.Notes.SeedNotes;
using Bunit;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AINotesApp.Tests.Unit.Components.Pages.Admin;

/// <summary>
/// BUnit tests for Admin/SeedNotes page.
/// </summary>
[ExcludeFromCodeCoverage]
public class SeedNotesTests : BunitContext
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly TestAuthStateProvider _authProvider = new();

    public SeedNotesTests()
    {
        Services.AddSingleton<IMediator>(_mediator.Object);
        Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
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
    public void StartSeeding_ShowsSuccess_WhenMediatorCreatesNotes()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<SeedNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SeedNotesResponse
            {
                CreatedCount = 2,
                CreatedNoteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                Errors = new List<string>()
            });

        var cut = Render<SeedNotes>();

        // Act
        var button = cut.Find("button.btn.btn-primary.btn-lg");
        button.Click();

        // Assert
        cut.Markup.Should().Contain("Successfully created 2 notes with AI summaries, tags, and embeddings!");
    }

    [Fact]
    public void StartBackfilling_ShowsSuccess_WhenMediatorProcessesNotes()
    {
        // Arrange
        _authProvider.SetAuthorized("user-123");
        _mediator
            .Setup(m => m.Send(It.IsAny<BackfillTagsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackfillTagsResponse
            {
                ProcessedCount = 3,
                TotalNotes = 3,
                Errors = new List<string>()
            });

        var cut = Render<SeedNotes>();

        // Act
        var button = cut.Find("button.btn.btn-success.btn-lg");
        button.Click();

        // Assert
        cut.Markup.Should().Contain("Successfully generated tags for 3 notes!");
    }

    private class TestAuthStateProvider : AuthenticationStateProvider
    {
        private Task<AuthenticationState> _state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => _state;

        public void SetAuthorized(string userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "Test");
            _state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
            NotifyAuthenticationStateChanged(_state);
        }
    }
}