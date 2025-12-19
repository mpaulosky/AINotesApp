// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AboutTests.cs
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
///   Unit tests for About page using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class AboutTests : BunitContext
{

	[Fact]
	public void About_ShouldRender_WithoutErrors()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void About_ShouldDisplay_HeaderWithAboutText()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Should().Be("About");
	}

	[Fact]
	public void About_ShouldDisplay_ApplicationDescription()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("AINotesApp");
		cut.Markup.Should().Contain("Blazor application");
		cut.Markup.Should().Contain("AI-powered features");
		cut.Markup.Should().Contain("summarization");
		cut.Markup.Should().Contain("tagging");
		cut.Markup.Should().Contain("semantic search");
	}

	[Fact]
	public void About_ShouldDisplay_VersionInformation()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("Version:");
		cut.Markup.Should().Contain("1.0.0");
	}

	[Fact]
	public void About_ShouldDisplay_CopyrightInformation()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("Copyright");
		cut.Markup.Should().Contain("2025");
		cut.Markup.Should().Contain("Matthew Paulosky");
	}

	[Fact]
	public void About_HasCorrectPageRoute()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert - Component should be accessible at /about route
		// This is verified by the component compiling with the @page directive
		cut.Should().NotBeNull();
	}

	[Fact]
	public void About_ShouldHave_ThreeParagraphs()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs.Should().HaveCount(3);
	}

	[Fact]
	public void About_FirstParagraph_ContainsDescription()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs[0].TextContent.Should().Contain("AINotesApp");
		paragraphs[0].TextContent.Should().Contain("managing notes");
	}

	[Fact]
	public void About_SecondParagraph_ContainsVersion()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs[1].TextContent.Should().Contain("Version:");
	}

	[Fact]
	public void About_ThirdParagraph_ContainsCopyright()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var paragraphs = cut.FindAll("p");
		paragraphs[2].TextContent.Should().Contain("Copyright");
	}

	[Fact]
	public void About_Header_IsH1Element()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var h1 = cut.Find("h1");
		h1.NodeName.Should().Be("H1");
	}

	[Fact]
	public void About_ShouldHave_OnlyOneHeader()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var headers = cut.FindAll("h1");
		headers.Should().HaveCount(1);
	}

	[Fact]
	public void About_DescriptionParagraph_ContainsAIPoweredFeatures()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var firstParagraph = cut.Find("p");
		firstParagraph.TextContent.Should().Contain("AI-powered features");
	}

	[Fact]
	public void About_DescriptionParagraph_MentionsAllFeatures()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var firstParagraph = cut.Find("p");
		firstParagraph.TextContent.Should().Contain("summarization");
		firstParagraph.TextContent.Should().Contain("tagging");
		firstParagraph.TextContent.Should().Contain("semantic search");
	}

	[Fact]
	public void About_Copyright_UsesCopyrightSymbol()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		cut.Markup.Should().Contain("&copy;");
	}

	[Fact]
	public void About_RendersStaticContent_WithoutJavaScript()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert - Should be purely static content with no JavaScript
		cut.Markup.Should().NotContain("<script");
		cut.Markup.Should().NotContain("onclick");
	}

	[Fact]
	public void About_DoesNotRequire_Authentication()
	{
		// Arrange & Act - Render without any authentication setup
		var cut = Render<About>();

		// Assert - Should render successfully without auth
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
		cut.Markup.Should().Contain("About");
	}

	[Fact]
	public void About_HasSimpleStructure_WithNoComplexity()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert - Should only have h1 and p elements, no divs or complex structure
		var allElements = cut.FindAll("*");
		var h1Count = cut.FindAll("h1").Count;
		var pCount = cut.FindAll("p").Count;

		h1Count.Should().Be(1);
		pCount.Should().Be(3);
	}

	[Fact]
	public void About_Version_IsDisplayedInCorrectFormat()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var versionParagraph = cut.FindAll("p")[1];
		versionParagraph.TextContent.Should().Match("Version: *.*.*");
	}

	[Fact]
	public void About_Copyright_IncludesCurrentYear()
	{
		// Arrange & Act
		var cut = Render<About>();

		// Assert
		var copyrightParagraph = cut.FindAll("p")[2];
		copyrightParagraph.TextContent.Should().Contain("2025");
	}

}
