// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MainLayoutTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.RegularExpressions;

using AINotesApp.Components.Layout;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components.Layout;

/// <summary>
///   Unit tests for the MainLayout component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class MainLayoutTests : BunitContext
{

	private readonly TestAuthStateProvider _authProvider;

	public MainLayoutTests()
	{
		_authProvider = new TestAuthStateProvider();

		// Register authentication state provider
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);

		// Register fake authorization services
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	private IRenderedComponent<TComponent> RenderWithAuth<TComponent>() where TComponent : IComponent
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<TComponent>((Action<ComponentParameterCollectionBuilder<TComponent>>)(ps => ps
						.AddCascadingValue(authStateTask)
				));
	}

	[Fact]
	public void MainLayout_RendersPageStructure()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find(".page").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersSidebar()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find(".sidebar").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersMainSection()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find("main").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersNavMenu()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		// Try to find the brand element by class or id, fallback to text search
		var brand = cut.FindAll("a, span, div, h1, h2, h3, h4, h5, h6").FirstOrDefault(e =>
				e.TextContent.Trim().Contains("AINotesApp", StringComparison.OrdinalIgnoreCase));

		brand.Should().NotBeNull();
		brand.TextContent.Should().Contain("AINotesApp");
	}

	[Fact]
	public void MainLayout_RendersTopRow()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find(".top-row").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersAboutLink()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		var aboutLink = cut.FindAll("a")
				.FirstOrDefault(a => string.Equals(a.TextContent.Trim(), "About", StringComparison.OrdinalIgnoreCase));

		aboutLink.Should().NotBeNull();
		aboutLink.TextContent.Trim().Should().Be("About");
	}

	[Fact]
	public void MainLayout_RendersContentArticle()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find("article.content").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersErrorUI()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		var errorUi = cut.Find("#blazor-error-ui");
		errorUi.Should().NotBeNull();
		errorUi.GetAttribute("data-nosnippet").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ErrorUI_ContainsReloadLink()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		var reloadButton = cut.Find("#blazor-error-ui a.reload");
		reloadButton.Should().NotBeNull();
		reloadButton.TextContent.Should().Be("Reload");
	}

	[Fact]
	public void MainLayout_ErrorUI_ContainsDismissButton()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Markup.Should().Contain("dismiss");
	}

	[Fact]
	public void MainLayout_InheritsLayoutComponentBase()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Instance.Should().BeAssignableTo<LayoutComponentBase>();
	}

	[Fact]
	public void MainLayout_HasCorrectCssClasses()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert
		cut.Find(".page").Should().NotBeNull();
		cut.Find(".sidebar").Should().NotBeNull();
		cut.Find("main").Should().NotBeNull();
		cut.Find(".top-row.px-4").Should().NotBeNull();
		cut.Find("article.content.px-4").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ErrorUI_HasCorrectStructure()
	{
		// Arrange & Act
		var cut = RenderWithAuth<MainLayout>();

		// Assert (case-insensitive, whitespace-insensitive)
		Regex.IsMatch(cut.Markup, @"an\s+unhandled\s+error\s+has\s+occurred", RegexOptions.IgnoreCase)
				.Should().BeTrue();

		Regex.IsMatch(cut.Markup, @"reload", RegexOptions.IgnoreCase)
				.Should().BeTrue();
	}

	public class TestAuthStateProvider : AuthenticationStateProvider
	{

		private bool _isAuthenticated  ;

		private string _userName = string.Empty;

		public void SetAuthorized(string userName)
		{
			_isAuthenticated = true;
			_userName = userName;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		public void SetNotAuthorized()
		{
			_isAuthenticated = false;
			_userName = string.Empty;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var identity = _isAuthenticated
					? new ClaimsIdentity([
							new Claim(ClaimTypes.Name, _userName),
							new Claim(ClaimTypes.NameIdentifier, _userName)
					], "TestAuth")
					: new ClaimsIdentity();

			var user = new ClaimsPrincipal(identity);

			return Task.FromResult(new AuthenticationState(user));
		}

	}

	/// <summary>
	///   Fake authorization service for testing
	/// </summary>
	private class FakeAuthorizationService : IAuthorizationService
	{

		public Task<AuthorizationResult> AuthorizeAsync(
				ClaimsPrincipal user,
				object? resource,
				IEnumerable<IAuthorizationRequirement> requirements)
		{
			// Check if user is authenticated
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}

			return Task.FromResult(AuthorizationResult.Failed());
		}

		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
		{
			// Check if user is authenticated
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}

			return Task.FromResult(AuthorizationResult.Failed());
		}

	}

	/// <summary>
	///   Fake authorization policy provider for testing
	/// </summary>
	private class FakeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
	{

		public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
		{
			return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
		{
			return Task.FromResult<AuthorizationPolicy?>(null);
		}

	}

}