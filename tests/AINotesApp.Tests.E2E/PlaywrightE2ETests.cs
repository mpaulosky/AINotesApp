using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace AINotesApp.Tests.E2E;

/// <summary>
/// End-to-end tests using Playwright with WebApplicationFactory.
/// The application is started automatically for each test.
/// Tests the complete user journey: registration, login, and CRUD operations for notes.
/// </summary>
public class PlaywrightE2ETests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private string? _serverUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightE2ETests"/> class
    /// </summary>
    public PlaywrightE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Ensure the factory is initialized (container started)
        await _factory.InitializeAsync();

        // Create the client with WebApplicationFactoryClientOptions
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost", UriKind.Absolute)
        };
        
        var client = _factory.CreateClient(clientOptions);
        _serverUrl = client.BaseAddress!.ToString().TrimEnd('/');

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        
        // Launch browser (headless mode for CI/CD)
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        
        // Create a new context
        _context = await _browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true // Ignore certificate errors in test environment
        });
        
        // Create a new page
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
        // Don't dispose the factory here - it's managed by xUnit class fixture
    }

    [Fact]
    public async Task HomePage_Loads_Successfully()
    {
        // Given
        var baseUrl = _serverUrl;

        // When
        await _page!.GotoAsync(baseUrl!);

        // Then
        var title = await _page.TitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task NavigateToLogin_ShowsLoginPage()
    {
        // Given
        var baseUrl = _serverUrl;

        // When
        await _page!.GotoAsync($"{baseUrl}/Account/Login");

        // Then
        var heading = await _page.Locator("h1").TextContentAsync();
        heading.Should().Contain("Log in");
    }

    [Fact]
    public async Task NavigateToRegister_ShowsRegisterPage()
    {
        // Given
        var baseUrl = _serverUrl;

        // When
        await _page!.GotoAsync($"{baseUrl}/Account/Register");

        // Then
        var heading = await _page.Locator("h1").TextContentAsync();
        heading.Should().Contain("Register");
    }

    [Fact]
    public async Task CompleteUserJourney_RegisterLoginCreateEditDeleteNote()
    {
        // Given - Generate unique user credentials
        var timestamp = DateTimeOffset.UtcNow.Ticks;
        var email = $"testuser{timestamp}@example.com";
        var password = "Test@123456";

        // When/Then - Register a new user
        await _page!.GotoAsync($"{_serverUrl}/Account/Register");
        
        await _page.WaitForSelectorAsync("input[name='Input.Email']");
        await _page.FillAsync("input[name='Input.Email']", email);
        await _page.FillAsync("input[name='Input.Password']", password);
        await _page.FillAsync("input[name='Input.ConfirmPassword']", password);
        await _page.ClickAsync("button[type='submit']");

        // Wait for registration to complete (may redirect to confirm email or login)
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Then - Verify registration succeeded
        var currentUrl = _page.Url;
        currentUrl.Should().Contain("RegisterConfirmation", "User should be redirected to confirmation page");

        // When - Navigate to login and authenticate
        await _page.GotoAsync($"{_serverUrl}/Account/Login");
        await _page.WaitForSelectorAsync("input[name='Input.Email']");
        await _page.FillAsync("input[name='Input.Email']", email);
        await _page.FillAsync("input[name='Input.Password']", password);
        await _page.ClickAsync("button[type='submit']");
        
        // Wait for login to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify login succeeded (should redirect to home)
        currentUrl = _page.Url;
        currentUrl.Should().NotContain("Login", "User should be redirected away from login page");

        // When - Create a new note
        await _page.GotoAsync($"{_serverUrl}/notes/create");
        await _page.WaitForSelectorAsync("input[name='Title']");
        
        var noteTitle = $"E2E Test Note {timestamp}";
        var noteContent = "This is a test note created by E2E tests.";
        
        await _page.FillAsync("input[name='Title']", noteTitle);
        await _page.FillAsync("textarea[name='Content']", noteContent);
        await _page.ClickAsync("button[type='submit']");
        
        // Wait for note creation
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify note was created (should redirect to notes list)
        currentUrl = _page.Url;
        currentUrl.Should().Contain("/notes", "Should redirect to notes page after creation");
        
        var pageContent = await _page.ContentAsync();
        pageContent.Should().Contain(noteTitle, "Created note should appear in the list");

        // When - Click on the note to view details
        await _page.ClickAsync($"text={noteTitle}");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify note details are displayed
        currentUrl = _page.Url;
        currentUrl.Should().Match(url => url.Contains("/notes/") && !url.EndsWith("/notes"), 
            "Should be on note details page");
        
        pageContent = await _page.ContentAsync();
        pageContent.Should().Contain(noteTitle);
        pageContent.Should().Contain(noteContent);

        // When - Edit the note
        await _page.ClickAsync("a:has-text('Edit')");
        await _page.WaitForSelectorAsync("input[name='Title']");
        
        var updatedTitle = $"{noteTitle} - Updated";
        var updatedContent = $"{noteContent} - Updated content.";
        
        await _page.FillAsync("input[name='Title']", updatedTitle);
        await _page.FillAsync("textarea[name='Content']", updatedContent);
        await _page.ClickAsync("button[type='submit']");
        
        // Wait for update to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify note was updated
        pageContent = await _page.ContentAsync();
        pageContent.Should().Contain(updatedTitle, "Updated title should be displayed");
        pageContent.Should().Contain(updatedContent, "Updated content should be displayed");

        // When - Delete the note
        await _page.ClickAsync("button:has-text('Delete')");
        
        // Confirm deletion if there's a confirmation dialog
        _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        
        // Wait for deletion to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify note was deleted (should redirect to notes list)
        currentUrl = _page.Url;
        currentUrl.Should().Contain("/notes", "Should redirect to notes list after deletion");
        
        pageContent = await _page.ContentAsync();
        pageContent.Should().NotContain(updatedTitle, "Deleted note should not appear in the list");
    }

    [Fact]
    public async Task Registration_WithInvalidData_ShowsValidationErrors()
    {
        // Given
        await _page!.GotoAsync($"{_serverUrl}/Account/Register");
        await _page.WaitForSelectorAsync("input[name='Input.Email']");

        // When - Submit with mismatched passwords
        await _page.FillAsync("input[name='Input.Email']", "test@example.com");
        await _page.FillAsync("input[name='Input.Password']", "Password123!");
        await _page.FillAsync("input[name='Input.ConfirmPassword']", "DifferentPassword!");
        await _page.ClickAsync("button[type='submit']");

        // Wait for validation
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify validation error is shown
        var pageContent = await _page.ContentAsync();
        pageContent.Should().Contain("password", "Should show password validation error");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsError()
    {
        // Given
        await _page!.GotoAsync($"{_serverUrl}/Account/Login");
        await _page.WaitForSelectorAsync("input[name='Input.Email']");

        // When - Try to login with invalid credentials
        await _page.FillAsync("input[name='Input.Email']", "nonexistent@example.com");
        await _page.FillAsync("input[name='Input.Password']", "WrongPassword123!");
        await _page.ClickAsync("button[type='submit']");

        // Wait for response
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Verify error message is shown
        var pageContent = await _page.ContentAsync();
        pageContent.Should().Contain("Invalid", "Should show invalid login error");
    }

    [Fact]
    public async Task CreateNote_RequiresAuthentication()
    {
        // Given - Not authenticated
        await _page!.GotoAsync($"{_serverUrl}/notes/create");

        // Wait for redirect
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Then - Should redirect to login
        var currentUrl = _page.Url;
        currentUrl.Should().Contain("Account/Login", "Should redirect to login when not authenticated");
    }
}
