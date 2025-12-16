// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthorizationPolicyProvider.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Authorization;

namespace AINotesApp.Tests.Unit.Fakes;

/// <summary>
///   Fake authorization policy provider for testing. Provides simple authenticated user policies.
/// </summary>
[ExcludeFromCodeCoverage]
public class FakeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{

	/// <summary>
	///   Gets the default authorization policy.
	/// </summary>
	/// <returns>A policy requiring authenticated users.</returns>
	public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
	{
		return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
	}

	/// <summary>
	///   Gets the authorization policy by name.
	/// </summary>
	/// <param name="policyName">The policy name.</param>
	/// <returns>A policy requiring authenticated users.</returns>
	public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
	{
		return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
	}

	/// <summary>
	///   Gets the fallback authorization policy.
	/// </summary>
	/// <returns>Null, indicating no fallback policy.</returns>
	public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
	{
		return Task.FromResult<AuthorizationPolicy?>(null);
	}

}
