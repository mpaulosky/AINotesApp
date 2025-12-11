# End-to-End Tests with Playwright and Testcontainers

This project contains end-to-end tests using Microsoft Playwright for browser automation with SQL Server Testcontainers for database isolation.

## Prerequisites

1. **Docker Desktop** must be installed and running:
   - Download from: <https://www.docker.com/products/docker-desktop>
   - Ensure Docker is running before executing tests

2. **Build the test project first** (required to generate Playwright scripts):

   ```bash
   dotnet build tests/AINotesApp.Tests.E2E
   ```

3. **Install Playwright browsers**:

   On Windows (PowerShell):

   ```powershell
   pwsh tests/AINotesApp.Tests.E2E/bin/Debug/net10.0/playwright.ps1 install
   ```

   On Linux/Mac:

   ```bash
   ./tests/AINotesApp.Tests.E2E/bin/Debug/net10.0/playwright.sh install
   ```

## Running the Tests

The tests automatically:

- Start a SQL Server 2022 container using Testcontainers
- Apply all EF Core migrations to create the database schema
- Start the AINotesApp with the containerized database
- Run Playwright browser tests
- Clean up the container after completion

Simply run:

```bash
dotnet test tests/AINotesApp.Tests.E2E
```

Or from the solution root:

```bash
dotnet test --filter "FullyQualifiedName~AINotesApp.Tests.E2E"
```

## How It Works

1. **Testcontainers.MsSql**:
   - Automatically pulls and starts a SQL Server 2022 Docker container
   - Provides an isolated database for each test run
   - Connection string is dynamically generated

2. **CustomWebApplicationFactory**:
   - Implements `IAsyncLifetime` to manage container lifecycle
   - Replaces production SQL Server connection with container connection
   - Applies EF Core migrations via `Database.Migrate()`
   - Provides a test server URL to Playwright

3. **PlaywrightE2ETests**:
   - Waits for container initialization before running tests
   - Executes browser-based tests against the running application
   - Automatically cleans up all resources after tests complete

## Benefits of This Approach

✅ **Real SQL Server database** (not in-memory mock)  
✅ **Production-like environment** with actual migrations  
✅ **Complete isolation** - each test run gets a fresh container  
✅ **Automatic cleanup** - containers are removed after tests  
✅ **CI/CD ready** - works anywhere Docker is available  
✅ **No manual database setup required**  

## Writing New E2E Tests

Follow the Given-When-Then pattern:

```csharp
[Fact]
public async Task Example_Test_Scenario()
{
    // Given - Setup preconditions
    var baseUrl = _serverUrl;
    
    // When - Perform actions
    await _page!.GotoAsync(baseUrl!);
    await _page.ClickAsync("button[type='submit']");
    
    // Then - Assert expected outcomes
    var result = await _page.Locator(".result").TextContentAsync();
    result.Should().Be("Expected Value");
}
```

## Troubleshooting

### Docker Not Running

```text
Error: Cannot connect to Docker daemon
```

**Solution**: Start Docker Desktop and wait for it to fully initialize.

### Port Conflicts

```text
Error: Address already in use
```

**Solution**: Testcontainers automatically assigns random ports. If issues persist, restart Docker.

### Container Pull Issuestext me

```text
Error: Failed to pull image
```

**Solution**: Ensure you have internet connectivity and Docker can pull from mcr.microsoft.com.

## Documentation

- [Playwright for .NET](https://playwright.dev/dotnet/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)