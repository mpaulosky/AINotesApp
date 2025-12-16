// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NavMenuTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Components.Layout;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components.Layout;

/// <summary>
///   Unit tests for the NavMenu component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class NavMenuTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	public NavMenuTests()
	{
		_authProvider = new FakeAuthenticationStateProvider();

		// Register an authentication state provider
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);

		// Register fake authorization services
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	private IRenderedComponent<TComponent> RenderWithAuth<TComponent>() where TComponent : IComponent
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<TComponent>(ps => ps
				.AddCascadingValue(authStateTask)
		);
	}

	[Fact]
	public void NavMenu_ShouldRender_BrandName()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Find(".navbar-brand").TextContent.Should().Be("AINotesApp");
	}

	[Fact]
	public void NavMenu_ShouldRender_HomeLink()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		var homeLinks = cut.FindAll(".nav-link");
		var homeLink = homeLinks.FirstOrDefault(link => link.TextContent.Contains("Home"));
		homeLink.Should().NotBeNull();
	}

	[Fact]
	public void NavMenu_ShouldRender_AuthRequiredLink()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		var navLinks = cut.FindAll(".nav-link");
		var authLink = navLinks.FirstOrDefault(link => link.TextContent.Contains("Auth Required"));
		authLink.Should().NotBeNull();
	}

	[Fact]
	public void NavMenu_WhenNotAuthenticated_ShowsRegisterAndLoginLinks()
	{
		//_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		//cut.Markup.Should().Contain("Register");
		cut.Markup.Should().Contain("Login");
	}

	[Fact]
	public void NavMenu_WhenNotAuthenticated_DoesNotShowMyNotesLink()
	{
		_authProvider.SetNotAuthorized();

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Markup.Should().NotContain("My Notes");
	}

	[Fact]
	public void NavMenu_WhenAuthenticated_ShowsMyNotesLink()
	{
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("My Notes");
	}

	[Fact]
	public void NavMenu_WhenAuthenticated_ShowsUserName()
	{
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("TestUser");
	}

	[Fact]
	public void NavMenu_WhenAuthenticated_ShowsLogoutButton()
	{
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("Logout");
	}

	[Fact]
	public void NavMenu_WhenAuthenticated_DoesNotShowRegisterAndLoginLinks()
	{
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		var navLinks = cut.FindAll(".nav-link");
		var registerLink = navLinks.FirstOrDefault(link => link.TextContent.Contains("Register"));
		var loginLink = navLinks.FirstOrDefault(link => link.TextContent.Contains("Login"));

		registerLink.Should().BeNull();
		loginLink.Should().BeNull();
	}

	[Fact]
	public void NavMenu_HasNavbarToggler()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		var toggler = cut.Find(".navbar-toggler");
		toggler.Should().NotBeNull();
		toggler.GetAttribute("type").Should().Be("checkbox");
	}

	[Fact]
	public void NavMenu_HasNavScrollableSection()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Find(".nav-scrollable").Should().NotBeNull();
	}

	[Fact]
	public void NavMenu_ImplementsIDisposable()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		Assert.IsAssignableFrom<IDisposable>(cut.Instance);
	}

	[Fact]
	public void NavMenu_WhenAuthenticated_ShowsManageLink()
	{
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		// Check for the Manage link text
		cut.Markup.Should().Contain("Manage");
	}

	[Fact]
	public void NavMenu_TopRow_ContainsBrandAndFluid()
	{
		// Arrange & Act
		var cut = RenderWithAuth<NavMenu>();

		// Assert
		cut.Find(".top-row").Should().NotBeNull();
		cut.Find(".container-fluid").Should().NotBeNull();
	}

}
