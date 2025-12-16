// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Auth0AuthenticationStateProvider.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace AINotesApp.Services;

public class Auth0AuthenticationStateProvider : AuthenticationStateProvider
{

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly ILogger<Auth0AuthenticationStateProvider> _logger;

	public Auth0AuthenticationStateProvider(
			IHttpContextAccessor httpContextAccessor,
			ILogger<Auth0AuthenticationStateProvider> logger)
	{
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var httpContext = _httpContextAccessor.HttpContext;

		if (httpContext?.User.Identity?.IsAuthenticated == true)
		{
			var user = httpContext.User;

			// Log user claims for debugging
			_logger.LogInformation("User authenticated with claims:");

			foreach (var claim in user.Claims)
			{
				_logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
			}

			// Create a new ClaimsIdentity with the existing claims plus any additional processing
			ClaimsIdentity identity = new(user.Identity);

			// Add role claims if they exist
			var rolesClaim = user.FindFirst("https://articlesite.com/roles")?.Value;

			if (!string.IsNullOrEmpty(rolesClaim))
			{
				var roles = rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);

				foreach (var role in roles)
				{
					identity.AddClaim(new Claim(ClaimTypes.Role, role.Trim()));
				}
			}

			// Check for Auth0 roles in the standard location
			var auth0Roles = user.FindAll("roles");

			foreach (var roleClaim in auth0Roles)
			{
				if (!identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
				{
					identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
				}
			}

			ClaimsPrincipal claimsPrincipal = new(identity);

			return Task.FromResult(new AuthenticationState(claimsPrincipal));
		}

		// Return an anonymous user if not authenticated
		ClaimsPrincipal anonymous = new(new ClaimsIdentity());

		return Task.FromResult(new AuthenticationState(anonymous));
	}

}