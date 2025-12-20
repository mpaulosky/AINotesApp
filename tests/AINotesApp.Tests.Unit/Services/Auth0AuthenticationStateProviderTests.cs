// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Auth0AuthenticationStateProviderTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AINotesApp.Services;

using FluentAssertions;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Services;

// /// <summary>
// ///   Unit tests for Auth0AuthenticationStateProvider.
// /// </summary>
// [ExcludeFromCodeCoverage]
// public class Auth0AuthenticationStateProviderTests
// {

// 	private readonly IHttpContextAccessor _httpContextAccessor;

// 	private readonly ILogger<Auth0AuthenticationStateProvider> _logger;

// 	private readonly Auth0AuthenticationStateProvider _provider;

// 	public Auth0AuthenticationStateProviderTests()
// 	{
// 		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
// 		_logger = Substitute.For<ILogger<Auth0AuthenticationStateProvider>>();
// 		_provider = new Auth0AuthenticationStateProvider(_httpContextAccessor, _logger);
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WhenHttpContextIsNull_ReturnsAnonymousUser()
// 	{
// 		// Given
// 		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeFalse();
// 		result.User.Claims.Should().BeEmpty();
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WhenUserIsNotAuthenticated_ReturnsAnonymousUser()
// 	{
// 		// Given
// 		var httpContext = Substitute.For<HttpContext>();
// 		var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeFalse();
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WhenUserIsAuthenticated_ReturnsAuthenticatedUser()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new(ClaimTypes.Email, "test@example.com")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeTrue();
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WhenUserIsAuthenticated_LogsClaims()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new(ClaimTypes.Email, "test@example.com")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		_logger.Received().Log(
// 			LogLevel.Information,
// 			Arg.Any<EventId>(),
// 			Arg.Is<object>(o => o.ToString()!.Contains("User authenticated with claims")),
// 			Arg.Any<Exception>(),
// 			Arg.Any<Func<object, Exception?, string>>());

// 		_logger.Received().Log(
// 			LogLevel.Information,
// 			Arg.Any<EventId>(),
// 			Arg.Is<object>(o => o.ToString()!.Contains("Claim:")),
// 			Arg.Any<Exception>(),
// 			Arg.Any<Func<object, Exception?, string>>());
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithCustomRolesClaim_AddsRolesToIdentity()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "Admin,User,Moderator")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeTrue();
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Moderator");
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithCustomRolesClaimWithSpaces_TrimsRoleNames()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", " Admin , User , Moderator ")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Moderator");
// 		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should()
// 			.NotContain(c => c.Value.Contains(" "));
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithEmptyCustomRolesClaim_DoesNotAddRoles()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithAuth0RolesClaims_AddsRolesToIdentity()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("roles", "Admin"),
// 			new("roles", "User")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeTrue();
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithDuplicateRoles_DoesNotAddDuplicates()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "Admin,User"),
// 			new("roles", "Admin"), // Duplicate
// 			new("roles", "Moderator")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		var adminRoles = result.User.Claims
// 			.Where(c => c.Type == ClaimTypes.Role && c.Value == "Admin")
// 			.ToList();
// 		adminRoles.Should().HaveCount(1); // Only one Admin role should exist
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Moderator");
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithBothRoleTypes_AddsAllUniqueRoles()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "Admin,User"),
// 			new("roles", "Moderator"),
// 			new("roles", "Editor")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		var roleClaims = result.User.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
// 		roleClaims.Should().HaveCount(4);
// 		roleClaims.Should().Contain(c => c.Value == "Admin");
// 		roleClaims.Should().Contain(c => c.Value == "User");
// 		roleClaims.Should().Contain(c => c.Value == "Moderator");
// 		roleClaims.Should().Contain(c => c.Value == "Editor");
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithNoRoleClaims_ReturnsAuthenticatedUserWithoutRoles()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new(ClaimTypes.Email, "test@example.com")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Identity.Should().NotBeNull();
// 		result.User.Identity!.IsAuthenticated.Should().BeTrue();
// 		result.User.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithSingleRole_AddsRoleCorrectly()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "Admin")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		result.User.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
// 		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(1);
// 	}

// 	[Fact]
// 	public async Task GetAuthenticationStateAsync_WithEmptyRolesInCommaSeparatedList_IgnoresEmptyEntries()
// 	{
// 		// Given
// 		var claims = new List<Claim>
// 		{
// 			new(ClaimTypes.Name, "testuser"),
// 			new("https://articlesite.com/roles", "Admin,,User,,,Moderator")
// 		};
// 		var identity = new ClaimsIdentity(claims, "TestAuthType");
// 		var claimsPrincipal = new ClaimsPrincipal(identity);

// 		var httpContext = Substitute.For<HttpContext>();
// 		httpContext.User.Returns(claimsPrincipal);
// 		_httpContextAccessor.HttpContext.Returns(httpContext);

// 		// When
// 		var result = await _provider.GetAuthenticationStateAsync();

// 		// Then
// 		result.Should().NotBeNull();
// 		var roleClaims = result.User.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
// 		roleClaims.Should().HaveCount(3);
// 		roleClaims.Should().Contain(c => c.Value == "Admin");
// 		roleClaims.Should().Contain(c => c.Value == "User");
// 		roleClaims.Should().Contain(c => c.Value == "Moderator");
// 	}

// }


/// <summary>
/// Unit tests for Auth0AuthenticationStateProvider
/// </summary>
[ExcludeFromCodeCoverage]
public class Auth0AuthenticationStateProviderTests
{
	private readonly IHttpContextAccessor _mockHttpContextAccessor;
	private readonly ILogger<Auth0AuthenticationStateProvider> _mockLogger;
	private readonly Auth0AuthenticationStateProvider _provider;

	public Auth0AuthenticationStateProviderTests()
	{
		_mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_mockLogger = Substitute.For<ILogger<Auth0AuthenticationStateProvider>>();
		_provider = new Auth0AuthenticationStateProvider(_mockHttpContextAccessor, _mockLogger);
	}

	[Fact]
	public void Constructor_WithValidDependencies_ShouldCreateInstance()
	{
		// Arrange & Act
		var provider = new Auth0AuthenticationStateProvider(_mockHttpContextAccessor, _mockLogger);

		// Assert
		provider.Should().NotBeNull();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenHttpContextIsNull_ShouldReturnAnonymousUser()
	{
		// Arrange
		_mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserIsNotAuthenticated_ShouldReturnAnonymousUser()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var identity = new ClaimsIdentity(); // Not authenticated
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserIsAuthenticated_ShouldReturnAuthenticatedUser()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim(ClaimTypes.Email, "test@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasCustomRoleClaim_ShouldAddRoleClaimsToIdentity()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin,Editor")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasAuth0RoleClaim_ShouldAddRoleClaimsToIdentity()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("roles", "Admin"),
			new Claim("roles", "Editor")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasBothRoleClaims_ShouldNotDuplicateRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin"),
			new Claim("roles", "Admin") // Duplicate
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role && c.Value == "Admin").Should().HaveCount(1);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenCustomRoleClaimIsEmpty_ShouldNotAddRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenRoleClaimHasWhitespace_ShouldTrimRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", " Admin , Editor ")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.IsInRole(" Admin ").Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_ShouldPreserveOriginalClaims()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim(ClaimTypes.Email, "test@example.com"),
			new Claim("custom_claim", "custom_value")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("tester");
		result.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
		result.User.FindFirst("custom_claim")?.Value.Should().Be("custom_value");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenMultipleRolesInCustomClaim_ShouldSplitAndAddAll()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin,Editor,Viewer")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.IsInRole("Viewer").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(3);
	}
}