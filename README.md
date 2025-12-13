| **Unit Tests** | 35 | Fast, isolated tests for handlers and services |
| **Integration Tests** | 8 | Database operations and data persistence |
| **Architecture Tests** | 10 | Enforce design patterns and coding standards |
│ ├── AINotesApp.Tests.Unit/
│ ├── AINotesApp.Tests.Integration/
│ └── AINotesApp.Tests.Architecture/

<div align="center">

<img src="./AINotesApp/wwwroot/favicon.png" alt="AINotesApp" height="64" />

# AINotesApp

A modern, intelligent note-taking application powered by AI

[![Build Status](https://img.shields.io/github/actions/workflow/status/mpaulosky/AINotesApp/dotnet.yml?style=flat-square&label=Build)](https://github.com/mpaulosky/AINotesApp/actions)
[![codecov](https://img.shields.io/codecov/c/github/mpaulosky/AINotesApp?style=flat-square&logo=codecov)](https://codecov.io/gh/mpaulosky/AINotesApp)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/mpaulosky/AINotesApp?style=flat-square)](LICENSE.txt)

[![Open Issues](https://img.shields.io/github/issues/mpaulosky/AINotesApp?style=flat-square&label=Open%20Issues)](https://github.com/mpaulosky/AINotesApp/issues)
[![Closed Issues](https://img.shields.io/github/issues-closed/mpaulosky/AINotesApp?style=flat-square&label=Closed%20Issues)](https://github.com/mpaulosky/AINotesApp/issues?q=is%3Aissue+is%3Aclosed)
[![Open PRs](https://img.shields.io/github/issues-pr/mpaulosky/AINotesApp?style=flat-square&label=Open%20PRs)](https://github.com/mpaulosky/AINotesApp/pulls)
[![Closed PRs](https://img.shields.io/github/issues-pr-closed/mpaulosky/AINotesApp?style=flat-square&label=Closed%20PRs&color=purple)](https://github.com/mpaulosky/AINotesApp/pulls?q=is%3Apr+is%3Aclosed)

[Overview](#overview) • [Features](#features) • [Architecture](#architecture) • [Getting Started](#getting-started) • [Testing](#testing)

</div>

## Overview

AINotesApp is a personal note-taking application that demonstrates modern software architecture and AI integration using the latest .NET technologies. Built with Blazor Server for interactive real-time UI and integrated with OpenAI for intelligent features like automatic summaries, tagging, and semantic search.

The application showcases enterprise-level patterns including Vertical Slice Architecture, CQRS, and MediatR, making it an excellent reference for building scalable .NET applications with AI capabilities.

## Features

- **AI-Powered Intelligence**: Automatic note summarization, smart tagging, and semantic search using OpenAI
- **Rich Text Editing**: Blazor-based text editor for creating and formatting notes
- **Secure Authentication**: Built-in user authentication and authorization with ASP.NET Core Identity
- **Semantic Search**: Find related notes using AI embeddings and vector similarity
- **User Isolation**: Complete data separation between users for privacy
- **Modern UI**: Responsive Blazor Server interface with real-time updates
- **Clean Architecture**: Vertical Slice Architecture with CQRS pattern for maintainability

## Architecture

### Technology Stack

- **.NET 10.0** - Latest .NET framework with C# 14.0
- **Blazor Server** - Interactive server-side rendering
- **ASP.NET Core Identity** - Authentication and authorization
- **Entity Framework Core 10.0** - ORM with SQL Server
- **MediatR** - Command/Query mediator pattern
- **OpenAI API** - AI text generation and embeddings
- **SQL Server Express** - Local database

### Design Patterns

#### Vertical Slice Architecture

Features are organized by business capability rather than technical layers:

```
Features/
└── Notes/
    ├── CreateNote/
    │   └── CreateNote.cs
    ├── UpdateNote/
    │   └── UpdateNote.cs
    ├── DeleteNote/
    │   └── DeleteNote.cs
    ├── GetNoteDetails/
    │   └── GetNoteDetails.cs
    ├── ListNotes/
    │   └── ListNotes.cs
    └── SearchNotes/
        └── SearchNotes.cs
```

Each operation contains its Command/Query, Response DTO, and Handler in a single consolidated file.

#### CQRS (Command Query Responsibility Segregation)

- **Commands**: Modify state (Create, Update, Delete)
- **Queries**: Read state (Get, List, Search)
- Handlers implement business logic with proper separation of concerns

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Git](https://git-scm.com/downloads)
- OpenAI API key (for AI features)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/mpaulosky/AINotesApp.git
   cd AINotesApp
   ```

2. Configure the database connection in `AINotesApp/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AINotesAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

3. Configure OpenAI (using User Secrets):

   ```bash
   cd AINotesApp
   dotnet user-secrets init
   dotnet user-secrets set "AiService:ApiKey" "your-openai-api-key"
   dotnet user-secrets set "AiService:ModelName" "gpt-4o"
   ```

4. Apply database migrations:

   ```bash
   dotnet ef database update --project AINotesApp
   ```

5. Run the application:

   ```bash
   cd AINotesApp
   dotnet run
   ```

6. Open your browser and navigate to `https://localhost:5001`

### Configuration

#### Database

The application uses SQL Server Express by default. Update the connection string in `appsettings.json` if using a different SQL Server instance.

#### AI Service

Configure OpenAI settings via User Secrets (recommended) or `appsettings.json`:

```json
{
  "AiService": {
    "ApiKey": "your-api-key",
    "ModelName": "gpt-4o",
    "EmbeddingModelName": "text-embedding-3-small"
  }
}
```

> [!WARNING]
> Never commit API keys to source control. Always use User Secrets or environment variables for sensitive configuration.

## Testing

The project includes comprehensive test coverage with **208 passing tests**:

### Test Projects

| Test Type              | Count | Description                                                  |
| ---------------------- | ----- | ------------------------------------------------------------ |
| **Component Tests**    | 155   | Blazor component rendering and interaction tests using BUnit |
| **Unit Tests**         | 35    | Fast, isolated tests for handlers and services               |
| **Integration Tests**  | 8     | Database operations and data persistence                     |
| **Architecture Tests** | 10    | Enforce design patterns and coding standards                 |

├── tests/ # Test projects
│ ├── AINotesApp.Tests.Unit/ # Unit + Component tests
│ ├── AINotesApp.Tests.Integration/ # Integration tests
│ └── AINotesApp.Tests.Architecture/ # Architecture tests

# Run all tests

dotnet test

# Run specific test project

dotnet test tests/AINotesApp.Tests.Unit
dotnet test tests/AINotesApp.Tests.Integration
dotnet test tests/AINotesApp.Tests.Architecture

# Run only component tests

dotnet test tests/AINotesApp.Tests.Unit --filter "FullyQualifiedName~Components"

# Run with code coverage

dotnet test --collect:"XPlat Code Coverage"

```

For detailed test documentation, see [tests/README.md](tests/README.md).

## Project Structure

```

AINotesApp/
├── AINotesApp/ # Main application
│ ├── Components/ # Blazor components
│ │ ├── Account/ # Authentication components
│ │ ├── Layout/ # Layout components
│ │ └── Pages/ # Page components
│ ├── Data/ # Entity models and DbContext
│ ├── Features/ # Vertical slices (CQRS)
│ │ └── Notes/ # Note feature operations
│ └── Services/ # Application services
│ └── Ai/ # AI integration
├── tests/ # Test projects
│ ├── AINotesApp.Tests.Unit/
│ ├── AINotesApp.Tests.Integration/
│ ├── AINotesApp.Tests.Architecture/
│ └── AINotesApp.Tests.E2E/
└── docs/ # Documentation

````

## Key Features Implementation

### AI-Powered Note Summarization

When creating or updating notes, the application automatically generates:
- Concise summaries using OpenAI
- Relevant tags for organization
- Vector embeddings for semantic search

### Semantic Search

Find related notes based on meaning rather than just keywords:
```csharp
// Search for notes semantically related to a query
var relatedNotes = await mediator.Send(new GetRelatedNotesQuery
{
    NoteId = noteId,
    UserId = currentUserId
});
````

### User Isolation

All notes are strictly isolated by user with security checks at the database level:

```csharp
var note = await _context.Notes
    .FirstOrDefaultAsync(
        n => n.Id == command.Id && n.UserId == command.UserId,
        cancellationToken);
```

## Development Guidelines

This project follows strict coding standards and architectural patterns. For detailed development guidelines, see [.github/copilot-instructions.md](.github/copilot-instructions.md).

### Key Principles

- Follow Vertical Slice Architecture
- Use CQRS pattern for all features
- Add comprehensive XML documentation
- Implement proper security checks
- Use async/await patterns consistently
- Write tests for all new features

## Resources

- [.NET 10.0 Documentation](https://learn.microsoft.com/dotnet/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [OpenAI API Documentation](https://platform.openai.com/docs)
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)

## Troubleshooting

### Database Connection Issues

If you encounter connection errors:

1. Verify SQL Server Express is installed and running
2. Check the connection string in `appsettings.json`
3. Ensure `TrustServerCertificate=True` is included in the connection string

### Migration Issues

If migrations fail:

```bash
# List existing migrations
dotnet ef migrations list --project AINotesApp

# Remove last migration if needed
dotnet ef migrations remove --project AINotesApp

# Re-apply migrations
dotnet ef database update --project AINotesApp
```

### AI Features Not Working

1. Verify your OpenAI API key is configured correctly
2. Check User Secrets: `dotnet user-secrets list --project AINotesApp`
3. Ensure you have sufficient OpenAI API credits
4. Check application logs for API errors
