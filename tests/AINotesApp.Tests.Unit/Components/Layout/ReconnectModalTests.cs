using AINotesApp.Components.Layout;
using Bunit;
using FluentAssertions;

namespace AINotesApp.Tests.Unit.Components.Layout;

/// <summary>
/// Unit tests for ReconnectModal component using BUnit 2.x
/// </summary>
public class ReconnectModalTests : BunitContext
{
	[Fact]
	public void ReconnectModal_ShouldRender_WithoutErrors()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ReconnectModal_ShouldHave_DialogElement()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var dialog = cut.Find("dialog#components-reconnect-modal");
		dialog.Should().NotBeNull();
		dialog.GetAttribute("data-nosnippet").Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldHave_RejoiningAnimationDiv()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var animation = cut.Find(".components-rejoining-animation");
		animation.Should().NotBeNull();
		animation.GetAttribute("aria-hidden").Should().Be("true");
	}

	[Fact]
	public void ReconnectModal_ShouldHave_AnimationDivs()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var animationDivs = cut.FindAll(".components-rejoining-animation > div");
		animationDivs.Should().HaveCount(2);
	}

	[Fact]
	public void ReconnectModal_ShouldDisplay_FirstAttemptMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find(".components-reconnect-first-attempt-visible");
		message.Should().NotBeNull();
		message.TextContent.Trim().Should().Be("Rejoining the server...");
	}

	[Fact]
	public void ReconnectModal_ShouldDisplay_RepeatedAttemptMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find(".components-reconnect-repeated-attempt-visible");
		message.Should().NotBeNull();
		message.TextContent.Should().Contain("Rejoin failed... trying again in");
		message.TextContent.Should().Contain("seconds.");
	}

	[Fact]
	public void ReconnectModal_ShouldHave_SecondsToNextAttemptSpan()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var span = cut.Find("#components-seconds-to-next-attempt");
		span.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldDisplay_ReconnectFailedMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find(".components-reconnect-failed-visible");
		message.Should().NotBeNull();
		message.InnerHtml.Should().Contain("Failed to rejoin.");
		message.InnerHtml.Should().Contain("Please retry or reload the page.");
	}

	[Fact]
	public void ReconnectModal_ShouldHave_RetryButton()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var button = cut.Find("#components-reconnect-button");
		button.Should().NotBeNull();
		button.TextContent.Trim().Should().Be("Retry");
		button.ClassList.Should().Contain("components-reconnect-failed-visible");
	}

	[Fact]
	public void ReconnectModal_ShouldDisplay_PauseMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find(".components-pause-visible");
		message.Should().NotBeNull();
		message.TextContent.Trim().Should().Be("The session has been paused by the server.");
	}

	[Fact]
	public void ReconnectModal_ShouldHave_ResumeButton()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var button = cut.Find("#components-resume-button");
		button.Should().NotBeNull();
		button.TextContent.Trim().Should().Be("Resume");
		button.ClassList.Should().Contain("components-pause-visible");
	}

	[Fact]
	public void ReconnectModal_ShouldDisplay_ResumeFailedMessage()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var message = cut.Find(".components-resume-failed-visible");
		message.Should().NotBeNull();
		message.InnerHtml.Should().Contain("Failed to resume the session.");
		message.InnerHtml.Should().Contain("Please reload the page.");
	}

	[Fact]
	public void ReconnectModal_ShouldHave_Container()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var container = cut.Find(".components-reconnect-container");
		container.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_ShouldInclude_JavaScriptModule()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var script = cut.Find("script[type='module']");
		script.Should().NotBeNull();
		script.GetAttribute("src").Should().Contain("ReconnectModal.razor.js");
	}

	[Fact]
	public void ReconnectModal_AllMessages_ArePresent()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		cut.Markup.Should().Contain("Rejoining the server...");
		cut.Markup.Should().Contain("Rejoin failed... trying again in");
		cut.Markup.Should().Contain("Failed to rejoin.");
		cut.Markup.Should().Contain("The session has been paused by the server.");
		cut.Markup.Should().Contain("Failed to resume the session.");
	}

	[Fact]
	public void ReconnectModal_AllButtons_ArePresent()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().HaveCount(2);

		var retryButton = cut.Find("#components-reconnect-button");
		var resumeButton = cut.Find("#components-resume-button");

		retryButton.Should().NotBeNull();
		resumeButton.Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_Dialog_HasCorrectId()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var dialog = cut.Find("dialog");
		dialog.GetAttribute("id").Should().Be("components-reconnect-modal");
	}

	[Fact]
	public void ReconnectModal_HasCorrectStructure()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		var dialog = cut.Find("dialog#components-reconnect-modal");
		var container = cut.Find(".components-reconnect-container");
		var animation = cut.Find(".components-rejoining-animation");

		dialog.Should().NotBeNull();
		container.Should().NotBeNull();
		animation.Should().NotBeNull();

		// Verify hierarchy
		dialog.QuerySelector(".components-reconnect-container").Should().NotBeNull();
		container.QuerySelector(".components-rejoining-animation").Should().NotBeNull();
	}

	[Fact]
	public void ReconnectModal_AllCssClasses_ArePresent()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		cut.Markup.Should().Contain("components-reconnect-first-attempt-visible");
		cut.Markup.Should().Contain("components-reconnect-repeated-attempt-visible");
		cut.Markup.Should().Contain("components-reconnect-failed-visible");
		cut.Markup.Should().Contain("components-pause-visible");
		cut.Markup.Should().Contain("components-resume-failed-visible");
	}

	[Fact]
	public void ReconnectModal_AllRequiredIds_ArePresent()
	{
		// Arrange & Act
		var cut = Render<ReconnectModal>();

		// Assert
		cut.Find("#components-reconnect-modal").Should().NotBeNull();
		cut.Find("#components-seconds-to-next-attempt").Should().NotBeNull();
		cut.Find("#components-reconnect-button").Should().NotBeNull();
		cut.Find("#components-resume-button").Should().NotBeNull();
	}
}
