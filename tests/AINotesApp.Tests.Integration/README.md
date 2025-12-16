# Integration Tests Documentation

This directory contains integration tests for the AINotesApp that test complete workflows, database interactions, and feature integration.

## Overview

Integration tests verify that different components of the system work together correctly. Unlike unit tests that isolate individual components, integration tests use real database contexts (in-memory) and test complete feature workflows.

## Test Structure

```
AINotesApp.Tests.Integration/
├── Database/
│   └── DatabaseFixture.cs          # Shared database fixture for tests
├── Features/
│   ├── NotesDatabaseIntegrationTests.cs    # Basic database CRUD operations
│   ├── NotesCrudWorkflowTests.cs           # Complete CRUD workflows
│   └── Notes/
│       └── BackfillTagsIntegrationTests.cs # Backfill tags feature tests
└── Helpers/
    ├── MockAiServiceHelper.cs      # Shared AI service mocking utilities
    └── NoteTestDataBuilder.cs      # Fluent builder for creating test Note data
```

## Shared Test Infrastructure

### 1. DatabaseFixture

**Location**: `Database/DatabaseFixture.cs`

Provides in-memory database contexts for integration tests with proper isolation.

**Key Features**:
- **Unique Databases**: Each fixture instance creates a unique in-memory database
- **Test Isolation**: `CreateNewContext()` for complete isolation between tests
- **Shared Context**: `CreateSharedContext()` when tests need to access the same data
- **Automatic Cleanup**: Implements `IDisposable` for proper resource management

**Usage Patterns**:

```csharp
// Pattern 1: Using xUnit class fixture for shared database across tests
public class MyIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test_UsesIsolatedContext()
    {
        // Each test gets a fresh database
        await using var context = _fixture.CreateNewContext();
        
        // ... test code ...
    }
}

// Pattern 2: Using shared context for related operations
[Fact]
public async Task Test_UsesSharedContext()
{
    // First operation
    await using var context1 = _fixture.CreateSharedContext();
    context1.Notes.Add(new Note { /* ... */ });
    await context1.SaveChangesAsync();
    
    // Second operation accessing same data
    await using var context2 = _fixture.CreateSharedContext();
    var note = await context2.Notes.FirstAsync();
    // ... assertions ...
}
```

**Methods**:
- `Context` - Shared ApplicationDbContext for the fixture
- `CreateNewContext()` - Creates a completely isolated database context
- `CreateSharedContext()` - Creates a context connected to the fixture's shared database

### 2. MockAiServiceHelper

**Location**: `Helpers/MockAiServiceHelper.cs`

Provides standardized mock AI service instances for testing features that depend on AI functionality.

**Key Features**:
- **Standard Responses**: Default test data for quick test setup
- **Custom Responses**: Configure specific AI responses for targeted tests
- **Dynamic Behavior**: Create mocks that respond based on input parameters

**Usage Patterns**:

```csharp
// Pattern 1: Standard mock with default responses
[Fact]
public async Task Test_WithStandardAiService()
{
    var aiService = MockAiServiceHelper.CreateMockAiService();
    var handler = new CreateNoteHandler(context, aiService);
    
    var result = await handler.Handle(command, CancellationToken.None);
    
    result.AiSummary.Should().Be("Test summary");
    result.Tags.Should().Be("test,tag");
}

// Pattern 2: Custom responses for specific test scenarios
[Fact]
public async Task Test_WithCustomAiResponses()
{
    var aiService = MockAiServiceHelper.CreateMockAiService(
        summary: "Custom test summary",
        tags: "custom,tags,here",
        embedding: new[] { 1.0f, 2.0f, 3.0f }
    );
    
    // ... test with custom AI responses ...
}

// Pattern 3: Dynamic behavior based on input
[Fact]
public async Task Test_WithDynamicAiService()
{
    var aiService = MockAiServiceHelper.CreateDynamicMockAiService(
        summaryFunc: content => $"Summary of: {content}",
        tagsFunc: (title, content) => $"tags-{title}",
        embeddingFunc: content => new[] { content.Length * 0.1f }
    );
    
    // ... test with input-dependent AI behavior ...
}
```

**Methods**:
- `CreateMockAiService()` - Standard mock with default test responses
- `CreateMockAiService(summary, tags, embedding)` - Mock with custom responses
- `CreateDynamicMockAiService(summaryFunc, tagsFunc, embeddingFunc)` - Dynamic mock with functions

**Default Test Data**:
- Summary: "Test summary"
- Tags: "test,tag"
- Embedding: [0.1f, 0.2f, 0.3f, 0.4f, 0.5f]

### 3. NoteTestDataBuilder

**Location**: `Helpers/NoteTestDataBuilder.cs`

Provides a fluent builder API for creating test Note instances with sensible defaults and easy customization.

**Key Features**:
- **Test Data Builder Pattern**: Clean, readable test data creation
- **Sensible Defaults**: All required fields pre-populated
- **Fluent API**: Chain method calls for customization
- **Type Safety**: Compile-time checking of Note properties

**Usage Patterns**:

```csharp
// Pattern 1: Default note for simple tests
[Fact]
public async Task Test_WithDefaultNote()
{
    await using var context = _fixture.CreateNewContext();
    
    var note = NoteTestDataBuilder.CreateDefault().Build();
    
    context.Notes.Add(note);
    await context.SaveChangesAsync();
    
    // Default values:
    // - Id: new Guid
    // - Title: "Test Note"
    // - Content: "Test Content"
    // - OwnerSubject: "test-user"
    // - CreatedAt/UpdatedAt: DateTime.UtcNow
}

// Pattern 2: Customized note for specific test scenarios
[Fact]
public async Task Test_WithCustomNote()
{
    var note = NoteTestDataBuilder.CreateDefault()
        .WithTitle("Custom Title")
        .WithContent("Custom Content")
        .WithOwnerSubject("specific-user-123")
        .Build();
    
    note.Title.Should().Be("Custom Title");
    note.OwnerSubject.Should().Be("specific-user-123");
}

// Pattern 3: Note with AI-generated content
[Fact]
public async Task Test_WithAiContent()
{
    var note = NoteTestDataBuilder.CreateDefault()
        .WithTitle("AI-Enhanced Note")
        .WithContent("Note content")
        .WithAiSummary("AI-generated summary")
        .WithTags("ai, machine-learning, test")
        .WithEmbedding(new[] { 0.1f, 0.2f, 0.3f })
        .Build();
    
    note.AiSummary.Should().NotBeNull();
    note.Tags.Should().Contain("ai");
}

// Pattern 4: Multiple notes with different timestamps
[Fact]
public async Task Test_WithTimestampedNotes()
{
    var now = DateTime.UtcNow;
    
    var oldNote = NoteTestDataBuilder.CreateDefault()
        .WithTitle("Old Note")
        .WithCreatedAt(now.AddDays(-7))
        .WithUpdatedAt(now.AddDays(-7))
        .Build();
    
    var newNote = NoteTestDataBuilder.CreateDefault()
        .WithTitle("New Note")
        .WithCreatedAt(now)
        .WithUpdatedAt(now)
        .Build();
    
    // Test ordering or time-based queries
}

// Pattern 5: Creating multiple test notes efficiently
[Fact]
public async Task Test_WithMultipleNotes()
{
    await using var context = _fixture.CreateNewContext();
    
    context.Notes.AddRange(
        NoteTestDataBuilder.CreateDefault().WithTitle("Note 1").Build(),
        NoteTestDataBuilder.CreateDefault().WithTitle("Note 2").Build(),
        NoteTestDataBuilder.CreateDefault().WithTitle("Note 3").Build()
    );
    
    await context.SaveChangesAsync();
    
    var count = await context.Notes.CountAsync();
    count.Should().Be(3);
}
```

**Methods**:
- `CreateDefault()` - Static factory returning new builder with defaults
- `WithId(Guid)` - Set specific note ID
- `WithTitle(string)` - Set note title
- `WithContent(string)` - Set note content  
- `WithOwnerSubject(string)` - Set owner/user identifier
- `WithCreatedAt(DateTime)` - Set creation timestamp
- `WithUpdatedAt(DateTime)` - Set last update timestamp
- `WithEmbedding(float[]?)` - Set AI embedding vector
- `WithTags(string?)` - Set comma-separated tags
- `WithAiSummary(string?)` - Set AI-generated summary
- `Build()` - Construct the Note instance

**Default Values**:
- Id: `Guid.NewGuid()`
- Title: "Test Note"
- Content: "Test Content"  
- OwnerSubject: "test-user"
- CreatedAt: `DateTime.UtcNow`
- UpdatedAt: `DateTime.UtcNow`
- Embedding: `null`
- Tags: `null`
- AiSummary: `null`

**Benefits**:
- **Reduced Duplication**: Eliminates repetitive Note instantiation code
- **Improved Readability**: Test data creation is self-documenting
- **Easy Maintenance**: Changes to Note structure only require builder updates
- **SOLID Principles**: Follows Single Responsibility and Open/Closed principles

## Test Categories

### Database Integration Tests

**File**: `Features/NotesDatabaseIntegrationTests.cs`

Tests basic database operations (Create, Read, Update, Delete) directly against the database context.

**Scenarios Tested**:
- Creating and saving notes
- Updating existing notes
- Deleting notes
- Querying notes with filters
- Handling concurrent updates
- Database constraint validation

**Purpose**: Verify that the Note entity and ApplicationDbContext work correctly with Entity Framework Core.

### CRUD Workflow Tests

**File**: `Features/NotesCrudWorkflowTests.cs`

Tests complete end-to-end CRUD workflows using MediatR handlers.

**Scenarios Tested**:
- Complete Create → Read → Update → Delete workflow
- Sequential updates (last-write-wins)
- Creating multiple notes
- Updating non-existent notes
- User isolation (users can't access other users' notes)
- AI integration (summaries, tags, embeddings)
- Error handling and validation

**Purpose**: Verify that all MediatR handlers work together correctly in realistic scenarios.

### Feature Integration Tests

**File**: `Features/Notes/BackfillTagsIntegrationTests.cs`

Tests the backfill tags feature which adds AI-generated tags to existing notes.

**Scenarios Tested**:
- Multi-user isolation (backfill processes only target user's notes)
- Selective processing (only notes without tags)
- Error handling when AI service fails
- Batch processing

**Purpose**: Verify that the backfill feature correctly processes notes while maintaining data isolation and error handling.

## Best Practices

### Test Isolation

✅ **DO**:
- Use `CreateNewContext()` for tests that need complete isolation
- Dispose contexts properly with `await using` or `using`
- Use unique identifiers (GUIDs) for test data
- Clean up or use fresh contexts between test steps

❌ **DON'T**:
- Share contexts between unrelated tests
- Rely on test execution order
- Forget to dispose database contexts
- Use hard-coded IDs that might conflict

### Mock AI Services

✅ **DO**:
- Use `MockAiServiceHelper` for consistent mock setup
- Choose the right mock pattern for your test needs
- Configure custom responses when testing specific scenarios
- Use dynamic mocks when testing input-dependent behavior

❌ **DON'T**:
- Create duplicate mock setup code in each test
- Use real AI services in integration tests
- Forget to configure AI mock returns before testing

### Test Data Creation

✅ **DO**:
- Use `NoteTestDataBuilder` for creating test Note instances
- Leverage fluent API for readable, self-documenting test data
- Use default values for non-critical properties
- Customize only the properties relevant to your test

❌ **DON'T**:
- Manually instantiate Note objects with verbose initializers
- Hard-code test data values throughout tests
- Create duplicate Note creation patterns
- Skip using the builder when it simplifies your test

### Test Organization

✅ **DO**:
- Group related tests in the same class
- Use descriptive test method names that explain the scenario
- Follow Arrange-Act-Assert pattern
- Add XML documentation comments to test classes

❌ **DON'T**:
- Mix different feature areas in the same test class
- Create overly complex tests that test multiple things
- Skip assertions or test incomplete workflows

## Running Integration Tests

### All Integration Tests

```powershell
dotnet test tests/AINotesApp.Tests.Integration/AINotesApp.Tests.Integration.csproj
```

### With Detailed Output

```powershell
dotnet test tests/AINotesApp.Tests.Integration/AINotesApp.Tests.Integration.csproj --logger "console;verbosity=detailed"
```

### Specific Test Class

```powershell
dotnet test tests/AINotesApp.Tests.Integration/AINotesApp.Tests.Integration.csproj --filter "FullyQualifiedName~NotesCrudWorkflowTests"
```

### Coverage Report

```powershell
dotnet test tests/AINotesApp.Tests.Integration/AINotesApp.Tests.Integration.csproj --collect:"XPlat Code Coverage"
```

## Test Database

Integration tests use **Entity Framework Core In-Memory Database** which provides:
- Fast test execution
- No external dependencies
- Automatic cleanup
- Thread-safe test isolation

**Note**: In-memory database has some limitations compared to SQL Server:
- No referential integrity checks
- No stored procedures
- Limited transaction support
- Different SQL dialect

For more complete database testing, consider using **Testcontainers** with real SQL Server (package already included in project).

## Dependencies

The integration test project includes:
- **FluentAssertions** - Expressive assertion library
- **NSubstitute** - Mocking framework for IAiService
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database provider
- **Testcontainers.MsSql** - SQL Server containers (for advanced scenarios)
- **xUnit** - Test framework

## Continuous Integration

Integration tests run automatically in CI/CD pipelines:
- ✅ On pull requests to main branch
- ✅ On commits to feature branches
- ✅ Before deployment to any environment

All integration tests must pass before code can be merged.

## Troubleshooting

### Tests Fail with "Database already exists"

**Cause**: Not using unique database names or not properly disposing contexts

**Solution**: Ensure `CreateNewContext()` is called with `await using` or that each test uses a fresh context

### Tests Fail Intermittently

**Cause**: Test execution order dependency or shared state

**Solution**: Review test isolation - each test should be completely independent

### AI Service Mock Not Working

**Cause**: Mock not configured or wrong method called

**Solution**: Verify mock setup using `MockAiServiceHelper` and check that async methods are properly awaited

### Slow Test Execution

**Cause**: Not using in-memory database or improper async patterns

**Solution**: Verify `UseInMemoryDatabase()` is used and all async operations use `await`

## Adding New Integration Tests

When adding new integration tests:

1. **Choose the Right Location**:
   - Database operations → `Features/NotesDatabaseIntegrationTests.cs`
   - Handler workflows → `Features/NotesCrudWorkflowTests.cs`
   - New features → Create new file in `Features/` directory

2. **Use Shared Infrastructure**:
   - Inherit from `IClassFixture<DatabaseFixture>` if using database
   - Use `MockAiServiceHelper` for AI service mocks
   - Follow existing naming conventions

3. **Document the Test**:
   - Add XML summary explaining what the test verifies
   - Use descriptive method names
   - Include inline comments for complex logic

4. **Verify Test Isolation**:
   - Run test individually
   - Run with all other tests
   - Verify cleanup happens correctly

## Migration from Unit to Integration Tests

Some tests may fit better as integration tests:
- Tests requiring real database operations
- Tests verifying multiple components working together
- Tests checking data persistence
- Tests validating complete workflows

Consider integration tests when unit tests become too complex with many mocks.

## Summary

Integration tests provide confidence that the application works correctly as a complete system. By using shared test infrastructure (`DatabaseFixture`, `MockAiServiceHelper`, and `NoteTestDataBuilder`), we maintain consistency, reduce duplication, and make tests easier to write and maintain.

**Current Test Count**: 18 integration tests
**Test Status**: ✅ All passing
**Code Coverage**: Covers critical CRUD workflows and database operations
**Shared Helpers**: 3 helper classes (DatabaseFixture, MockAiServiceHelper, NoteTestDataBuilder)
