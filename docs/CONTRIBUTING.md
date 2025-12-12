# Contributing to This Project

Thank you for taking the time to consider contributing to our project.

# Contributing to This Project

Thank you for taking the time to consider contributing to our project.

The following is a set of guidelines for contributing to the project. These are mostly guidelines, not rules, and can be
changed in the future. Please submit your suggestions with a pull-request to this document.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [What should I know before I get started](#what-should-i-know-before-i-get-started)
  - [Project Folder Structure](#project-folder-structure)
  - [Design Decisions](#design-decisions)
  - [How can I contribute](#how-can-i-contribute)
    - [Create an Issue](#create-an-issue)
    - [Respond to an Issue](#respond-to-an-issue)
    - [Write code](#write-code)
    - [Write documentation](#write-documentation)

## Welcome

    Thank you for your interest in contributing! We value all contributions and strive to make this project a welcoming,
inclusive space for everyone.

Below are guidelines to help you get started. If you have suggestions, please submit a pull request to this document.

## Code of Conduct

We have adopted a code of conduct from the Contributor Covenant. Contributors to this project are expected to adhere to
this code. Please report unwanted behavior to [Matthew Paulosky](mailto:matthew.paulosky@outlook.com)

## Quick Start

1. Fork the repository and clone your fork.
2. Ensure you have .NET 10.0 SDK and SQL Server Express installed.
3. Run database migrations: `dotnet ef database update --project AINotesApp`
4. Create a branch from `main` (use a descriptive name, e.g. `feature/123-add-search`).
5. Make your changes, following the code style and guidelines below (see `.github/copilot-instructions.md` for detailed patterns).
6. Add or update tests as needed.
7. Commit with clear messages (see below).
8. Push your branch and open a Pull Request to `main`.
9. Ensure all checks pass and respond to review feedback.

## What should I know before I get started

This project is a personal notes application built with Blazor Web App, ASP.NET Core Identity, Entity Framework Core, and SQL Server Express. It follows Vertical Slice Architecture with CQRS pattern for feature organization.

### Code Style & Commit Messages

- Use consistent formatting (C# conventions, .editorconfig if present).
- **Always add XML documentation comments** to:
  - Public classes, methods, and properties
  - Commands, Queries, Response DTOs
  - Handlers and their methods
- Use C# records for DTOs (Commands, Queries, Responses).
- Follow async/await patterns with `CancellationToken` parameters.
- Use EF Core best practices:
  - `AsNoTracking()` for read-only queries
  - `SaveChangesAsync()` with cancellation tokens
  - Write clear, descriptive commit messages:
  - Use present tense (e.g., "Add search feature")
  - Reference issues (e.g., `Fixes #123`)
- See `.github/copilot-instructions.md` for comprehensive coding standards.

### Project Folder Structure

This project is designed to be built and run primarily with Visual Studio 2022, JetBrains Rider, or Visual Studio Code.
The project structure follows **Vertical Slice Architecture** where each feature is organized in its own folder with
Commands/Queries, Response DTOs, and Handlers in a single consolidated file. The folders are configured as follows:

```bash
docs/                                   -- Documentation and guides

AINotesApp/                             -- Main application project
  Components/                           -- Blazor components
    Account/                            -- Identity/authentication components
      Pages/                            -- Account pages (login, register, etc.)
      Shared/                           -- Shared account components
    Layout/                             -- Layout components (MainLayout, NavMenu)
    Pages/                              -- Page components (Home, Weather, Notes)
  Data/                                 -- Data models and EF Core context
    ApplicationDbContext.cs             -- EF Core DbContext
    ApplicationUser.cs                  -- Identity user model
    Note.cs                             -- Note entity
    Migrations/                         -- EF Core migrations
  Features/                             -- Vertical Slice Architecture features
    Notes/                              -- Notes feature (CQRS handlers)
      CreateNote/                       -- Create note command and handler
      UpdateNote/                       -- Update note command and handler
      DeleteNote/                       -- Delete note command and handler
      GetNoteDetails/                   -- Get note query and handler
      ListNotes/                        -- List notes query and handler
  Services/                             -- Application services
    Ai/                                 -- AI-related services
  wwwroot/                              -- Static web assets (CSS, JS, etc.)
  Properties/                           -- Project properties and launch settings
  bin/                                  -- Build output
  obj/                                  -- Build objects
  appsettings.json                      -- Application configuration
  appsettings.Development.json          -- Development configuration
  Program.cs                            -- Application entry point

tests/                                  -- Tests
  AINotesApp.Tests.Integration/         -- Integration tests
  AINotesApp.Tests.Unit/                -- Unit tests

AINotesApp.slnx                         -- Solution file
LICENSE.txt                             -- License
README.md                               -- Project overview
```

See the main [README.md](../README.md) for more details.

All official versions of the project are built and delivered with GitHub Actions and linked in the main README.md
and [releases tab in GitHub](https://github.com/mpaulosky/AINotesApp/releases).

## Design Decisions

Design for this project is ultimately decided by the project team
lead, [Matthew Paulosky](mailto:matthew.paulosky@outlook.com). The following project tenets are adhered to when making
decisions:

1. Use **Blazor Web App** with Interactive Server rendering for the UI.
1. Use **SQL Server Express** for data persistence.
1. Use **Entity Framework Core 10.0** for data access with code-first migrations.
1. Follow **Vertical Slice Architecture** where features are organized by business capability.
1. Use **CQRS pattern** (Command Query Responsibility Segregation) for separating read and write operations.
1. Use **ASP.NET Core Identity** for authentication and authorization.
1. Each feature operation has a single consolidated file containing Command/Query, Response DTO, and Handler.
1. All code must include comprehensive XML documentation comments.
1. Use C# records for Commands, Queries, and Response DTOs.

If you have suggestions, please open an issue or discuss in your pull request.

### How can I contribute

We are always looking for help on this project. There are several ways that you can help:
This means one of several types of contributions:

1. [Create an Issue](#create-an-issue)
1. [Respond to an Issue](#respond-to-an-issue)
1. [Write code](#write-code)
1. [Write documentation](#write-documentation)

## Contribution Types

- **Report a Bug:** Please add the `Bug` label so we can triage and track it.
- **Suggest an Enhancement:** Add the `Enhancement` label for new features or improvements.
- **Write Code:** All code should be linked to an issue. Include or update tests for new features and bug fixes.
- **Write Documentation:** Help us improve `/docs` and keep the main [README.md](../README.md) up to date.

### Create an Issue

Create a [New Issue Here](https://github.com/mpaulosky/AINotesApp/issues).

1. If you are reporting a `Bug` that you have found. Be sure to add the `Bug` label so that we can triage and track it.
1. If you are reporting an `Enhancement` that you think would improve the project. Be sure to add the `Enhancement`
   label so we can track it.

Please provide as much detail as possible, including steps to reproduce, expected behavior, and screenshots if helpful.

### Respond to an Issue

[Fork the Repository to your GitHub account](https://github.com/mpaulosky/AINotesApp/fork).

1. Create a new Branch from the main branch with a reference to the existing Issue number.
1. Work on the issue following the Vertical Slice Architecture pattern.
1. Create Unit, Integration tests for any code that require them. We use [xUnit](https://www.nuget.org/packages/xunit/)
   to test our code and [bUnit](https://www.nuget.org/packages/bunit/) to test our Blazor components.
1. Ensure all code includes comprehensive XML documentation comments.
1. Register any new handlers in Program.cs as Scoped services.
1. When you are done Create a Pull Request from your branch to the main branch.
1. Submit the Pull Request.

**Note:** Pull requests without unit tests will be delayed until tests are added. All new features and bug fixes must
include appropriate tests.

Any code that is written to support a blazor component or new functionality are required to be accompanied with unit
tests at the time the pull request is submitted. Pull requests without unit tests will be delayed and asked for unit
tests to prove their functionality.

### Review Process

1. All PRs are reviewed by maintainers and may require changes before merging.
2. Automated checks (build, tests, lint) must pass before review.
3. Be responsive to feedback and update your PR as needed.
4. Once approved, your PR will be merged into `main`.

### Write code

All code should have an assigned issue that matches it. This way we can prevent contributors from working on the same
feature at the same time.

Code for components' features should also include some definition in the `/docs` folder so that our users can
identify and understand which feature is supported.

See [docs/](../docs) for feature documentation guidelines.

### Write documentation

The documentation for the project is always needed. We are always looking for help to add content to the `/docs`
section of the repository with proper links back through to the main `/README.md`.

---

Thank you for helping us make this project better!
