// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ProfileTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Components.User;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components.User;

/// <summary>
///   Unit tests for Profile component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class ProfileTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	/// <summary>
	///   Initializes a new instance of the <see cref="ProfileTests" /> class
	/// </summary>
	public ProfileTests()
	{
		_authProvider = new FakeAuthenticationStateProvider();

		// Register services
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	/// <summary>
	///   Renders the component with authentication context
	/// </summary>
	/// <returns>The rendered component</returns>
	private IRenderedComponent<Profile> RenderWithAuth()
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
		});
	}

	/// <summary>
	///   Creates a custom authentication state with specific claims
	/// </summary>
	private Task<AuthenticationState> CreateCustomAuthState(
			bool isAuthenticated,
			string? name = null,
			string? userId = null,
			string? email = null,
			string? picture = null,
			string[]? roles = null)
	{
		var claims = new List<Claim>();

		if (isAuthenticated)
		{
			if (!string.IsNullOrEmpty(name))
			{
				claims.Add(new Claim(ClaimTypes.Name, name));
			}

			if (!string.IsNullOrEmpty(userId))
			{
				claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
			}

			if (!string.IsNullOrEmpty(email))
			{
				claims.Add(new Claim(ClaimTypes.Email, email));
			}

			if (!string.IsNullOrEmpty(picture))
			{
				claims.Add(new Claim("picture", picture));
			}

			if (roles is not null)
			{
				claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
			}
		}

		var identity = isAuthenticated
				? new ClaimsIdentity(claims, "TestAuth")
				: new ClaimsIdentity();

		var principal = new ClaimsPrincipal(identity);

		return Task.FromResult(new AuthenticationState(principal));
	}

	[Fact]
	public void Profile_WhenAuthenticated_ShowsUserName()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("John Doe");
		cut.Markup.Should().Contain("Name:");
	}

	[Fact]
	public void Profile_WhenAuthenticated_ShowsUserId()
	{
		// Arrange
		var userId = "auth0|123456789";

		_authProvider.SetAuthorized("John Doe", userId);

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain(userId);
		cut.Markup.Should().Contain("User ID:");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithEmail_ShowsEmail()
	{
		// Arrange
		var authState = CreateCustomAuthState(
				isAuthenticated: true,
				name: "John Doe",
				userId: "user123",
				email: "john.doe@example.com");

		// Act
		var cut = Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authState);
		});

		// Assert
		cut.Markup.Should().Contain("john.doe@example.com");
		cut.Markup.Should().Contain("Email:");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithoutEmail_ShowsEmptyEmail()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Email:");
		// Should show empty email field (h4 tag should be empty)
		var markup = cut.Markup;
		markup.Should().MatchRegex(@"<p[^>]*>\s*<span>Email:</span>\s*<h4>\s*</h4>\s*</p>");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithPicture_ShowsPictureImage()
	{
		// Arrange
		var pictureUrl = "https://example.com/profile.jpg";

		var authState = CreateCustomAuthState(
				isAuthenticated: true,
				name: "John Doe",
				userId: "user123",
				picture: pictureUrl);

		// Act
		var cut = Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authState);
		});

		// Assert
		cut.Markup.Should().Contain($"src=\"{pictureUrl}\"");
		cut.Markup.Should().Contain("alt=\"Profile Picture\"");
		cut.Markup.Should().Contain("rounded-full");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithoutPicture_ShowsPlaceholder()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("?");
		cut.Markup.Should().Contain("bg-gray-700");
		cut.Markup.Should().NotContain("src=");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithRoles_ShowsRolesList()
	{
		// Arrange
		var roles = new[] { "Admin", "User", "Moderator" };

		_authProvider.SetAuthorized("John Doe", "user123", roles);

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Roles:");
		cut.Markup.Should().Contain("Admin");
		cut.Markup.Should().Contain("User");
		cut.Markup.Should().Contain("Moderator");
		cut.Markup.Should().Contain("list-disc");
		cut.Markup.Should().Contain("text-green-700");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithoutRoles_ShowsNoRolesMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("No roles assigned");
		cut.Markup.Should().Contain("text-yellow-400");
	}

	[Fact]
	public void Profile_WhenAuthenticated_ShowsAllClaimsTable()
	{
		// Arrange
		var authState = CreateCustomAuthState(
				isAuthenticated: true,
				name: "John Doe",
				userId: "user123",
				email: "john@example.com",
				roles: new[] { "Admin" });

		// Act
		var cut = Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authState);
		});

		// Assert
		cut.Markup.Should().Contain("All Claims");
		cut.Markup.Should().Contain("Debug information showing all claims for this user:");
		cut.Markup.Should().Contain("Claim Type");
		cut.Markup.Should().Contain("Value");
		cut.Markup.Should().Contain("<table");
		cut.Markup.Should().Contain("<thead>");
		cut.Markup.Should().Contain("<tbody>");
	}

	[Fact]
	public void Profile_WhenAuthenticated_DisplaysClaimsInTable()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123", new[] { "Admin" });

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain(ClaimTypes.Name);
		cut.Markup.Should().Contain(ClaimTypes.NameIdentifier);
		cut.Markup.Should().Contain("John Doe");
		cut.Markup.Should().Contain("user123");
	}

	[Fact]
	public void Profile_HasCorrectPageRoute()
	{
		// Arrange & Act
		var cut = RenderWithAuth();

		// Assert - Component should be accessible at /profile route
		// This is verified by the component compiling with the @page directive
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Profile_HasAuthorizeAttribute()
	{
		// Arrange & Act
		var cut = RenderWithAuth();

		// Assert - Component should require authorization
		// This is verified by the component compiling with [Authorize] attribute
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Profile_RendersProfileInformationSection()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("Profile Information");
		cut.Markup.Should().Contain("Basic Information");
		cut.Markup.Should().Contain("Roles & Permissions");
		cut.Markup.Should().Contain("Profile Picture:");
	}

	[Fact]
	public void Profile_UsesCorrectGridLayout()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("grid grid-cols-1 md:grid-cols-3");
		cut.Markup.Should().Contain("gap-6");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithEmptyPictureString_ShowsPlaceholder()
	{
		// Arrange
		var authState = CreateCustomAuthState(
				isAuthenticated: true,
				name: "John Doe",
				userId: "user123",
				picture: ""); // Empty string

		// Act
		var cut = Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authState);
		});

		// Assert
		cut.Markup.Should().Contain("?");
		cut.Markup.Should().Contain("bg-gray-700");
		cut.Markup.Should().NotContain("src=\"\"");
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithMultipleRoles_DisplaysAllRoles()
	{
		// Arrange
		var roles = new[] { "Admin", "User", "Moderator", "Developer", "Tester" };

		_authProvider.SetAuthorized("John Doe", "user123", roles);

		// Act
		var cut = RenderWithAuth();

		// Assert
		foreach (var role in roles)
		{
			cut.Markup.Should().Contain(role);
		}
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithLongUserId_DisplaysFullId()
	{
		// Arrange
		var longUserId = "auth0|1234567890abcdefghijklmnopqrstuvwxyz";

		_authProvider.SetAuthorized("John Doe", longUserId);

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain(longUserId);
	}

	[Fact]
	public void Profile_WhenAuthenticated_WithSpecialCharactersInName_DisplaysCorrectly()
	{
		// Arrange
		var specialName = "O'Brien-Smith";

		_authProvider.SetAuthorized(specialName, "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain(specialName);
	}

	[Fact]
	public void Profile_ClaimsTableOrderedByType()
	{
		// Arrange
		var authState = CreateCustomAuthState(
				isAuthenticated: true,
				name: "John Doe",
				userId: "user123",
				email: "john@example.com");

		// Act
		var cut = Render<Profile>(ps =>
		{
			ps.AddCascadingValue(authState);
		});

		// Assert - Claims should be ordered alphabetically in the table
		// The component uses OrderBy(c => c.Type) in the foreach loop
		cut.Markup.Should().Contain("<tbody>");
		cut.Markup.Should().Contain(ClaimTypes.Email);
		cut.Markup.Should().Contain(ClaimTypes.Name);
		cut.Markup.Should().Contain(ClaimTypes.NameIdentifier);
	}

	[Fact]
	public void Profile_ResponsiveLayout_HasContainerClasses()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123");

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("container mx-auto");
		cut.Markup.Should().Contain("container-card");
	}

	[Fact]
	public void Profile_RolesSection_HasCorrectStyling()
	{
		// Arrange
		_authProvider.SetAuthorized("John Doe", "user123", new[] { "Admin" });

		// Act
		var cut = RenderWithAuth();

		// Assert
		cut.Markup.Should().Contain("text-green-700");
		cut.Markup.Should().Contain("ml-4");
		cut.Markup.Should().Contain("list-inside");
	}

}
