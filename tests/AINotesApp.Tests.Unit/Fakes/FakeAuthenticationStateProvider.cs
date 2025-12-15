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

[ExcludeFromCodeCoverage]
public class FakeAuthenticationStateProvider : AuthenticationStateProvider
{

	private readonly bool _isAuthenticated;

	private readonly string[] _roles;

	private readonly string _userName;

	public FakeAuthenticationStateProvider(string userName, string[]? roles, bool isAuthenticated = true)
	{
		_userName = userName;
		_roles = roles ?? [];
		_isAuthenticated = isAuthenticated;
	}

	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var claims = new List<Claim>();

		if (_isAuthenticated)
		{
			claims.Add(new Claim(ClaimTypes.Name, _userName));
			claims.AddRange(_roles.Select(role => new Claim(ClaimTypes.Role, role)));
		}

		var identity = _isAuthenticated
				? new ClaimsIdentity(claims, "FakeAuth")
				: new ClaimsIdentity();

		var principal = new ClaimsPrincipal(identity);

		return Task.FromResult(new AuthenticationState(principal));
	}

}