# convert-to-auth0-auth-bootstrap

## Goal

Introduce Auth0 authentication dependencies, configuration, and middleware so the application authenticates users
through Auth0 instead of ASP.NET Core Identity cookies.

## Prerequisites

Make sure that the use is currently on the `convert-to-auth0-auth-bootstrap` branch before beginning implementation.
If not, move them to the correct branch. If the branch does not exist, create it from main.

### Step-by-Step Instructions

#### Step 1: Update project packages

- [ ] Open `AINotesApp/AINotesApp.csproj` and replace its contents with the code below.
- [ ] Copy and paste code below into `AINotesApp/AINotesApp.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>aspnet-AINotesApp-41c588d7-01bb-4a12-8a64-3e6c8198e9a3</UserSecretsId>
    <BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Auth0.AspNetCore.Authentication" Version="1.5.0" />
    <PackageReference Include="Blazored.TextEditor" Version="1.1.3" />
    <PackageReference Include="MediatR" Version="14.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="10.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="10.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Build.Tasks.Core" Version="18.0.2" />
    <PackageReference Include="OpenAI" Version="2.7.0" />
  </ItemGroup>

</Project>
```

##### Step 1 Verification Checklist

- [ ] `dotnet restore AINotesApp/AINotesApp.csproj` completes without errors.
- [ ] `dotnet list AINotesApp/AINotesApp.csproj package` shows the Auth0 and OpenID Connect packages.

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 2: Add Auth0 config to appsettings.json

- [ ] Open `AINotesApp/appsettings.json` and replace its contents with the code below.
- [ ] Copy and paste code below into `AINotesApp/appsettings.json`:

```json
{
	"AllowedHosts": "*",
	"ConnectionStrings": {
		"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AINotesAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
	},
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft.AspNetCore": "Warning"
		}
	},
	"Auth0": {
		"Domain": "dev-ainotes.us.auth0.com",
		"ClientId": "A1NotesClient",
		"Audience": "https://api.ainotesapp.local",
		"CallbackPath": "/auth/callback",
		"LogoutPath": "/auth/logout"
	}
}
```

##### Step 2 Verification Checklist

- [ ] JSON validates (no trailing commas, braces matched).
- [ ] `dotnet build AINotesApp/AINotesApp.csproj` still succeeds.

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 3: Add Auth0 config to appsettings.Development.json

- [ ] Open `AINotesApp/appsettings.Development.json` and replace its contents with the code below.
- [ ] Copy and paste code below into `AINotesApp/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Auth0": {
    "Domain": "dev-ainotes.us.auth0.com",
    "ClientId": "A1NotesClient",
    "Audience": "https://api.ainotesapp.local",
    "CallbackPath": "/auth/callback",
    "LogoutPath": "/auth/logout"
  }
}
```

##### Step 3 Verification Checklist

- [ ] JSON validates and preserves the logging overrides.
- [ ] Configuration binding works: launching the app reads Dev-specific Auth0 paths.

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 4: Rewire Program.cs for Auth0

- [ ] Open `AINotesApp/Program.cs` and replace its contents with the code below.
- [ ] Copy and paste code below into `AINotesApp/Program.cs`:

```csharp
using Auth0.AspNetCore.Authentication;
using AINotesApp.Components;
using AINotesApp.Data;
using AINotesApp.Services.Ai;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"] ?? string.Empty;
        options.ClientId = builder.Configuration["Auth0:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        options.Scope = "openid profile email";
        options.CallbackPath = builder.Configuration["Auth0:CallbackPath"] ?? "/auth/callback";
        options.LogoutPath = builder.Configuration["Auth0:LogoutPath"] ?? "/auth/logout";
    })
    .WithAccessToken(options =>
    {
        var audience = builder.Configuration["Auth0:Audience"];
        if (!string.IsNullOrWhiteSpace(audience))
        {
            options.Audience = audience;
        }
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("NotesAdmin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "notes.admin"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure OpenAI options
builder.Services.Configure<AiServiceOptions>(
    builder.Configuration.GetSection(AiServiceOptions.SectionName));

// Register AI Service
builder.Services.AddScoped<IAiService, OpenAiService>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AINotesApp.Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make the implicit Program class accessible to tests
namespace AINotesApp
{
    /// <summary>
    /// Partial Program class to enable test accessibility for the application's entry point.
    /// </summary>
    public partial class Program { }
}
```

##### Step 4 Verification Checklist

- [ ] `dotnet build AINotesApp/AINotesApp.csproj` succeeds.
- [ ] App startup logs show Auth0 middleware registered (look for `Auth0.AspNetCore.Authentication` in output).
- [ ] Navigating to `/auth/login` redirects to the Auth0 tenant (may fail without valid credentials, but redirect must
  occur).

#### Step 4 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.

#### Step 5: Persist Auth0 secrets & smoke test

- [ ] Store the Auth0 client secret with `dotnet user-secrets`:

```bash
dotnet user-secrets set "Auth0:ClientSecret" "PASTE_AUTH0_CLIENT_SECRET" -p AINotesApp/AINotesApp.csproj
```

- [ ] Run a full build to verify everything compiles with the new configuration:

```bash
dotnet build AINotesApp/AINotesApp.csproj
```

##### Step 5 Verification Checklist

- [ ] User-secrets command reports `Successfully saved Auth0:ClientSecret`.
- [ ] Build passes with Auth0 configuration in place.

#### Step 5 STOP & COMMIT

**STOP & COMMIT:** Agent must stop here and wait for the user to test, stage, and commit the change.