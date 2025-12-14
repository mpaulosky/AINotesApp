using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AINotesApp.Components.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace AINotesApp.Tests.Unit.Components.Pages;

[ExcludeFromCodeCoverage]
public class ErrorTests : BunitContext
{
    [Fact]
    public void Renders_NoRequestId_WhenNotAvailable()
    {
        // Arrange
        Activity.Current = null;
        // Act
        var cut = Render<Error>();

        // Assert
        cut.Markup.Should().Contain("An error occurred while processing your request.");
        cut.Markup.Should().NotContain("Request ID:");
    }

    [Fact]
    public void Renders_RequestId_WhenAvailableFromActivity()
    {
        // Arrange
        var activity = new Activity("test");
        activity.Start();
        try
        {
            // Act
            var cut = Render<Error>();

            // Assert
            cut.Markup.Should().Contain("Request ID:");
        }
        finally
        {
            activity.Stop();
            Activity.Current = null;
        }
    }
}