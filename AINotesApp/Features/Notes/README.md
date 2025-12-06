# Notes Feature - Vertical Slice Architecture

This folder contains the Notes feature implemented using Vertical Slice Architecture with CQRS pattern.

## Structure

Each feature slice is organized in its own folder with a single consolidated file containing:

- **Command/Query**: The request object (record type)
- **Response**: The data returned to the caller (record type)
- **Handler**: The business logic to process the request (class)

All three components reside in a single `.cs` file named after the feature (e.g., `CreateNote.cs`), making it easy to locate and maintain all related code together.

### Folder Structure

```text
Features/Notes/
├── CreateNote/
│   └── CreateNote.cs          (Command, Response, Handler)
├── UpdateNote/
│   └── UpdateNote.cs          (Command, Response, Handler)
├── DeleteNote/
│   └── DeleteNote.cs          (Command, Response, Handler)
├── GetNoteDetails/
│   └── GetNoteDetails.cs      (Query, Response, Handler)
├── ListNotes/
│   └── ListNotes.cs           (Query, Response, Handler)
└── README.md
```

## Slices

### CreateNote

Creates a new note for a user.

**File**: `CreateNote/CreateNote.cs`

Contains:

- **Command**: `CreateNoteCommand`
- **Response**: `CreateNoteResponse`
- **Handler**: `CreateNoteHandler`

### UpdateNote

Updates an existing note (user must own the note).

**File**: `UpdateNote/UpdateNote.cs`

Contains:

- **Command**: `UpdateNoteCommand`
- **Response**: `UpdateNoteResponse`
- **Handler**: `UpdateNoteHandler`

### DeleteNote

Deletes a note (user must own the note).

**File**: `DeleteNote/DeleteNote.cs`

Contains:

- **Command**: `DeleteNoteCommand`
- **Response**: `DeleteNoteResponse`
- **Handler**: `DeleteNoteHandler`

### GetNoteDetails

Retrieves details of a specific note.

**File**: `GetNoteDetails/GetNoteDetails.cs`

Contains:

- **Query**: `GetNoteDetailsQuery`
- **Response**: `NoteDetailsResponse`
- **Handler**: `GetNoteDetailsHandler`

### ListNotes

Lists all notes for a user with pagination and search.

**File**: `ListNotes/ListNotes.cs`

Contains:

- **Query**: `ListNotesQuery`
- **Response**: `ListNotesResponse` (includes `NoteListItem`)
- **Handler**: `ListNotesHandler`

## Usage

### Registering Handlers

Add the handlers to your dependency injection container in `Program.cs`:

```csharp
// Register Note handlers
builder.Services.AddScoped<CreateNoteHandler>();
builder.Services.AddScoped<UpdateNoteHandler>();
builder.Services.AddScoped<DeleteNoteHandler>();
builder.Services.AddScoped<GetNoteDetailsHandler>();
builder.Services.AddScoped<ListNotesHandler>();
```

### Example Usage

```csharp
// Create a note
var createHandler = serviceProvider.GetRequiredService<CreateNoteHandler>();
var createCommand = new CreateNoteCommand
{
    Title = "My Note",
    Content = "This is my note content",
    UserId = currentUserId
};
var createResponse = await createHandler.HandleAsync(createCommand);

// List notes
var listHandler = serviceProvider.GetRequiredService<ListNotesHandler>();
var listQuery = new ListNotesQuery
{
    UserId = currentUserId,
    PageNumber = 1,
    ```csharp
    SearchTerm = "search term"
};
var listResponse = await listHandler.HandleAsync(listQuery);

// Get note details
var detailsHandler = serviceProvider.GetRequiredService<GetNoteDetailsHandler>();
var detailsQuery = new GetNoteDetailsQuery
{
    Id = noteId,
    UserId = currentUserId
};
var detailsResponse = await detailsHandler.HandleAsync(detailsQuery);

// Update a note
var updateHandler = serviceProvider.GetRequiredService<UpdateNoteHandler>();
var updateCommand = new UpdateNoteCommand
{
    Id = noteId,
    Title = "Updated Title",
    Content = "Updated content",
    UserId = currentUserId
};
var updateResponse = await updateHandler.HandleAsync(updateCommand);

// Delete a note
var deleteHandler = serviceProvider.GetRequiredService<DeleteNoteHandler>();
var deleteCommand = new DeleteNoteCommand
{
    Id = noteId,
    UserId = currentUserId
};
var deleteResponse = await deleteHandler.HandleAsync(deleteCommand);
```

## Benefits of Vertical Slice Architecture

1. **Feature-focused**: Each slice represents a complete feature
2. **Cohesive**: All related code (Command/Query, Response, Handler) is in one file
3. **Easy to navigate**: Single file per feature reduces context switching
4. **Easy to understand**: Complete feature logic is visible at once
5. **Easy to use
6. **Easy to test**: Each slice can be tested independently
7. **Minimal coupling**: Slices don't depend on each other

## File Organization Pattern

Each consolidated file follows this structure:

```csharp
using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.[FeatureName];

/// <summary>
/// Command/Query documentation
/// </summary>
public record [FeatureName]Command  // or Query
{
    // Properties with XML documentation
}

/// <summary>
/// Response documentation
/// </summary>
public record [FeatureName]Response
{
    // Properties with XML documentation
}

/// <summary>
/// Handler documentation
/// </summary>
public class [FeatureName]Handler
{
    private readonly ApplicationDbContext _context;

    public [FeatureName]Handler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<[FeatureName]Response> HandleAsync(
        [FeatureName]Command command,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```
