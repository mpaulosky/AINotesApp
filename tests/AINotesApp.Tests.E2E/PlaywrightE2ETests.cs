using FluentAssertions;
using Microsoft.Playwright;

namespace AINotesApp.Tests.E2E;

/// <summary>
/// End-to-end tests using Playwright.
/// Note: These tests require the application to be running.
/// </summary>
public class PlaywrightE2ETests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public async Task InitializeAsync()
    {
        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        
        // Launch browser (headless mode for CI/CD)
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        
        // Create a new context
        _context = await _browser.NewContextAsync();
        
        // Create a new page
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact(Skip = "Requires application to be running. Enable manually for E2E testing.")]
    public async Task HomePage_Loads_Successfully()
    {
        // Given
        var baseUrl = "https://localhost:5001"; // Adjust as needed

        // When
        await _page!.GotoAsync(baseUrl);

        // Then
        var title = await _page.TitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires application to be running. Enable manually for E2E testing.")]
    public async Task NavigateToLogin_ShowsLoginPage()
    {
        // Given
        var baseUrl = "https://localhost:5001";

        // When
        await _page!.GotoAsync($"{baseUrl}/Account/Login");

        // Then
        var heading = await _page.Locator("h1").TextContentAsync();
        heading.Should().Contain("Log in");
    }

    [Fact(Skip = "Requires application to be running. Enable manually for E2E testing.")]
    public async Task NavigateToRegister_ShowsRegisterPage()
    {
        // Given
        var baseUrl = "https://localhost:5001";

        // When
        await _page!.GotoAsync($"{baseUrl}/Account/Register");

        // Then
        var heading = await _page.Locator("h1").TextContentAsync();
        heading.Should().Contain("Register");
    }
}
