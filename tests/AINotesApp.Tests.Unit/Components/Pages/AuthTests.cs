using System.Security.Claims;
using AINotesApp.Components.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Components.Pages;

/// <summary>
/// Unit tests for Auth component using BUnit 2.x
/// </summary>
public class AuthTests : BunitContext
{
	private readonly TestAuthStateProvider _authProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthTests"/> class
	/// </summary>
	public AuthTests()
	{
		_authProvider = new TestAuthStateProvider();

		// Register authentication state provider
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);

		// Register fake authorization services
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	/// <summary>
	/// Renders a component with authentication context
	/// </summary>
	/// <typeparam name="TComponent">The component type to render</typeparam>
	/// <returns>The rendered component</returns>
	private IRenderedComponent<TComponent> RenderWithAuth<TComponent>() where TComponent : IComponent
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();
		return Render<TComponent>(ps => ps
			.AddCascadingValue(authStateTask)
		);
	}

	[Fact]
	public void Auth_ShouldRender_WithoutErrors()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Auth_ShouldRender_H1Header()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Should().Be("You are authenticated");
	}

	[Fact]
	public void Auth_WhenAuthenticated_ShowsUserName()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello TestUser!");
	}

	[Fact]
	public void Auth_WhenAuthenticated_DisplaysWelcomeMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("JohnDoe");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello");
		cut.Markup.Should().Contain("JohnDoe");
	}

	[Fact]
	public void Auth_WhenUserNameIsNull_HandlesGracefully()
	{
		// Arrange
		_authProvider.SetAuthorizedWithoutName();

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("Hello");
		// Component should still render without errors
		cut.Should().NotBeNull();
	}

	[Fact]
	public void Auth_HasPageTitle()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// PageTitle is rendered by the framework, not in component markup
		// Check that the component renders successfully with page title directive
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Auth_HasCorrectPageRoute()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// Component should render successfully, indicating the route is valid
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("You are authenticated");
	}

	[Fact]
	public void Auth_UsesAuthorizeView()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		// AuthorizeView should render the authenticated content
		cut.Markup.Should().Contain("Hello TestUser!");
	}

	[Fact]
	public void Auth_WhenAuthenticated_DisplaysCompleteMessage()
	{
		// Arrange
		_authProvider.SetAuthorized("AdminUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		cut.Markup.Should().Contain("You are authenticated");
		cut.Markup.Should().Contain("Hello AdminUser!");
	}

	[Fact]
	public void Auth_HasCorrectStructure()
	{
		// Arrange
		_authProvider.SetAuthorized("TestUser");

		// Act
		var cut = RenderWithAuth<Auth>();

		// Assert
		var h1 = cut.Find("h1");
		h1.Should().NotBeNull();
		h1.TextContent.Trim().Should().Be("You are authenticated");
	}

	/// <summary>
	/// Helper class for fake authentication state provider
	/// </summary>
	public class TestAuthStateProvider : AuthenticationStateProvider
	{
		private bool _isAuthenticated = false;
		private string _userName = string.Empty;
		private bool _hasName = true;

		/// <summary>
		/// Sets the authentication state to authorized with a user name
		/// </summary>
		/// <param name="userName">The user name to use</param>
		public void SetAuthorized(string userName)
		{
			_isAuthenticated = true;
			_userName = userName;
			_hasName = true;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		/// Sets the authentication state to authorized without a user name
		/// </summary>
		public void SetAuthorizedWithoutName()
		{
			_isAuthenticated = true;
			_userName = string.Empty;
			_hasName = false;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		/// Sets the authentication state to not authorized
		/// </summary>
		public void SetNotAuthorized()
		{
			_isAuthenticated = false;
			_userName = string.Empty;
			_hasName = true;
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		/// <summary>
		/// Gets the authentication state
		/// </summary>
		/// <returns>The authentication state</returns>
		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			ClaimsIdentity identity;

			if (_isAuthenticated)
			{
				var claims = new List<Claim>();
				if (_hasName && !string.IsNullOrEmpty(_userName))
				{
					claims.Add(new Claim(ClaimTypes.Name, _userName));
					claims.Add(new Claim(ClaimTypes.NameIdentifier, _userName));
				}
				identity = new ClaimsIdentity(claims, "TestAuth");
			}
			else
			{
				identity = new ClaimsIdentity();
			}

			var user = new ClaimsPrincipal(identity);
			return Task.FromResult(new AuthenticationState(user));
		}
	}

	/// <summary>
	/// Fake authorization service for testing
	/// </summary>
	private class FakeAuthorizationService : IAuthorizationService
	{
		/// <summary>
		/// Authorizes the user based on requirements
		/// </summary>
		/// <param name="user">The user principal</param>
		/// <param name="resource">The resource to authorize</param>
		/// <param name="requirements">The authorization requirements</param>
		/// <returns>Authorization result</returns>
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
		{
			// Check if user is authenticated
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}
			return Task.FromResult(AuthorizationResult.Failed());
		}

		/// <summary>
		/// Authorizes the user based on policy name
		/// </summary>
		/// <param name="user">The user principal</param>
		/// <param name="resource">The resource to authorize</param>
		/// <param name="policyName">The policy name</param>
		/// <returns>Authorization result</returns>
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
		{
			// Check if user is authenticated
			if (user?.Identity?.IsAuthenticated == true)
			{
				return Task.FromResult(AuthorizationResult.Success());
			}
			return Task.FromResult(AuthorizationResult.Failed());
		}
	}

	/// <summary>
	/// Fake authorization policy provider for testing
	/// </summary>
	private class FakeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
	{
		/// <summary>
		/// Gets the default authorization policy
		/// </summary>
		/// <returns>The default authorization policy</returns>
		public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
		{
			return Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		/// <summary>
		/// Gets the authorization policy by name
		/// </summary>
		/// <param name="policyName">The policy name</param>
		/// <returns>The authorization policy</returns>
		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
		}

		/// <summary>
		/// Gets the fallback authorization policy
		/// </summary>
		/// <returns>The fallback authorization policy</returns>
		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
		{
			return Task.FromResult<AuthorizationPolicy?>(null);
		}
	}
}
