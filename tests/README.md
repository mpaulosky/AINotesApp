# AINotesApp Test Suite

This directory contains comprehensive test coverage for the AINotesApp project, organized into different test types
following industry best practices.

## Test Projects

### 1. AINotesApp.Tests.Unit (179 tests) ✅

Unit tests for handlers, services, and Blazor component tests.

**Coverage:**

**Component Tests (144 tests)** - Blazor UI components using BUnit:

- Layout components:
    - MainLayout - 13 tests
    - NavMenu - 14 tests
    - ReconnectModal - 20 tests
- Page components:
    - Auth - 10 tests
    - Home - 5 tests
    - NotFound - 10 tests
- Notes feature components:
    - NoteDetails - 13 tests
    - NoteEditor - 19 tests
    - NotesList - 19 tests
    - RelatedNotes - 21 tests

**Unit Tests (35 tests)** - Handlers and services:

- Data models (Note) - 7 tests
- Feature handlers:
    - CreateNote - 5 tests
    - UpdateNote - 5 tests
    - ListNotes - 7 tests
- Services (OpenAiService) - 8 tests
- Additional handler tests - 3 tests

**Technologies:**

- XUnit v2.9.3
- FluentAssertions v8.8.0
- NSubstitute v5.3.0 (for mocking)
- BUnit v2.x (for Blazor component testing)
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
dotnet test tests/AINotesApp.Tests.Integration
dotnet test tests/AINotesApp.Tests.Architecture
```

Run only component tests (BUnit):

```bash
dotnet test tests/AINotesApp.Tests.Unit --filter "FullyQualifiedName~Components"
```

Run specific component test file:

```bash
dotnet test tests/AINotesApp.Tests.Unit --filter "FullyQualifiedName~NotesListTests"
```

## Test Summary

| Test Type    | Count   | Status          | Description                                |
|--------------|---------|-----------------|--------------------------------------------|
| Component    | 144     | ✅ Passing       | Blazor component rendering and interaction |
| Unit         | 35      | ✅ Passing       | Fast, isolated tests for handlers/services |
| Integration  | 8       | ✅ Passing       | Database and handler integration           |
| Architecture | 10      | ✅ Passing       | Enforce design patterns and rules          |
| **Total**    | **197** | **197 Passing** | **Comprehensive coverage**                 |

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

<!-- E2E tests were previously skipped by default; E2E tests are no longer present in this project. -->

- All tests are deterministic and repeatable

## Code Coverage

To generate code coverage reports:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

When adding new features:

1. **Add BUnit component tests** for new Blazor components
2. Add unit tests for new handlers and services
3. Add integration tests for database operations
4. Update architecture tests if introducing new patterns

### Component Testing with BUnit

All Blazor components should have corresponding BUnit tests:

- Test component rendering with various parameters
- Test user interactions (clicks, form inputs)
- Test authentication scenarios
- Mock dependencies (IMediator, NavigationManager, etc.)
- Use `WaitForAssertion()` for async rendering
- Follow the pattern in existing component tests

## Dependencies

- **.NET 10.0** - Target framework
- **XUnit 2.9.3** - Test framework with VSTest Adapter v3.1.5
- **FluentAssertions 8.8.0** - Assertion library
- **NSubstitute 5.3.0** - Mocking framework
- **BUnit 2.x** - Blazor component testing framework
- **NetArchTest.Rules 1.3.2** - Architecture tests
- **EF Core InMemory 10.0.0** - In-memory database for tests
- **Microsoft.AspNetCore.Components.Authorization** - Auth testing support