using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace AINotesApp.Tests.Unit.Fakes;

[ExcludeFromCodeCoverage]
public class FakeAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly string _userName;
    private readonly string[] _roles;
    private readonly bool _isAuthenticated;

    public FakeAuthenticationStateProvider(string userName, string[] roles, bool isAuthenticated = true)
    {
        _userName = userName;
        _roles = roles ?? new string[0];
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
