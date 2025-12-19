// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthenticationServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Auth0.AspNetCore.Authentication;

using Microsoft.AspNetCore.Components.Authorization;

namespace AINotesApp.Services;

/// <summary>
///   Extension methods for configuring authentication and authorization services.
/// </summary>
[ExcludeFromCodeCoverage]

public static class AuthenticationServiceExtensions
{

	/// <summary>
	///   Adds and configures authentication, authorization, and CORS services.
	/// </summary>
	public static void AddAuthenticationAndAuthorization(
			this IServiceCollection services,
			IConfiguration configuration)
	{

		services.AddHttpContextAccessor();

		services
				.AddAuth0WebAppAuthentication(options =>
				{
					options.Domain = configuration["Auth0:Domain"] ?? string.Empty;
					options.ClientId = configuration["Auth0:ClientId"] ?? string.Empty;
					options.ClientSecret = configuration["Auth0:ClientSecret"] ?? string.Empty;
					options.Scope = "openid profile email";
					options.CallbackPath = configuration["Auth0:CallbackPath"] ?? "/auth/callback";
				})
				.WithAccessToken(options =>
				{
					var audience = configuration["Auth0:Audience"];

					if (!string.IsNullOrWhiteSpace(audience))
					{
						options.Audience = audience;
					}
				});

		// Add policy
		services.AddAuthorizationBuilder()
				.AddPolicy("NotesAdmin", policy =>
						policy.RequireClaim(ClaimTypes.Role, "notes.admin"));

		// Configure authentication state services
		services.AddScoped<AuthenticationStateProvider, Auth0AuthenticationStateProvider>();

		services.AddCascadingAuthenticationState();

	}

}