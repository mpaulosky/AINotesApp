// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ErrorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages;

using Bunit;

using FluentAssertions;

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