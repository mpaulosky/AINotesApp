# Test Helpers Documentation

This directory contains shared test utilities and helpers to promote code reuse and follow SOLID principles across the test suite.

## FakeAuthenticationStateProvider

A consolidated, reusable authentication provider for testing Blazor components and services that require authentication.

### Location
- **Implementation**: `AINotesApp.Tests.Unit.Fakes.FakeAuthenticationStateProvider`
- **Helper Methods**: `AINotesApp.Tests.Unit.Helpers.TestAuthHelper`

### Features

1. **Dynamic State Management**: Can change authentication state during test execution
2. **Comprehensive Claims**: Includes `Name`, `NameIdentifier`, and `sub` (Auth0) claims
3. **Role Support**: Can assign multiple roles to test users
4. **Notification Support**: Properly notifies components when authentication state changes

### Usage Examples

#### Example 1: Basic Setup with Dynamic State Changes

```csharp
public class MyComponentTests : BunitContext
{
    private readonly FakeAuthenticationStateProvider _authProvider = new();

    public MyComponentTests()
    {
        Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
    }

    [Fact]
    public void Test_RequiresAuthentication()
    {
        // Arrange: Start unauthenticated
        var cut = Render<MyComponent>();

        // Assert: Verify unauthorized behavior
        cut.Markup.Should().Contain("Please log in");

        // Act: Authenticate user
        _authProvider.SetAuthorized("testuser", "user-123");

        // Assert: Verify authenticated behavior
        cut.WaitForAssertion(() => 
            cut.Markup.Should().Contain("Welcome testuser"));
    }
}
```

#### Example 2: Using TestAuthHelper for Static Authentication

```csharp
public class MyComponentTests : BunitContext
{
    public MyComponentTests()
    {
        // Register authenticated user with roles
        TestAuthHelper.RegisterTestAuthentication(
            Services, 
            "admin-user", 
            new[] { "Admin", "User" });
    }

    [Fact]
    public void Test_AdminFeature()
    {
        var cut = Render<AdminComponent>();
        cut.Markup.Should().Contain("Admin Panel");
    }
}
```

#### Example 3: Dynamic Authentication with TestAuthHelper

```csharp
public class MyComponentTests : BunitContext
{
    private readonly FakeAuthenticationStateProvider _authProvider;

    public MyComponentTests()
    {
        _authProvider = TestAuthHelper.RegisterDynamicTestAuthentication(Services);
    }

    [Fact]
    public void Test_LoginLogout()
    {
        var cut = Render<MyComponent>();

        // Test unauthenticated state
        cut.Markup.Should().Contain("Please log in");

        // Authenticate
        _authProvider.SetAuthorized("user1", "user-id-123", new[] { "User" });
        cut.WaitForAssertion(() => 
            cut.Markup.Should().Contain("Welcome user1"));

        // Log out
        _authProvider.SetNotAuthorized();
        cut.WaitForAssertion(() => 
            cut.Markup.Should().Contain("Please log in"));
    }
}
```

### API Reference

#### FakeAuthenticationStateProvider

**Constructors**:
- `FakeAuthenticationStateProvider()`: Creates unauthenticated provider
- `FakeAuthenticationStateProvider(string userName, string[]? roles = null, bool isAuthenticated = true)`: Creates provider with initial state

**Methods**:
- `SetAuthorized(string userName, string? userId = null, string[]? roles = null)`: Sets user as authenticated with specified claims
- `SetNotAuthorized()`: Sets user as unauthenticated and clears all claims
- `GetAuthenticationStateAsync()`: Returns current authentication state (inherited from base class)

**Claims Provided**:
- `ClaimTypes.Name`: Username
- `ClaimTypes.NameIdentifier`: User ID
- `"sub"`: Auth0 subject claim (same as user ID)
- `ClaimTypes.Role`: One claim per role (if roles provided)

#### TestAuthHelper

**Static Methods**:
- `RegisterTestAuthentication(IServiceCollection services, string userName, string[] roles)`: Registers static authenticated user
- `RegisterDynamicTestAuthentication(IServiceCollection services)`: Returns provider instance for dynamic state changes
- `RegisterAuthenticatedUser(IServiceCollection services, string userName, string userId, string[]? roles = null)`: Registers user with explicit user ID

## Benefits of Consolidation

### Before (Duplicated Code)
- ❌ 7+ different `TestAuthStateProvider` implementations across test files
- ❌ Inconsistent claim configurations
- ❌ Maintenance burden when authentication logic changes
- ❌ Violates DRY (Don't Repeat Yourself) principle

### After (Consolidated)
- ✅ Single source of truth in `FakeAuthenticationStateProvider`
- ✅ Consistent claims across all tests (`Name`, `NameIdentifier`, `sub`)
- ✅ Easy to maintain and enhance
- ✅ Follows SOLID principles (Single Responsibility, Open/Closed)
- ✅ Comprehensive XML documentation
- ✅ Flexible usage patterns via helper methods

## FakeAuthorizationService and FakeAuthorizationPolicyProvider

Shared authorization implementations for testing components that use `[Authorize]` attributes or policy-based authorization.

### Location
- **Authorization Service**: `AINotesApp.Tests.Unit.Fakes.FakeAuthorizationService`
- **Policy Provider**: `AINotesApp.Tests.Unit.Fakes.FakeAuthorizationPolicyProvider`

### Features

- **Simple Authorization Logic**: Authenticated users are authorized, unauthenticated users are not
- **Policy Support**: Provides default authenticated user policies
- **Consistent Behavior**: All policies require authenticated users

### Usage Example

```csharp
public class MyAuthorizedComponentTests : BunitContext
{
    private readonly FakeAuthenticationStateProvider _authProvider;

    public MyAuthorizedComponentTests()
    {
        // Register authentication and authorization services
        _authProvider = new FakeAuthenticationStateProvider();
        Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
    }

    [Fact]
    public void Test_AuthorizedComponent()
    {
        // Arrange: Set user as authorized
        _authProvider.SetAuthorized("testuser", "user-123");

        // Act: Render component with [Authorize] attribute
        var cut = Render<SecureComponent>();

        // Assert: Component renders for authorized user
        cut.Markup.Should().Contain("Secure Content");
    }

    [Fact]
    public void Test_UnauthorizedAccess()
    {
        // Arrange: User not authorized
        _authProvider.SetNotAuthorized();

        // Act: Attempt to render authorized component
        var cut = Render<SecureComponent>();

        // Assert: Authorization prevents access
        cut.Markup.Should().NotContain("Secure Content");
    }
}
```

### Why Use These Fakes?

- ✅ **Consistent Authorization Behavior**: All tests use the same authorization logic
- ✅ **Simple to Use**: Just register the services in your test constructor
- ✅ **Works with [Authorize]**: Supports both attribute-based and policy-based authorization
- ✅ **Maintained in One Place**: Changes to authorization logic affect all tests uniformly

## Migration Guide

If you have an existing test with a local `TestAuthStateProvider`:

**Before**:
```csharp
private class TestAuthStateProvider : AuthenticationStateProvider
{
    private Task<AuthenticationState> _state = // ...
    public void SetAuthorized(string userId) { /* ... */ }
    public override Task<AuthenticationState> GetAuthenticationStateAsync() { /* ... */ }
}
```

**After**:
```csharp
// Add using statement
using AINotesApp.Tests.Unit.Fakes;

// Replace instantiation
private readonly FakeAuthenticationStateProvider _authProvider = new();

// Remove local TestAuthStateProvider class
```

## SOLID Principles Applied

1. **Single Responsibility Principle (SRP)**: `FakeAuthenticationStateProvider` has one job - provide authentication state for tests
2. **Open/Closed Principle (OCP)**: Extendable through inheritance, closed for modification
3. **Dependency Inversion Principle (DIP)**: Tests depend on abstractions (`AuthenticationStateProvider`), not concrete implementations
4. **Don't Repeat Yourself (DRY)**: Eliminates code duplication across test files

## Testing Best Practices

1. ✅ **Use shared test doubles**: Leverage `FakeAuthenticationStateProvider` instead of creating new ones
2. ✅ **Centralize test helpers**: Add reusable utilities to the `Helpers` directory
3. ✅ **Document test patterns**: Include XML documentation and usage examples
4. ✅ **Follow AAA pattern**: Arrange, Act, Assert in all tests
5. ✅ **Use meaningful names**: Test names should describe what is being tested
6. ✅ **Test one thing**: Each test should verify a single behavior

## Contributing

When adding new test helpers:

1. Place shared fakes in `Fakes/` directory
2. Place utility methods in `Helpers/` directory
3. Add comprehensive XML documentation
4. Provide usage examples in this README
5. Follow project naming conventions (see `.github/instructions/`)
6. Add `[ExcludeFromCodeCoverage]` attribute to test helper classes
