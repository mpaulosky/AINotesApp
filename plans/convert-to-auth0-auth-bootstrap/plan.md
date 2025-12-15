# Plan: Auth0 Auth Bootstrap

## Scope

Introduce Auth0 authentication packages, configuration, and middleware so the app can authenticate users via Auth0 while
Identity services remain untouched elsewhere.

## Assumptions

- Auth0 tenant domain: `dev-ainotes.us.auth0.com`
- Regular Web App Client ID: `A1NotesClient`
- Client Secret stored via user-secrets as `Auth0:ClientSecret`
- Audience: `https://api.ainotesapp.local`
- Callback path `/auth/callback`, logout path `/auth/logout`

## Steps

1. **Update project packages**
    - File: `AINotesApp/AINotesApp.csproj`
    - Remove Identity package references (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`,
      `Microsoft.AspNetCore.Identity.UI`).
    - Add `Auth0.AspNetCore.Authentication` (v1.5.0) and ensure `Microsoft.AspNetCore.Authentication.OpenIdConnect` is
      referenced.
    - Keep EF Core packages intact.

2. **Add Auth0 configuration blocks**
    - Files: `AINotesApp/appsettings.json`, `AINotesApp/appsettings.Development.json`
    - Insert:
      ```json
      "Auth0": {
        "Domain": "dev-ainotes.us.auth0.com",
        "ClientId": "A1NotesClient",
        "Audience": "https://api.ainotesapp.local",
        "CallbackPath": "/auth/callback",
        "LogoutPath": "/auth/logout"
      }
      ```
    - Remove Identity cookie scheme configuration sections (e.g., `"Identity": {...}`) if present.
    - Ensure logging/config structure stays valid JSON.

3. **Store secrets locally**
    - Command (documentation only, not scripted):
      `dotnet user-secrets set "Auth0:ClientSecret" "<value>" -p AINotesApp/AINotesApp.csproj`
    - Add note to `docs/SECURITY.md` referencing the new secret key name (handled in later plan but mention).

4. **Wire middleware in Program.cs**
    - File: `AINotesApp/Program.cs`
    - Remove all Identity service registrations: `AddIdentityCore`, `.AddEntityFrameworkStores`, `.AddSignInManager`,
      `AddIdentityCookies`, and `IdentityRevalidatingAuthenticationStateProvider` registrations.
    - Retain `AddDbContext<ApplicationDbContext>` and MediatR registrations.
    - Add `builder.Services.AddAuth0WebAppAuthentication(options => { ... })` configuring Domain, ClientId,
      CallbackPath, LogoutPath, default scopes `openid profile email`, plus
      `.WithAccessToken(options => options.Audience = "https://api.ainotesapp.local" );`.
    - Update authentication/authorization pipeline: `builder.Services.AddAuthorizationBuilder()` and ensure admin policy
      checks `notes.admin` role (claim type `"http://schemas.microsoft.com/ws/2008/06/identity/claims/roles"`).
    - Replace `builder.Services.AddCascadingAuthenticationState<IdentityRevalidatingAuthenticationStateProvider>()`
      usage with `builder.Services.AddCascadingAuthenticationState()` referencing the default provider from
      `AuthenticationStateProvider` registered by Auth0.
    - In the middleware pipeline, ensure `app.UseAuthentication(); app.UseAuthorization();` remain.

5. **Smoke test**
    - Run `dotnet build AINotesApp/AINotesApp.csproj` to confirm packages/config compile successfully.

## Deliverables

- All edits confined to `AINotesApp.csproj`, `appsettings*.json`, and `Program.cs`.
- No component/page changes in this plan.