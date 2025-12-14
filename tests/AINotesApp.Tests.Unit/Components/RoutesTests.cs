using System.Diagnostics.CodeAnalysis;
using AINotesApp.Components;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AINotesApp.Tests.Unit.Components;

/// <summary>
/// BUnit tests for Routes.razor
/// </summary>
[ExcludeFromCodeCoverage]
public class RoutesTests : BunitContext
{
    private readonly TestAuthStateProvider _authProvider;

    public RoutesTests()
    {
        _authProvider = new TestAuthStateProvider();
        Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
        // Provide required authorization services for AuthorizeRouteView
        Services.AddSingleton<IAuthorizationService, AllowAllAuthorizationService>();
        Services.AddSingleton<IOptions<AuthorizationOptions>>(Options.Create(new AuthorizationOptions()));
        Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
    }

    [Fact]
    public void Routes_ShouldRender_WithRouterAndAuthorizeRouteView()
    {
        // Arrange
        _authProvider.SetAuthorized("user1");

        // Act
        var cut = Render<Routes>();

        // Assert - verify that Home content is rendered through routing
        cut.Markup.Should().Contain("Hello, world!");
        cut.Markup.Should().Contain("Welcome to your new app.");
    }

    private class TestAuthStateProvider : AuthenticationStateProvider
    {
        private Task<AuthenticationState> _stateTask = Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => _stateTask;

        public void SetAuthorized(string name)
        {
            var identity = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, name)
            }, "test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            _stateTask = Task.FromResult(new AuthenticationState(principal));
            NotifyAuthenticationStateChanged(_stateTask);
        }
    }

    private sealed class AllowAllAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
            => Task.FromResult(AuthorizationResult.Success());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
            => Task.FromResult(AuthorizationResult.Success());
    }
}