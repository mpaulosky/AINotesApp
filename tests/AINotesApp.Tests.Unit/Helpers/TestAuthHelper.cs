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

}