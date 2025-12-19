using System.Diagnostics.CodeAnalysis;

using Bunit;
using FluentAssertions;
using Xunit;

namespace AINotesApp.Tests.Unit.Components;

/// <summary>
/// Tests for the RedirectToLogin component which redirects to the MVC Account/Login endpoint.
/// Note: This component navigates immediately in OnInitialized, making traditional rendering tests challenging.
/// </summary>
[ExcludeFromCodeCoverage]
public class RedirectToLoginTests : BunitContext
{
	[Fact]
	public void RedirectToLogin_RendersWithoutErrors()
	{
		// Arrange & Act
		var act = () => Render<AINotesApp.Components.RedirectToLogin>();

		// Assert
		act.Should().NotThrow("component should render and navigate without throwing exceptions");
	}

	[Fact]
	public void RedirectToLogin_HasNoVisibleMarkup()
	{
		// Arrange & Act
		var cut = Render<AINotesApp.Components.RedirectToLogin>();

		// Assert
		// Component has no HTML content - it just navigates
		cut.Markup.Trim().Should().BeEmpty("component should not render any HTML, only navigate");
	}

	[Fact]
	public void RedirectToLogin_DoesNotRequireAuthentication()
	{
		// Arrange & Act
		// Render without any authentication context
		var cut = Render<AINotesApp.Components.RedirectToLogin>();

		// Assert
		cut.Should().NotBeNull("component should work without authentication");
		cut.Markup.Trim().Should().BeEmpty();
	}

	[Fact]
	public void RedirectToLogin_MultipleInstances_RenderIndependently()
	{
		// Arrange & Act
		var cut1 = Render<AINotesApp.Components.RedirectToLogin>();
		var cut2 = Render<AINotesApp.Components.RedirectToLogin>();

		// Assert
		cut1.Should().NotBeNull();
		cut2.Should().NotBeNull();
		cut1.Markup.Trim().Should().BeEmpty();
		cut2.Markup.Trim().Should().BeEmpty();
	}
}
