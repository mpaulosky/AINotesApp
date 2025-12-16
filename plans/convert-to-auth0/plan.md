Plan: Migrate from ASP.NET Core Identity to Auth0

Goal
Replace the Identity + cookie stack with Auth0 OIDC authentication while preserving per-user data isolation and existing
UX.

Guiding Assumptions

- Auth0 tenant will expose:
    - Domain: `dev-ainotes.us.auth0.com`
    - Application (Regular Web App) Client ID: `A1NotesClient`
    - Application Client Secret is stored in user-secrets as `Auth0:ClientSecret`
    - API Identifier: `https://api.ainotesapp.local`
- Local callback path: `/auth/callback`, logout path: `/auth/logout`
- Auth0 RBAC enabled, with `notes.admin` role assigned to Seed Notes users.

Implementation Steps

Step 1: Introduce Auth0 configuration and packages

- Update `AINotesApp/Program.csproj`:
    - Remove Identity-specific package references (Microsoft.AspNetCore.Identity.EntityFrameworkCore,
      Microsoft.AspNetCore.Identity.UI).
    - Add `Auth0.AspNetCore.Authentication` (v1.5.0 or latest compatible) and
      `Microsoft.AspNetCore.Authentication.OpenIdConnect`.
- Update `nuget.config` only if new feeds are required (expect none).
- Verify `dotnet restore` succeeds.

Step 2: Define configuration values

- In `AINotesApp/appsettings.json` and `.Development.json`:
    - Add
      `"Auth0": { "Domain": "dev-ainotes.us.auth0.com", "ClientId": "A1NotesClient", "Audience": "https://api.ainotesapp.local", "CallbackPath": "/auth/callback", "LogoutPath": "/auth/logout" }`.
    - Remove Identity cookie options sections referencing `IdentityConstants`.
- Store `Auth0:ClientSecret` and any production ClientId overrides via `dotnet user-secrets`.
- Document the new secrets in `docs/SECURITY.md`.

Step 3: Rewire Program.cs for Auth0

- Remove calls to `AddDbContext<ApplicationDbContext>` overloads that inject Identity stores, and drop
  `AddIdentityCore`, `AddEntityFrameworkStores`, `AddSignInManager`, and Identity cookie configuration.
- Register `AddAuth0WebAppAuthentication(options => { ... })` with the domain/client settings above, configure
  `options.Scope` to include `openid profile email`, and set `options.CallbackPath`/`LogoutPath`.
- Add `.WithAccessToken(options => { options.Audience = "https://api.ainotesapp.local"; })` for downstream API calls.
- Replace `IdentityRevalidatingAuthenticationStateProvider` with
  `RevalidatingServerAuthenticationStateProvider<RemoteAuthenticationState>` from the Auth0 sample, or configure
  `CascadingAuthenticationState` to use `AuthenticationStateProvider` from the OIDC handler.
- Ensure authorization policies for admin operations check the `notes.admin` role claim emitted by Auth0.

Step 4: Remove Identity UI artifacts and add Auth0-facing components

- Delete `Components/Account/Identity*` files, `Pages/Account/*` razor pages, and passkey helpers.
- Add new Razor components:
    - `Components/Account/Login.razor`: invokes `NavigationManager.NavigateTo("auth/login", forceLoad: true)`.
    - `Components/Account/Logout.razor`: posts to `/auth/logout` and clears local state.
    - `Components/Account/AccessDenied.razor`: friendly message with link to home.
- Update `_Imports.razor` to remove Identity namespaces and add `using Auth0.AspNetCore.Authentication` if needed.

Step 5: Update layout and routing

- `Components/Layout/NavMenu.razor`:
    - Replace all `<LoginDisplay>` usages with explicit `<AuthorizeView>` blocks calling new Login/Logout components.
    - Display user info from `context.User.Identity?.Name ?? context.User.FindFirst("name")?.Value`.
- `Components/Routes.razor`:
    - Keep `<AuthorizeRouteView>` but update `NotAuthorized` content to direct users to `/auth/login`.
- `App.razor` and `MainLayout.razor`:
    - Remove Identity-specific JS interop and ensure `<CascadingAuthenticationState>` wraps the router using the
      Auth0-provided authentication state provider.

Step 6: Switch data layer and features to Auth0 subject IDs

- `Data/ApplicationDbContext.cs`:
    - Remove `IdentityDbContext<ApplicationUser>` inheritance; replace with `DbContext` plus explicit `DbSet<AppUser>`.
    - Define `AppUser` entity with `Auth0Subject` (string) as key, Name, Email, CreatedUtc.
- `Data/Note.cs` and all features under `Features/Notes/**`:
    - rename `OwnerId` to `OwnerSubject` (string) and update migrations.
    - Inside handlers pull user id via `IHttpContextAccessor.HttpContext!.User.FindFirst("sub")!.Value` or equivalent.
- Update migrations: create one migration to add `OwnerSubject` column + indexes, migrate data from old GUIDs using
  mapping table produced during user migration, drop Identity tables only after verification.

Step 7: Testing and helper updates

- `tests/AINotesApp.Tests.Unit/Fakes/FakeAuthenticationStateProvider.cs` and helpers:
    - Change fake claims principal to emit `sub`, `name`, `email`, `roles` claims consistent with Auth0 tokens.
- Update all component tests referencing `ClaimTypes.NameIdentifier` to use the new helper constant.
- Integration tests that spin up the server must register Auth0 authentication handlers via test configuration or use
  `TestAuth0AuthenticationHandler` to simulate tokens.
- Run `dotnet test` for all three test projects.

Step 8: Documentation, automation, migration scripts

- `docs/SECURITY.md` and `README.md`:
    - Describe Auth0 integration steps, required tenant configuration, and secrets.
    - Provide instructions for running `dotnet user-secrets set Auth0:ClientSecret "..."`.
- `plans/convert-to-auth0/migration-notes.md` (new):
    - Document Identity-to-Auth0 user export/import steps, including CSV format and Management API scopes (
      `create:users`, `update:users`).
- Update CI/CD workflows (if any) to inject Auth0 secrets as environment variables.

Verification

- After each step, run `dotnet build` to ensure compile success; after Step 7 run `dotnet test`.
- Manual test matrix (desktop + mobile): login, logout, access denied flow, CRUD notes, seed notes admin page, profile
  display, reconnection modal under auth context.

Rollback Considerations

- Keep Identity migrations and tables until Auth0 tenant has all users imported and a full backup of `AINotesApp`
  database exists.
- Feature flag the new auth flow via `UseAuth0Authentication` appsetting to allow toggling during staging tests.
- Document how to revert to the previous commit and restore Identity tables if severe Auth0 outage occurs.