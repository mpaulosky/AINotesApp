# AINotesApp Test Suite

This directory contains comprehensive test coverage for the AINotesApp project, organized into different test types following industry best practices.

## Test Projects

### 1. AINotesApp.Tests.Unit (35 tests) ✅
Unit tests for individual components and handlers.

**Coverage:**
- Data models (Note) - 7 tests
- Feature handlers:
  - CreateNote - 5 tests
  - UpdateNote - 5 tests
  - ListNotes - 7 tests
- Services (OpenAiService) - 8 tests
- Additional handler tests - 3 tests

**Technologies:**
- XUnit v2.9.3
- FluentAssertions v7.1.0
- NSubstitute v5.3.0 (for mocking)
- Entity Framework Core InMemory

**Run tests:**
```bash
dotnet test tests/AINotesApp.Tests.Unit
```

### 2. AINotesApp.Tests.Integration (8 tests) ✅
Integration tests for database operations and data persistence.

**Coverage:**
- Note CRUD operations
- User isolation (multi-user scenarios)
- Embedding storage and retrieval
- AI content (summary, tags) persistence
- Ordering and pagination

**Technologies:**
- XUnit v2.9.3
- FluentAssertions v7.1.0
- Entity Framework Core InMemory
- Microsoft.AspNetCore.Mvc.Testing

**Run tests:**
```bash
dotnet test tests/AINotesApp.Tests.Integration
```

### 3. AINotesApp.Tests.Architecture (10 tests) ✅
Architecture tests to enforce coding standards and design patterns.

**Coverage:**
- Vertical Slice Architecture validation
- CQRS pattern enforcement (Commands/Queries)
- Dependency rules
- Naming conventions
- MediatR implementation
- Service layering

**Technologies:**
- XUnit v2.9.3
- FluentAssertions v7.1.0
- NetArchTest.Rules v1.3.2

**Run tests:**
```bash
dotnet test tests/AINotesApp.Tests.Architecture
```

### 4. AINotesApp.Tests.E2E (3 tests - Skipped by default)
End-to-end tests using Playwright for browser automation.

**Status:** Tests are skipped by default as they require the application to be running.

**Technologies:**
- XUnit v2.9.3
- FluentAssertions v7.1.0
- Microsoft.Playwright v1.50.0

**Setup Playwright:**
```bash
cd tests/AINotesApp.Tests.E2E
pwsh bin/Debug/net10.0/playwright.ps1 install
```

**Run tests:**
1. Start the application:
   ```bash
   cd AINotesApp
   dotnet run
   ```
2. In a separate terminal:
   ```bash
   dotnet test tests/AINotesApp.Tests.E2E
   ```

See [E2E README](AINotesApp.Tests.E2E/README.md) for detailed instructions.

## Running All Tests

Run all tests in the solution:
```bash
dotnet test
```

Run with detailed output:
```bash
dotnet test --verbosity normal
```

Run specific test project:
```bash
dotnet test tests/AINotesApp.Tests.Unit
```

Run tests by category (if using traits):
```bash
dotnet test --filter Category=Unit
```

## Test Summary

| Test Type | Count | Status | Description |
|-----------|-------|--------|-------------|
| Unit | 35 | ✅ Passing | Fast, isolated tests for components |
| Integration | 8 | ✅ Passing | Database and handler integration |
| Architecture | 10 | ✅ Passing | Enforce design patterns and rules |
| E2E | 3 | ⏭️ Skipped | Browser automation (optional) |
| **Total** | **56** | **53 Passing** | **Comprehensive coverage** |

## Test Structure

All tests follow the **Given-When-Then** (Arrange-Act-Assert) pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Given - Setup and preconditions
    var handler = new Handler(dependencies);
    var command = new Command { ... };

    // When - Action being tested
    var result = await handler.Handle(command);

    // Then - Expected outcomes using FluentAssertions
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedValue);
}
```

## Best Practices

1. **Naming Convention:** `MethodName_Scenario_ExpectedBehavior`
2. **Assertions:** Use FluentAssertions for readable assertions
3. **Mocking:** Use NSubstitute for dependency mocking
4. **Isolation:** Each test should be independent
5. **AAA Pattern:** Arrange-Act-Assert (Given-When-Then)
6. **Data-Driven:** Use `[Theory]` and `[InlineData]` for parameterized tests

## XUnit v3 Features (When Available)

The tests are written to be compatible with XUnit v2 and ready for v3:
- Use `[Fact]` for simple tests
- Use `[Theory]` for data-driven tests
- Use `TestOutput` for test logging
- Leverage async/await support

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- No external dependencies required
- InMemory database for fast execution
- E2E tests are skipped by default
- All tests are deterministic and repeatable

## Code Coverage

To generate code coverage reports:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

When adding new features:
1. Add unit tests for new handlers
2. Add integration tests for database operations
3. Update architecture tests if introducing new patterns
4. Consider E2E tests for critical user workflows

## Dependencies

- **.NET 10.0** - Target framework
- **XUnit 2.9.3** - Test framework
- **FluentAssertions 7.1.0** - Assertion library
- **NSubstitute 5.3.0** - Mocking framework
- **NetArchTest.Rules 1.3.2** - Architecture tests
- **Microsoft.Playwright 1.50.0** - E2E browser automation
- **EF Core InMemory 10.0.0** - In-memory database for tests
