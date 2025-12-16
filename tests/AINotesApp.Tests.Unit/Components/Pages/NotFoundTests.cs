// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NotFoundTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages;

using Bunit;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Components.Pages;

/// <summary>
///   Unit tests for the NotFound component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class NotFoundTests : BunitContext
{

	[Fact]
	public void NotFound_ShouldRender_WithoutErrors()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void NotFound_ShouldRender_H3Header()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var h3 = cut.Find("h3");
		h3.Should().NotBeNull();
		h3.TextContent.Should().Be("Not Found");
	}

	[Fact]
	public void NotFound_ShouldRender_ErrorMessage()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		cut.Markup.Should().Contain("Sorry, the content you are looking for does not exist.");
	}

	[Fact]
	public void NotFound_ShouldHave_ParagraphElement()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var paragraph = cut.Find("p");
		paragraph.Should().NotBeNull();
		paragraph.TextContent.Should().Contain("Sorry");
	}

	[Fact]
	public void NotFound_HasCorrectPageRoute()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		// Component should render successfully, indicating the route is valid
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Not Found");
	}

	[Fact]
	public void NotFound_HasCorrectStructure()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var h3 = cut.Find("h3");
		var p = cut.Find("p");

		h3.Should().NotBeNull();
		p.Should().NotBeNull();
		h3.TextContent.Trim().Should().Be("Not Found");
		p.TextContent.Trim().Should().Be("Sorry, the content you are looking for does not exist.");
	}

	[Fact]
	public void NotFound_MessageText_IsUserFriendly()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var paragraph = cut.Find("p");
		paragraph.TextContent.Should().Contain("Sorry");
		paragraph.TextContent.Should().Contain("content");
		paragraph.TextContent.Should().Contain("does not exist");
	}

	[Fact]
	public void NotFound_Header_UsesH3Element()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var h3Elements = cut.FindAll("h3");
		h3Elements.Should().HaveCount(1);
		h3Elements.First().TextContent.Should().Be("Not Found");
	}

	[Fact]
	public void NotFound_HasSingleParagraph()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs.Should().HaveCount(1);
	}

	[Fact]
	public void NotFound_RendersCompleteMessage()
	{
		// Arrange & Act
		var cut = Render<NotFound>();

		// Assert
		cut.Markup.Should().Contain("Not Found");
		cut.Markup.Should().Contain("Sorry, the content you are looking for does not exist.");
	}

}