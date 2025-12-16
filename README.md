# AINotesApp

## Testing

To run all tests:

```bash
dotnet test
```

To run a specific test project:

```bash
dotnet test tests/AINotesApp.Tests.Unit
```

To run only component tests:

```bash
dotnet test tests/AINotesApp.Tests.Unit --filter "FullyQualifiedName~Components"
```

To run with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Getting Started

1. Configure the database connection in `AINotesApp/appsettings.json`.
2. Configure OpenAI (using User Secrets).
3. Apply database migrations.
4. Run the application.
5. Open your browser and navigate to `https://localhost:5001`.

## User Isolation

User strictly isolates all notes with security checks at the database level:

| Category           | Count | Description                                    |
|--------------------|------:|------------------------------------------------|
| Unit Tests         |    35 | Fast, isolated tests for handlers and services |
| Integration Tests  |     8 | Database operations and data persistence       |
| Architecture Tests |    10 | Enforce design patterns and coding standards   |

```text
tests/
├── AINotesApp.Tests.Unit/
├── AINotesApp.Tests.Integration/
└── AINotesApp.Tests.Architecture/
```

A modern, intelligent note-taking application powered by AI

[![Build and Test](https://github.com/mpaulosky/AINotesApp/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mpaulosky/AINotesApp/actions/workflows/build-and-test.yml)
[![codecov](https://img.shields.io/codecov/c/github/mpaulosky/AINotesApp?style=flat-square&logo=codecov)](https://codecov.io/gh/mpaulosky/AINotesApp)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/mpaulosky/AINotesApp?style=flat-square)](LICENSE.txt)

[![Open Issues](https://img.shields.io/github/issues/mpaulosky/AINotesApp?style=flat-square&label=Open%20Issues)](https://github.com/mpaulosky/AINotesApp/issues)
[![Closed Issues](https://img.shields.io/github/issues-closed/mpaulosky/AINotesApp?style=flat-square&label=Closed%20Issues)](https://github.com/mpaulosky/AINotesApp/issues?q=is%3Aissue+is%3Aclosed)
[![Open PRs](https://img.shields.io/github/issues-pr/mpaulosky/AINotesApp?style=flat-square&label=Open%20PRs)](https://github.com/mpaulosky/AINotesApp/pulls)
[![Closed PRs](https://img.shields.io/github/issues-pr-closed/mpaulosky/AINotesApp?style=flat-square&label=Closed%20PRs&color=purple)](https://github.com/mpaulosky/AINotesApp/pulls?q=is%3Apr+is%3Aclosed)

[Overview](#overview) • [Features](#features) • [Architecture](#architecture) • [Getting Started](#getting-started) • [Testing](#testing)

## Overview

AINotesApp is a personal note-taking application that demonstrates modern software architecture and AI integration using
the latest .NET technologies. Built with Blazor Server for interactive real-time UI and integrated with OpenAI for
intelligent features like automatic summaries, tagging, and semantic search.

The application showcases enterprise-level patterns including Vertical Slice Architecture, CQRS, and MediatR, making it
an excellent reference for building scalable .NET applications with AI capabilities.

## Features

- **AI-Powered Intelligence**: Automatic note summarization, smart tagging, and semantic search using OpenAI
- **Rich Text Editing**: Blazor-based text editor for creating and formatting notes
- **Secure Authentication**: Enterprise-grade OAuth 2.0 and OpenID Connect authentication via Auth0
- **Semantic Search**: Find related notes using AI embeddings and vector similarity
- **User Isolation**: Complete data separation between users for privacy
- **Modern UI**: Responsive Blazor Server interface with real-time updates
- **Clean Architecture**: Vertical Slice Architecture with CQRS pattern for maintainability

## Architecture

### Technology Stack

- **.NET 10.0** – Latest .NET Framework with C# 14.0
- **Blazor Server** - Interactive server-side rendering
- **Auth0 Authentication** – OAuth 2.0 and OpenID Connect authentication
- **Entity Framework Core 10.0** – ORM with SQL Server
- **MediatR** - Command/Query mediator pattern
- **OpenAI API** – AI text generation and embeddings
- **SQL Server Express** - Local database

### Design Patterns

#### Vertical Slice Architecture

Features are organized by business capability rather than technical layers:

```text
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
- [Auth0 Account](https://auth0.com/signup) - Free tier available (required for authentication)
- OpenAI API key (for AI features)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/mpaulosky/AINotesApp.git
   cd AINotesApp
   ```

2. **Configure Auth0 Authentication:**

   a. Create an Auth0 tenant at [auth0.com](https://auth0.com/signup) (free tier available)

   b. Create a Regular Web Application in Auth0:
      - Navigate to Applications → Create Application
      - Choose "Regular Web Applications"
      - Note your Domain and Client ID

   c. Configure Application Settings:
      - **Allowed Callback URLs**: `https://localhost:5001/auth/callback`
      - **Allowed Logout URLs**: `https://localhost:5001/`
      - **Allowed Web Origins**: `https://localhost:5001`
      - Save Changes

   d. Create an API in Auth0:
      - Navigate to Applications → APIs → Create API
      - Name: `AINotesApp API`
      - Identifier: `https://api.ainotesapp.local` (or your preferred identifier)
      - Signing Algorithm: RS256
      - Enable RBAC and "Add Permissions in the Access Token"

   e. (Optional) Create roles:
      - Navigate to User Management → Roles → Create Role
      - Create role: `notes.admin` for administrative features
      - Assign permissions to the role if needed

3. Configure `AINotesApp/appsettings.json` with your Auth0 settings:

   ```json
   {
     "Auth0": {
       "Domain": "your-tenant.us.auth0.com",
       "ClientId": "your-client-id-from-auth0",
       "Audience": "https://api.ainotesapp.local",
       "CallbackPath": "/auth/callback",
       "LogoutPath": "/auth/logout"
     },
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AINotesAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

4. **Configure Auth0 Client Secret** (NEVER commit to source control):

   ```bash
   cd AINotesApp
   dotnet user-secrets init
   dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret-from-auth0"
   ```

5. Configure OpenAI (using User Secrets):

   ```bash
   dotnet user-secrets set "AiService:ApiKey" "your-openai-api-key"
   dotnet user-secrets set "AiService:ModelName" "gpt-4o"
   ```

6. Apply database migrations:

   ```bash
   dotnet ef database update --project AINotesApp
   ```

7. Run the application:

   ```bash
   cd AINotesApp
   dotnet run
   ```

8. Open your browser and navigate to `https://localhost:5001`

9. Click **Login** and authenticate using Auth0 Universal Login

> [!NAuth0 Authentication

The application uses Auth0 for secure authentication. Configuration is split between `appsettings.json` (non-secret settings) and User Secrets (secret settings).

**Required `appsettings.json` settings:**

```json
{
  "Auth0": {
    "Domain": "your-tenant.us.auth0.com",
    "ClientId": "your-client-id",
    "Audience": "https://api.ainotesapp.local",
    "CallbackPath": "/auth/callback",
    "LogoutPath": "/auth/logout"
  }
}
```

**Required User Secrets (NEVER in appsettings.json):**

```bash
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
```

For production deployments, use environment variables:
- Azure App Service: Set `Auth0__ClientSecret` in Application Settings
- Docker: Use environment variables with `Auth0__ClientSecret` format

#### OTE]
> On first run, you may need to create a user account in Auth0. The application will automatically create user records when you first authenticate.

### Configuration

#### Database

The application uses SQL Server Express by default. Update the connection string in `appsettings.json` if using a
different SQL Server instance.

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
|------------------------|-------|--------------------------------------------------------------|
| **Component Tests**    | 155   | Blazor component rendering and interaction tests using BUnit |
| **Unit Tests**         | 35    | Fast, isolated tests for handlers and services               |
| **Integration Tests**  | 8     | Database operations and data persistence                     |
| **Architecture Tests** | 10    | Enforce design patterns and coding standards                 |

├── tests/ # Test projects
│ ├── AINotesApp.Tests.Unit/ # Unit + Component tests
│ ├── AINotesApp.Tests.Integration/ # Integration tests
│ └── AINotesApp.Tests.Architecture/ # Architecture tests

```text

For detailed test documentation, see [tests/README.md](tests/README.md).

## Project Structure

```text

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

User strictly isolates all notes with security checks at the database level:

```csharp
var note = await _context.Notes
    .FirstOrDefaultAsync(
        n => n.Id == command.Id && n.UserId == command.UserId,
        cancellationToken);
```

## Development Guidelines

This project follows strict coding standards and architectural patterns. For detailed development guidelines,
see [.GitHub/copilot-instructions.md](.github/copilot-instructions.md).

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
- [Auth0 Documentation](https://auth0.com/docs)
- [Auth0 ASP.NET Core SDK](https://auth0.com/docs/quickstart/webapp/aspnet-core)
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

### Authentication Issues

If you cannot log in:

1. **Verify Auth0 Configuration:**
   ```bash
   # Check that all secrets are configured
   dotnet user-secrets list --project AINotesApp
   ```
   Should show: `Auth0:ClientSecret`

2. **Check Auth0 Application Settings:**
   - Allowed Callback URLs must include `https://localhost:5001/auth/callback`
   - Allowed Logout URLs must include `https://localhost:5001/`
   - Application type should be "Regular Web Application"

3. **Verify appsettings.json:**
   - `Auth0:Domain` matches your Auth0 tenant
   - `Auth0:ClientId` matches your Auth0 application
   - `Auth0:Audience` matches your Auth0 API identifier

4. **Check browser console and application logs** for specific error messages

> [!WARNING]
> Never commit `Auth0:ClientSecret` to version control. Always use User Secrets for local development and environment variables for production.