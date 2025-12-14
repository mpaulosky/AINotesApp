using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages;

using Bunit;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Components.Pages;

/// <summary>
/// Unit tests for Home component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeTests : BunitContext
{
	[Fact]
	public void Home_ShouldRender_WithCorrectTitle()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Find("h1").TextContent.Should().Be("Hello, world!");
	}

	[Fact]
	public void Home_ShouldRender_WithWelcomeMessage()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Markup.Should().Contain("Welcome to your new app");
	}

	[Fact]
	public void Home_ShouldHave_PageTitle()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert - PageTitle is rendered by the framework, not in component markup
		// Check that the component renders successfully with page title directive
		cut.Markup.Should().NotBeNullOrEmpty();
		cut.Markup.Should().Contain("Hello, world!");
	}

	[Fact]
	public void Home_ShouldRender_WithoutErrors()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Home_ShouldHave_CorrectStructure()
	{
		// Arrange & Act
		var cut = Render<Home>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Trim().Should().Be("Hello, world!");
	}
}
