// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthorizationService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

namespace AINotesApp.Tests.Unit.Fakes;

/// <summary>
///   Fake authorization service for testing. Always authorizes authenticated users.
/// </summary>
[ExcludeFromCodeCoverage]
public class FakeAuthorizationService : IAuthorizationService
{

	/// <summary>
	///   Authorizes the user based on requirements.
	/// </summary>
	/// <param name="user">The user principal.</param>
	/// <param name="resource">The resource to authorize.</param>
	/// <param name="requirements">The authorization requirements.</param>
	/// <returns>Authorization result indicating success if user is authenticated.</returns>
	public Task<AuthorizationResult> AuthorizeAsync(
			ClaimsPrincipal user,
			object? resource,
			IEnumerable<IAuthorizationRequirement> requirements)
	{
		return Task.FromResult(user?.Identity?.IsAuthenticated == true
				? AuthorizationResult.Success()
				: AuthorizationResult.Failed());
	}

	/// <summary>
	///   Authorizes the user based on the policy name.
	/// </summary>
	/// <param name="user">The user principal.</param>
	/// <param name="resource">The resource to authorize.</param>
	/// <param name="policyName">The policy name.</param>
	/// <returns>Authorization result indicating success if user is authenticated.</returns>
	public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
	{
		return Task.FromResult(user?.Identity?.IsAuthenticated == true
				? AuthorizationResult.Success()
				: AuthorizationResult.Failed());
	}

}
