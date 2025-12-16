// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     RoutesTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Components;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AINotesApp.Tests.Unit.Components;

/// <summary>
///   BUnit tests for Routes. Razor
/// </summary>
[ExcludeFromCodeCoverage]
public class RoutesTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	public RoutesTests()
	{
		_authProvider = new FakeAuthenticationStateProvider();
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);

		// Provide required authorization services for AuthorizeRouteView
		Services.AddSingleton<IAuthorizationService, AllowAllAuthorizationService>();
		Services.AddSingleton(Options.Create(new AuthorizationOptions()));
		Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
	}

	[Fact]
	public void Routes_ShouldRender_WithRouterAndAuthorizeRouteView()
	{
		// Arrange
		_authProvider.SetAuthorized("user1");

		// Act
		var cut = Render<Routes>();

		// Assert - verify that Home content is rendered through routing
		cut.Markup.Should().Contain("Hello, world!");
		cut.Markup.Should().Contain("Welcome to your new app.");
	}

	private sealed class AllowAllAuthorizationService : IAuthorizationService
	{

		public Task<AuthorizationResult> AuthorizeAsync(
				ClaimsPrincipal user,
				object? resource,
				IEnumerable<IAuthorizationRequirement> requirements)
		{
			return Task.FromResult(AuthorizationResult.Success());
		}

		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
		{
			return Task.FromResult(AuthorizationResult.Success());
		}

	}

}