# convert-to-auth0-ui

## Goal

Remove ASP.NET Core Identity UI components and replace them with Auth0-focused login/logout experiences inside the
Blazor layouts and router.

## Prerequisites

Make sure that the use is currently on the `convert-to-auth0-ui` branch before beginning implementation.
If not, move them to the correct branch. If the branch does not exist, create it from main.

### Step-by-Step Instructions

#### Step 1: Clean up Identity components

- [ ] Delete the entire directory `AINotesApp/Components/Account/Pages/` from the repository.
- [ ] Delete the following files:
    - `AINotesApp/Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`
    - `AINotesApp/Components/Account/IdentityRedirectManager.cs`
    - `AINotesApp/Components/Account/IdentityRevalidatingAuthenticationStateProvider.cs`
    - `AINotesApp/Components/Account/IdentityNoOpEmailSender.cs`
    - `AINotesApp/Components/Account/PasskeyInputModel.cs`
    - `AINotesApp/Components/Account/PasskeyOperation.cs`

##### Step 1 Verification Checklist

- [ ] `git status` shows the listed files/directories removed.
- [ ] `dotnet build AINotesApp/AINotesApp.csproj` still succeeds.

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 2: Add Auth0 account components

- [ ] Create `AINotesApp/Components/Account/Login.razor` with the following content:

```razor
@inject NavigationManager Navigation

<button class="btn btn-primary" @onclick="SignIn">Sign in</button>

@code {
    private void SignIn()
    {
        Navigation.NavigateTo("auth/login", forceLoad: true);
    }
}
```

- [ ] Create `AINotesApp/Components/Account/Logout.razor` with:

```razor
<form method="post" action="/auth/logout">
    <button class="btn btn-link" type="submit">Log out</button>
</form>
```

- [ ] Create `AINotesApp/Components/Account/AccessDenied.razor` with:

```razor
<div class="alert alert-warning mt-3">
    <p>You do not have access to this content.</p>
    <a href="/" class="btn btn-secondary">Go home</a>
</div>
```

##### Step 2 Verification Checklist

- [ ] `dotnet build` succeeds (new components compile).
- [ ] Navigating to `/auth/login` via the Login button redirects (Auth0 may show an error without proper settings, but
  navigation must occur).

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 3: Update `_Imports.razor`

- [ ] Open `AINotesApp/Components/_Imports.razor` and replace its contents with:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using AINotesApp
@using AINotesApp.Components
@using AINotesApp.Data
@using AINotesApp.Services.Ai
@using Blazored.TextEditor
```

##### Step 3 Verification Checklist

- [ ] No Identity namespaces remain in `_Imports.razor`.
- [ ] Build still succeeds.

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 4: Refresh NavMenu for Auth0

- [ ] Replace the contents of `AINotesApp/Components/Layout/NavMenu.razor` with:

```razor
<nav class="sidebar">
    <div class="top-row ps-3 navbar navbar-dark">
        <a class="navbar-brand" href="/">AI Notes</a>
    </div>

    <div class="nav-links">
        <AuthorizeView>
            <Authorized>
                <div class="user-info">
                    Hello, @GetUserName(context.User)
                </div>
                <a class="nav-link" href="/notes">Notes</a>
                <a class="nav-link" href="/notes/create">New Note</a>
                <a class="nav-link" href="/admin/seed">Seed Notes</a>
                <Components.Account.Logout />
            </Authorized>
            <NotAuthorized>
                <Components.Account.Login />
            </NotAuthorized>
        </AuthorizeView>
    </div>
</nav>

@code {
    private static string GetUserName(ClaimsPrincipal principal)
    {
        return principal.Identity?.Name ?? principal.FindFirst("name")?.Value ?? "user";
    }
}
```

- [ ] Update `AINotesApp/Components/Layout/NavMenu.razor.css` to include button styling:

```css
.sidebar {
    display: flex;
    flex-direction: column;
    width: 260px;
    background-color: #1b1f22;
    color: #fff;
}

.nav-links {
    padding: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}

.nav-link {
    color: #fff;
    text-decoration: none;
}

.nav-link:hover {
    text-decoration: underline;
}

.user-info {
    margin-bottom: 1rem;
    font-weight: 600;
}

form[action="/auth/logout"] {
    display: inline;
}
```

##### Step 4 Verification Checklist

- [ ] Running the app shows the new sidebar with Auth0 login/logout buttons.
- [ ] Authenticated state shows user name derived from claims.

#### Step 4 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 5: Update routing and layouts

- [ ] Replace `AINotesApp/Components/Routes.razor` with:

```razor
@using AINotesApp.Components.Account

<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
                <NotAuthorized>
                    <AuthorizeView>
                        <Authorized>
                            <AccessDenied />
                        </Authorized>
                        <NotAuthorized>
                            <Login />
                        </NotAuthorized>
                    </AuthorizeView>
                </NotAuthorized>
                <Authorizing>
                    <p>Authorizing...</p>
                </Authorizing>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="typeof(MainLayout)">
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

- [ ] Ensure `AINotesApp/Components/App.razor` contains:

```razor
<Routes />
``` 

- [ ] Update `AINotesApp/Components/Layout/MainLayout.razor` to:

```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

##### Step 5 Verification Checklist

- [ ] Unauthorized pages now render the Login component.
- [ ] Authenticated-but-unauthorized users see the AccessDenied message.

#### Step 5 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 6: Reconnect modal review

- [ ] Confirm `AINotesApp/Components/Layout/ReconnectModal.razor` and `.js` still compile/work without Identity
  dependencies (no code changes required unless build fails).

##### Step 6 Verification Checklist

- [ ] Circuit disconnects show the reconnect modal as before.

#### Step 6 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 7: Manual verification

- [ ] Run the app: `dotnet watch run --project AINotesApp/AINotesApp.csproj`
- [ ] Navigate to `/notes` while signed out and verify the Login component renders.
- [ ] Click “Sign in” and ensure the browser is redirected to `https://dev-ainotes.us.auth0.com/...`.
- [ ] After Auth0 login, confirm the sidebar shows the user’s display name and the Logout button posts to
  `/auth/logout`.

##### Step 7 Verification Checklist

- [ ] Manual browsing confirms new login/log-out UI functions.
- [ ] No console errors related to removed Identity components.

#### Step 7 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.