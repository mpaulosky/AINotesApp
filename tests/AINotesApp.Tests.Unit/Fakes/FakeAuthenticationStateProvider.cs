// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthenticationStateProvider.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace AINotesApp.Tests.Unit.Fakes;

/// <summary>
///   Fake authentication state provider for testing.
///   Provides methods to dynamically set authentication state during tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class FakeAuthenticationStateProvider : AuthenticationStateProvider
{

	private bool _isAuthenticated;

	private bool _includeName = true;

	private string[] _roles = [];

	private string _userName = string.Empty;

	private string _userId = string.Empty;

	/// <summary>
	///   Initializes a new instance of the <see cref="FakeAuthenticationStateProvider"/> class with default unauthenticated state.
	/// </summary>
	public FakeAuthenticationStateProvider()
	{
		_isAuthenticated = false;
	}

	/// <summary>
	///   Initializes a new instance of the <see cref="FakeAuthenticationStateProvider"/> class with specified authentication state.
	/// </summary>
	/// <param name="userName">The username for the authenticated user.</param>
	/// <param name="roles">The roles assigned to the user.</param>
	/// <param name="isAuthenticated">Whether the user is authenticated.</param>
	public FakeAuthenticationStateProvider(string userName, string[]? roles = null, bool isAuthenticated = true)
	{
		_userName = userName;
		_roles = roles ?? [];
		_isAuthenticated = isAuthenticated;
		_userId = userName; // Default userId to userName
	}

	/// <summary>
	///   Sets the authentication state to authorized with the specified user information.
	/// </summary>
	/// <param name="userName">The username for the authenticated user.</param>
	/// <param name="userId">The user ID (sub claim). If null, defaults to userName.</param>
	/// <param name="roles">Optional roles for the user.</param>
	public void SetAuthorized(string userName, string? userId = null, string[]? roles = null)
	{
		_isAuthenticated = true;
		_userName = userName;
		_userId = userId ?? userName;
		_roles = roles ?? [];
		_includeName = true;
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
	}

	/// <summary>
	///   Sets the authentication state to authorized without a username claim.
	///   Useful for testing scenarios where a user is authenticated but doesn't have name claims.
	/// </summary>
	public void SetAuthorizedWithoutName()
	{
		_isAuthenticated = true;
		_userName = string.Empty;
		_userId = string.Empty;
		_roles = [];
		_includeName = false;
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
	}

	/// <summary>
	///   Sets the authentication state to not authorized.
	/// </summary>
	public void SetNotAuthorized()
	{
		_isAuthenticated = false;
		_userName = string.Empty;
		_userId = string.Empty;
		_roles = [];
		_includeName = true;
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
	}

	/// <summary>
	///   Gets the current authentication state.
	/// </summary>
	/// <returns>A task representing the authentication state.</returns>
	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		ClaimsIdentity identity;

		if (_isAuthenticated)
		{
			var claims = new List<Claim>();

			if (_includeName && !string.IsNullOrEmpty(_userName))
			{
				claims.Add(new Claim(ClaimTypes.Name, _userName));
				claims.Add(new Claim(ClaimTypes.NameIdentifier, _userId));
				claims.Add(new Claim("sub", _userId)); // Auth0 subject claim
			}

			claims.AddRange(_roles.Select(role => new Claim(ClaimTypes.Role, role)));

			identity = new ClaimsIdentity(claims, "FakeAuth");
		}
		else
		{
			identity = new ClaimsIdentity();
		}

		var principal = new ClaimsPrincipal(identity);

		return Task.FromResult(new AuthenticationState(principal));
	}

}