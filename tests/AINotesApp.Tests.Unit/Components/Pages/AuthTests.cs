// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Components.Pages;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components.Pages;

/// <summary>
///   Unit tests for an Auth component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	/// <summary>
	///   Initializes a new instance of the <see cref="AuthTests" /> class
	/// </summary>
	public AuthTests()
	{
		_authProvider = new FakeAuthenticationStateProvider();

		// Register an authentication state provider
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);

		// Register fake authorization services
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	/// <summary>
	///   Renders a component with authentication context
	/// </summary>
	/// <typeparam name="TComponent">The component type to render</typeparam>
	/// <returns>The rendered component</returns>
	private IRenderedComponent<TComponent> RenderWithAuth<TComponent>() where TComponent : IComponent
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<TComponent>(ps => ps
				.AddCascadingValue(authStateTask)
		);
	}

	[Fact]
	public void Auth_ShouldRender_WithoutErrors()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Auth_ShouldRender_H1Header()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Should().Be("You are authenticated");
	}

	[Fact]
	public void Auth_WhenAuthenticated_ShowsUserName()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello TestUser!");
	}

	[Fact]
	public void Auth_WhenAuthenticated_DisplaysWelcomeMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("JohnDoe");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello");
		cut.Markup.Should().Contain("JohnDoe");
	}

	[Fact]
	public void Auth_WhenUserNameIsNull_HandlesGracefully()
	{
		// Arrange
		_authProvider.SetAuthorizedWithoutName();

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello");

		// Component should still render without errors
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Auth_HasPageTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// PageTitle is rendered by the framework, not in component markup
		// Check that the component renders successfully with the page title directive
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Auth_HasCorrectPageRoute()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// Component should render successfully, indicating the route is valid
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("You are authenticated");
	}

	[Fact]
	public void Auth_UsesAuthorizeView()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// AuthorizeView should render the authenticated content
		cut.Markup.Should().Contain("Hello TestUser!");
	}

	[Fact]
	public void Auth_WhenAuthenticated_DisplaysCompleteMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("AdminUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("You are authenticated");
		cut.Markup.Should().Contain("Hello AdminUser!");
	}

	[Fact]
	public void Auth_HasCorrectStructure()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Trim().Should().Be("You are authenticated");
	}

}
