# GitHub Copilot Instructions for AINotesApp

## Project Overview

AINotesApp is a personal notes application built with the following technology stack and architectural patterns.

## Technology Stack

### Core Technologies

- **.NET 10.0** - Latest .NET framework
- **C# 14.0** - Latest C# language features
- **Blazor Web App** - Interactive server-side rendering
- **ASP.NET Core Identity** - Authentication and authorization
- **Entity Framework Core 10.0** - ORM for database access
- **SQL Server Express** - Local database

### NuGet Packages

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.0)
- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` (10.0.0)

## Architecture Patterns

### Vertical Slice Architecture

This project follows **Vertical Slice Architecture** where features are organized by business capability rather than technical concerns.

#### Folder Structure

Each feature operation is organized in its own folder with a **single consolidated file** containing the Command/Query, Response, and Handler:

```
Features/
└── [FeatureName]/
    ├── [Operation1]/
    │   └── [Operation1].cs          (Command/Query, Response, Handler)
    ├── [Operation2]/
    │   └── [Operation2].cs          (Command/Query, Response, Handler)
    └── README.md
```

#### Example: Notes Feature

```
Features/
└── Notes/
    ├── CreateNote/
    │   └── CreateNote.cs            (Command, Response, Handler)
    ├── UpdateNote/
    │   └── UpdateNote.cs            (Command, Response, Handler)
    ├── DeleteNote/
    │   └── DeleteNote.cs            (Command, Response, Handler)
    ├── GetNoteDetails/
    │   └── GetNoteDetails.cs        (Query, Response, Handler)
    ├── ListNotes/
    │   └── ListNotes.cs             (Query, Response, Handler)
    └── README.md
```

### CQRS (Command Query Responsibility Segregation)

Separate read and write operations:

- **Commands**: Modify state (Create, Update, Delete)

  - Use `Command` suffix
  - Return response DTOs
  - May return `null` on failure

- **Queries**: Read state (Get, List)
  - Use `Query` suffix
  - Return response DTOs
  - Use `AsNoTracking()` for performance

## Coding Standards

### 1. Use XML Documentation Comments

**Always** add XML documentation comments to:

- Public classes
- Public methods
- Public properties
- Commands and Queries
- Handlers
- Response DTOs

#### Example:

```csharp
/// <summary>
/// Command to create a new note
/// </summary>
public record CreateNoteCommand
{
    /// <summary>
    /// Title of the note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Content of the note
    /// </summary>
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// Handler for creating a new note
/// </summary>
public class CreateNoteHandler
{
    /// <summary>
    /// Handles the create note command
    /// </summary>
    /// <param name="command">The command containing note details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the created note details</returns>
    public async Task<CreateNoteResponse> HandleAsync(
        CreateNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

### 2. Use Records for DTOs

Use C# records for:

- Commands
- Queries
- Response DTOs

```csharp
public record CreateNoteCommand
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
```

### 3. Dependency Injection

- **Constructor Injection**: Use primary constructors where appropriate
- **Handler Registration**: Register handlers as `Scoped` services
- **DbContext**: Inject `ApplicationDbContext` into handlers

#### Handler Pattern:

```csharp
public class SomeHandler
{
    private readonly ApplicationDbContext _context;

    public SomeHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SomeResponse> HandleAsync(
        SomeCommand command,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

#### Registration in Program.cs:

```csharp
// Register handlers
builder.Services.AddScoped<CreateNoteHandler>();
builder.Services.AddScoped<UpdateNoteHandler>();
```

### 4. Entity Framework Core Best Practices

#### Read Operations (Queries):

- Use `AsNoTracking()` for read-only queries
- Project to DTOs using `Select()`
- Use `FirstOrDefaultAsync()` for single results
- Use `ToListAsync()` for collections

```csharp
var note = await _context.Notes
    .AsNoTracking()
    .FirstOrDefaultAsync(n => n.Id == query.Id, cancellationToken);
```

#### Write Operations (Commands):

- Track entities for updates
- Use `SaveChangesAsync()` with cancellation token
- Set timestamps explicitly

```csharp
var note = new Note
{
    Id = Guid.NewGuid(),
    Title = command.Title,
    CreatedAt = DateTimeOffset.UtcNow,
    UpdatedAt = DateTimeOffset.UtcNow
};

_context.Notes.Add(note);
await _context.SaveChangesAsync(cancellationToken);
```

### 5. Async/Await Patterns

- **Always** use async methods with `Async` suffix
- Accept `CancellationToken` as last parameter with default value
- Use `ConfigureAwait(false)` in library code (not needed in ASP.NET Core)
- Return `Task<T>` or `ValueTask<T>`

```csharp
public async Task<Response> HandleAsync(
    Command command,
    CancellationToken cancellationToken = default)
{
    // Async implementation
}
```

### 6. Security Patterns

#### User Ownership Checks:

Always verify that the current user owns the resource they're accessing:

```csharp
var note = await _context.Notes
    .FirstOrDefaultAsync(
        n => n.Id == command.Id && n.UserId == command.UserId,
        cancellationToken);

if (note == null)
{
    return null; // or throw UnauthorizedAccessException
}
```

### 7. Null Safety

- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `string.Empty` for default string values
- Use `null!` suppression operator only when necessary
- Use nullable types (`?`) for optional properties

```csharp
public string Title { get; set; } = string.Empty; // Required
public string? Summary { get; set; }              // Optional
public ApplicationUser User { get; set; } = null!; // Navigation property
```

### 8. Naming Conventions

#### Commands:

- Use verb + noun: `CreateNote`, `UpdateNote`, `DeleteNote`
- Suffix: `Command`
- Example: `CreateNoteCommand`

#### Queries:

- Use verb + noun: `GetNoteDetails`, `ListNotes`
- Suffix: `Query`
- Example: `ListNotesQuery`

#### Handlers:

- Match command/query name + `Handler`
- Example: `CreateNoteHandler`, `ListNotesHandler`

#### Responses:

- Match operation + `Response`
- Example: `CreateNoteResponse`, `ListNotesResponse`

### 9. Error Handling

#### Return null for not found:

```csharp
public async Task<NoteDetailsResponse?> HandleAsync(
    GetNoteDetailsQuery query,
    CancellationToken cancellationToken = default)
{
    var note = await _context.Notes
        .FirstOrDefaultAsync(n => n.Id == query.Id, cancellationToken);

    if (note == null)
    {
        return null;
    }

    // Return mapped response
}
```

#### Return success/failure in response:

```csharp
public record DeleteNoteResponse
{
    public bool Success { get; init; }
    public Guid? DeletedId { get; init; }
}
```

### 10. Database Indexes

When configuring entities in `OnModelCreating`:

- Add indexes on foreign keys
- Add indexes on commonly queried fields
- Add indexes on date fields for sorting

```csharp
entity.HasIndex(n => n.UserId);
entity.HasIndex(n => n.CreatedAt);
```

## Feature Implementation Checklist

When implementing a new feature, follow these steps:

### 1. Create Feature Folder

```
Features/[FeatureName]/
```

### 2. Create Operation File

For each operation (Create, Update, Delete, Get, List), create a **single consolidated file** containing:

1. **Command/Query (record)**

   - Use `record` type
   - Add XML documentation to the record and all properties
   - Include required properties

2. **Response DTO (record)**

   - Use `record` type
   - Add XML documentation to the record and all properties
   - Include only necessary data

3. **Handler (class)**
   - Inject dependencies via constructor
   - Add XML documentation to class, constructor, and methods
   - Implement `HandleAsync` method
   - Use appropriate EF Core patterns
   - Add security checks

**File Organization**: All three components in one file named `[OperationName].cs`

### 3. Register Handlers

Add to `Program.cs`:

```csharp
builder.Services.AddScoped<YourHandler>();
```

### 4. Create README

Document the feature in `Features/[FeatureName]/README.md`

## Blazor Component Integration

### Injecting Handlers into Components:

```razor
@page "/notes/create"
@inject CreateNoteHandler CreateNoteHandler
@inject NavigationManager Navigation

<h3>Create Note</h3>

@code {
    private string title = string.Empty;
    private string content = string.Empty;

    private async Task CreateNoteAsync()
    {
        var command = new CreateNoteCommand
        {
            Title = title,
            Content = content,
            UserId = currentUserId
        };

        var response = await CreateNoteHandler.HandleAsync(command);

        if (response != null)
        {
            Navigation.NavigateTo($"/notes/{response.Id}");
        }
    }
}
```

## Database Migration Workflow

### Creating Migrations:

```bash
dotnet ef migrations add [MigrationName] --project AINotesApp
```

### Updating Database:

```bash
dotnet ef database update --project AINotesApp
```

### Listing Migrations:

```bash
dotnet ef migrations list --project AINotesApp
```

## Connection String Configuration

Use `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AINotesAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Note**: Always include `TrustServerCertificate=True` for SQL Server Express

## Code Quality Standards

### General Principles:

1. **Single Responsibility**: Each handler does one thing
2. **DRY**: Don't repeat yourself
3. **KISS**: Keep it simple
4. **YAGNI**: You aren't gonna need it
5. **Explicit over Implicit**: Be clear in intent

### Performance:

- Use `AsNoTracking()` for read-only queries
- Use pagination for lists
- Project to DTOs early using `Select()`
- Use indexes appropriately

### Security:

- Always validate user ownership
- Use parameterized queries (EF Core does this)
- Validate input data
- Use HTTPS

### Testing:

- Test handlers independently
- Mock `ApplicationDbContext` using InMemory provider
- Test security constraints
- Test edge cases (null, empty, invalid data)

## Example: Complete Feature Implementation

### Single Consolidated File: `CreateNote/CreateNote.cs`

```csharp
using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.CreateNote;

/// <summary>
/// Command to create a new note
/// </summary>
public record CreateNoteCommand
{
    /// <summary>
    /// Title of the note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Content of the note
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// ID of the user creating the note
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Response after creating a note
/// </summary>
public record CreateNoteResponse
{
    /// <summary>
    /// ID of the created note
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Title of the created note
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// When the note was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Handler for creating a new note
/// </summary>
public class CreateNoteHandler
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNoteHandler"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public CreateNoteHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the create note command
    /// </summary>
    /// <param name="command">The command containing note details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the created note details</returns>
    public async Task<CreateNoteResponse> HandleAsync(
        CreateNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Content = command.Content,
            UserId = command.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateNoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            CreatedAt = note.CreatedAt
        };
    }
}
```

## Summary

When implementing new features or modifying existing code:

? Follow Vertical Slice Architecture  
? Use CQRS pattern (Commands vs Queries)  
? Add comprehensive XML documentation  
? Use Dependency Injection  
? Follow async/await best practices  
? Implement proper security checks  
? Use EF Core best practices  
? Register handlers in Program.cs  
? Use records for DTOs  
? Handle null cases appropriately  
? Add database indexes for performance  
? Write clean, maintainable code

**Remember**: Each vertical slice should be self-contained with all related code (Command/Query, Response, Handler) in a single file, implementing a complete feature end-to-end.

### Key Consolidation Benefits

✅ **Single File per Feature**: Command/Query, Response, and Handler together  
✅ **Better Cohesion**: Related code stays together  
✅ **Easier Navigation**: One file to find everything  
✅ **Reduced Context Switching**: Complete feature visible at once  
✅ **Simpler File Structure**: Fewer files to manage
