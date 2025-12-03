# Notes Feature - Vertical Slice Architecture

This folder contains the Notes feature implemented using Vertical Slice Architecture with CQRS pattern.

## Structure

Each slice is self-contained and includes:
- **Command/Query**: The request object
- **Handler**: The business logic to process the request
- **Response**: The data returned to the caller

## Slices

### CreateNote
Creates a new note for a user.
- **Command**: `CreateNoteCommand`
- **Handler**: `CreateNoteHandler`
- **Response**: `CreateNoteResponse`

### UpdateNote
Updates an existing note (user must own the note).
- **Command**: `UpdateNoteCommand`
- **Handler**: `UpdateNoteHandler`
- **Response**: `UpdateNoteResponse`

### DeleteNote
Deletes a note (user must own the note).
- **Command**: `DeleteNoteCommand`
- **Handler**: `DeleteNoteHandler`
- **Response**: `DeleteNoteResponse`

### GetNoteDetails
Retrieves details of a specific note.
- **Query**: `GetNoteDetailsQuery`
- **Handler**: `GetNoteDetailsHandler`
- **Response**: `NoteDetailsResponse`

### ListNotes
Lists all notes for a user with pagination and search.
- **Query**: `ListNotesQuery`
- **Handler**: `ListNotesHandler`
- **Response**: `ListNotesResponse`

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
    PageSize = 20,
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
2. **Easy to understand**: All related code is in one place
3. **Easy to modify**: Changes are localized to a single slice
4. **Easy to test**: Each slice can be tested independently
5. **Minimal coupling**: Slices don't depend on each other
