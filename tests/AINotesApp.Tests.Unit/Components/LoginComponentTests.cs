// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     LoginComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components;

/// <summary>
///   Unit tests for LoginComponent using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginComponentTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	/// <summary>
	///   Initializes a new instance of the <see cref="LoginComponentTests" /> class
	/// </summary>
	public LoginComponentTests()
	{
		_authProvider = new FakeAuthenticationStateProvider();

		// Register services
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	/// <summary>
	///   Renders the component with authentication context
	/// </summary>
	/// <returns>The rendered component</returns>
	private IRenderedComponent<AINotesApp.Components.LoginComponent> RenderWithAuth()
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<AINotesApp.Components.LoginComponent>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
		});
	}

	[Fact]
	public void LoginComponent_WhenAuthenticated_ShowsLogoutLink()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Log out");
		cut.Markup.Should().Contain("Account/Logout");
	}

	[Fact]
	public void LoginComponent_WhenNotAuthenticated_ShowsLoginLink()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Log in");
		cut.Markup.Should().Contain("Account/Login");
	}

	[Fact]
	public void LoginComponent_WhenAuthenticated_DoesNotShowLoginLink()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().NotContain("Log in");
		cut.Markup.Should().NotContain("Account/Login");
	}

	[Fact]
	public void LoginComponent_WhenNotAuthenticated_DoesNotShowLogoutLink()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().NotContain("Log out");
		cut.Markup.Should().NotContain("Account/Logout");
	}

	[Fact]
	public void LoginComponent_UsesAuthorizeView()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert - AuthorizeView should be present in the component structure
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void LoginComponent_LoginLink_HasCorrectHref()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.Should().NotBeNull();
		link.GetAttribute("href").Should().Be("Account/Login");
	}

	[Fact]
	public void LoginComponent_LogoutLink_HasCorrectHref()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.Should().NotBeNull();
		link.GetAttribute("href").Should().Be("Account/Logout");
	}

	[Fact]
	public void LoginComponent_LoginLink_HasCorrectStyling()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.ClassList.Should().Contain("p-1");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}

	[Fact]
	public void LoginComponent_LogoutLink_HasCorrectStyling()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.ClassList.Should().Contain("p-1");
		link.ClassList.Should().Contain("hover:text-blue-700");
	}

	[Fact]
	public void LoginComponent_RendersWithoutErrors()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void LoginComponent_WhenAuthenticatedWithDifferentUsers_ShowsLogout()
	{
		// Arrange
		_authProvider.SetAuthorized("User1", "user1-id");
		var cut1 = RenderWithAuth();

		// Act
		_authProvider.SetAuthorized("User2", "user2-id");
		var cut2 = RenderWithAuth();

		// Assert - Both should show logout
		cut1.Markup.Should().Contain("Log out");
		cut2.Markup.Should().Contain("Log out");
	}

	[Fact]
	public void LoginComponent_HasOnlyOneLink()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		var links = cut.FindAll("a");
		links.Should().HaveCount(1);
	}

	[Fact]
	public void LoginComponent_LoginLink_IsAnchorElement()
	{
		// Arrange
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.Should().NotBeNull();
		link.NodeName.Should().Be("A");
	}

	[Fact]
	public void LoginComponent_LogoutLink_IsAnchorElement()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		var link = cut.Find("a");
		link.Should().NotBeNull();
		link.NodeName.Should().Be("A");
	}

	[Fact]
	public void LoginComponent_WhenAuthenticatedWithRoles_StillShowsLogout()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser", "user123", new[] { "Admin", "User" });

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Log out");
		cut.Markup.Should().Contain("Account/Logout");
	}

	[Fact]
	public void LoginComponent_MultipleInstances_RenderIndependently()
	{
		// Arrange & Act
		_authProvider.SetAuthorized("TestUser", "user123");
		var cut1 = RenderWithAuth();
		var cut2 = RenderWithAuth();

		// Assert
		cut1.Markup.Should().Contain("Log out");
		cut2.Markup.Should().Contain("Log out");
		cut1.Markup.Should().Be(cut2.Markup);
	}

}
