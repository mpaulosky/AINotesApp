using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Unit.Helpers;

public static class TestAuthHelper
{
    /// <summary>
    /// Registers a test authentication provider for bUnit tests.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="userName">The test user name.</param>
    /// <param name="roles">The roles for the test user.</param>
    public static void RegisterTestAuthentication(IServiceCollection services, string userName, string[] roles)
    {
        services.AddSingleton<AuthenticationStateProvider>(
            new Fakes.FakeAuthenticationStateProvider(userName, roles));
    }
}
