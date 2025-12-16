# Plan: Auth0 UI Integration

## Scope

Replace Identity-specific Razor components/layout elements with Auth0-aware login, logout, and authorization UI while
leaving data and backend logic untouched.

## Dependencies

- Plan `convert-to-auth0-auth-bootstrap` completed (Auth0 middleware available).

## Steps

1. **Remove Identity component artifacts**
    - Delete files under `AINotesApp/Components/Account/` that are Identity-specific:
        - `IdentityComponentsEndpointRouteBuilderExtensions.cs`
        - `IdentityRedirectManager.cs`
        - `IdentityRevalidatingAuthenticationStateProvider.cs`
        - `IdentityNoOpEmailSender.cs`
        - Entire `Components/Account/Pages/` folder (Login, Register, Manage, etc.).
    - Remove passkey support classes: `PasskeyInputModel.cs`, `PasskeyOperation.cs`.

2. **Add Auth0 Razor components**
    - Create `AINotesApp/Components/Account/Login.razor`
        - Inject `NavigationManager`.
        - Render a button that calls `Navigation.NavigateTo("auth/login", forceLoad: true)`.
    - Create `AINotesApp/Components/Account/Logout.razor`
        - Render a `<form method="post" action="/auth/logout">` submitting with a button.
    - Create `AINotesApp/Components/Account/AccessDenied.razor`
        - Display message and link to `/`.

3. **Update `_Imports.razor`**
    - Remove Identity namespaces (e.g., `Microsoft.AspNetCore.Identity`).
    - Include `using Microsoft.AspNetCore.Components.Authorization` if not already.

4. **Adjust layout/navigation**
    - File: `Components/Layout/NavMenu.razor`
        - Replace the `LoginDisplay` or Identity-specific logic with `<AuthorizeView>` blocks showing Auth0 user info.
        - Display `context.User.Identity?.Name ?? context.User.FindFirst("name")?.Value`.
        - Use new `Login`/`Logout` components for actions.
        - Remove Register link; optionally add link to Auth0 signup using
          `NavigationManager.NavigateTo("auth/login?screen_hint=signup", true)` inside Login component.
    - File: `Components/Layout/NavMenu.razor.css`
        - Ensure styles reference new elements (buttons/forms) if needed.

5. **Router and App adjustments**
    - File: `Components/Routes.razor`
        - Keep `<AuthorizeRouteView>` but update `NotAuthorized` content to render `<Components.Account.Login />` and
          `AccessDenied` message for authenticated-but-unauthorized cases.
    - File: `Components/App.razor`
        - Wrap router in `<CascadingAuthenticationState>` using default provider from Auth0.
    - File: `Components/Layout/MainLayout.razor`
        - Remove references to Identity-specific services such as `IdentityRedirectManager`.

6. **Reconnect Modal script check**
    - Verify `Components/Layout/ReconnectModal.razor` and `.js` still function without Identity dependencies; update
      text if it references Identity login.

7. **Manual verification**
    - Build solution and run (`dotnet watch run` optional) to ensure UI loads and login/logout buttons navigate to the
      new endpoints (Auth0 routes won’t succeed yet but navigation should occur).