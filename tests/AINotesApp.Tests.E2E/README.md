# End-to-End Tests with Playwright

This project contains end-to-end tests using Microsoft Playwright for browser automation.

## Prerequisites

1. Install Playwright browsers:
   ```bash
   pwsh bin/Debug/net10.0/playwright.ps1 install
   ```

   Or on Linux/Mac:
   ```bash
   ./bin/Debug/net10.0/playwright.sh install
   ```

## Running the Tests

### Step 1: Start the Application

Before running E2E tests, you need to start the AINotesApp application:

```bash
cd AINotesApp
dotnet run
```

The application should be running at `https://localhost:5001` (or the configured port).

### Step 2: Run the Tests

In a separate terminal, run the E2E tests:

```bash
cd tests/AINotesApp.Tests.E2E
dotnet test
```

## Enabling Tests

By default, E2E tests are skipped because they require the application to be running.

To enable a test, remove the `Skip` parameter from the `[Fact]` attribute:

```csharp
// Before
[Fact(Skip = "Requires application to be running")]

// After
[Fact]
```

## Writing New E2E Tests

Follow the Given-When-Then pattern:

```csharp
[Fact]
public async Task Example_Test_Scenario()
{
    // Given - Setup preconditions
    var baseUrl = "https://localhost:5001";
    
    // When - Perform actions
    await _page!.GotoAsync(baseUrl);
    await _page.ClickAsync("button[type='submit']");
    
    // Then - Assert expected outcomes
    var result = await _page.Locator(".result").TextContentAsync();
    result.Should().Be("Expected Value");
}
```

## Playwright Documentation

For more information on using Playwright:
- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Locators Guide](https://playwright.dev/dotnet/docs/locators)
- [Assertions](https://playwright.dev/dotnet/docs/test-assertions)
