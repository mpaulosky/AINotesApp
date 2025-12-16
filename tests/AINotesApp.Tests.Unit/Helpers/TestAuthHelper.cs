// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     TestAuthHelper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Tests.Unit.Fakes;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Helpers;

/// <summary>
///   Helper class for configuring authentication in unit tests.
///   Provides methods to register authentication providers with consistent configuration.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestAuthHelper
{

	/// <summary>
	///   Registers a test authentication provider for bUnit tests.
	/// </summary>
	/// <param name="services">The service collection to register with.</param>
	/// <param name="userName">The test username.</param>
	/// <param name="roles">The roles for the test user.</param>
	public static void RegisterTestAuthentication(IServiceCollection services, string userName, string[] roles)
	{
		services.AddSingleton<AuthenticationStateProvider>(
				new FakeAuthenticationStateProvider(userName, roles));
	}

	/// <summary>
	///   Registers a test authentication provider for bUnit tests with dynamic state management.
	/// </summary>
	/// <param name="services">The service collection to register with.</param>
	/// <returns>The registered FakeAuthenticationStateProvider instance for dynamic configuration.</returns>
	public static FakeAuthenticationStateProvider RegisterDynamicTestAuthentication(IServiceCollection services)
	{
		var provider = new FakeAuthenticationStateProvider();
		services.AddSingleton<AuthenticationStateProvider>(provider);
		return provider;
	}

	/// <summary>
	///   Registers an authenticated user with the specified user ID and roles.
	/// </summary>
	/// <param name="services">The service collection to register with.</param>
	/// <param name="userName">The test username.</param>
	/// <param name="userId">The user ID (sub claim).</param>
	/// <param name="roles">Optional roles for the user.</param>
	public static void RegisterAuthenticatedUser(
		IServiceCollection services,
		string userName,
		string userId,
		string[]? roles = null)
	{
		services.AddSingleton<AuthenticationStateProvider>(
				new FakeAuthenticationStateProvider(userName, roles)
		);
	}

}