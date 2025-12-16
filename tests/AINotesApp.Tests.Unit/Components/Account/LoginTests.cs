// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     LoginTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Account;

using Bunit;
using Bunit.TestDoubles;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Components.Account;

/// <summary>
///   Unit tests for the Login component.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginTests : BunitContext
{

	/// <summary>
	///   Verifies that the Login component renders both Sign In and Sign Up buttons with correct text and styles.
	/// </summary>
	[Fact]
	public void Login_ShouldRender_WithSignInAndSignUpButtons()
	{
		// Arrange & Act
		var cut = Render<Login>();

		// Assert
		cut.Find("button.btn-primary").TextContent.Should().Be("Sign in");
		cut.Find("button.btn-link").TextContent.Should().Be("Create account");
	}

	/// <summary>
	///   Verifies that clicking the Sign In button navigates to the Auth0 login page without a screen hint.
	/// </summary>
	[Fact]
	public void SignInButton_WhenClicked_ShouldNavigateToAuthLogin()
	{
		// Arrange
		var cut = Render<Login>();

		// Act
		cut.Find("button.btn-primary").Click();

		// Assert
		var nav = Services.GetService<BunitNavigationManager>();
		nav.Should().NotBeNull();
		nav!.Uri.Should().Contain("auth/login");
		nav.Uri.Should().NotContain("screen_hint");
	}

	/// <summary>
	///   Verifies that clicking the Sign In button uses forced navigation (forceLoad = true).
	/// </summary>
	[Fact]
	public void SignInButton_WhenClicked_ShouldUseForcedNavigation()
	{
		// Arrange
		var cut = Render<Login>();
		var nav = Services.GetService<BunitNavigationManager>();

		// Act
		cut.Find("button.btn-primary").Click();

		// Assert
		nav.Should().NotBeNull();
		nav!.History.Should().HaveCount(1);
		nav.History.First().Options.ForceLoad.Should().BeTrue();
	}

	/// <summary>
	///   Verifies that clicking the Sign Up button navigates to the Auth0 login page with the signup screen hint.
	/// </summary>
	[Fact]
	public void SignUpButton_WhenClicked_ShouldNavigateToAuthLoginWithSignupHint()
	{
		// Arrange
		var cut = Render<Login>();

		// Act
		cut.Find("button.btn-link").Click();

		// Assert
		var nav = Services.GetService<BunitNavigationManager>();
		nav.Should().NotBeNull();
		nav!.Uri.Should().Contain("auth/login?screen_hint=signup");
	}

	/// <summary>
	///   Verifies that clicking the Sign Up button uses forced navigation (forceLoad = true).
	/// </summary>
	[Fact]
	public void SignUpButton_WhenClicked_ShouldUseForcedNavigation()
	{
		// Arrange
		var cut = Render<Login>();
		var nav = Services.GetService<BunitNavigationManager>();

		// Act
		cut.Find("button.btn-link").Click();

		// Assert
		nav.Should().NotBeNull();
		nav!.History.Should().HaveCount(1);
		nav.History.First().Options.ForceLoad.Should().BeTrue();
	}

	/// <summary>
	///   Verifies that the Login component renders without errors.
	/// </summary>
	[Fact]
	public void Login_ShouldRender_WithoutErrors()
	{
		// Arrange & Act
		var cut = Render<Login>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	/// <summary>
	///   Verifies that the Login component has the correct button structure.
	/// </summary>
	[Fact]
	public void Login_ShouldHave_TwoButtons()
	{
		// Arrange & Act
		var cut = Render<Login>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().HaveCount(2);
	}

	/// <summary>
	///   Verifies that the primary button has the correct CSS classes.
	/// </summary>
	[Fact]
	public void SignInButton_ShouldHave_PrimaryButtonStyle()
	{
		// Arrange & Act
		var cut = Render<Login>();

		// Assert
		var signInButton = cut.Find("button.btn-primary");
		signInButton.Should().NotBeNull();
		signInButton.ClassList.Should().Contain("btn");
		signInButton.ClassList.Should().Contain("btn-primary");
	}

	/// <summary>
	///   Verifies that the sign up button has the correct CSS classes.
	/// </summary>
	[Fact]
	public void SignUpButton_ShouldHave_LinkButtonStyle()
	{
		// Arrange & Act
		var cut = Render<Login>();

		// Assert
		var signUpButton = cut.Find("button.btn-link");
		signUpButton.Should().NotBeNull();
		signUpButton.ClassList.Should().Contain("btn");
		signUpButton.ClassList.Should().Contain("btn-link");
	}

}
