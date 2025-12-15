# Plan: Auth0 Tests & Documentation

## Scope

Update automated tests, helpers, and documentation to reflect Auth0 authentication, ensuring CI/CD and developer
guidance remain accurate.

## Dependencies

- Auth0 bootstrap, UI, and data plans completed.

## Steps

1. **Test helpers**
    - File: `tests/AINotesApp.Tests.Unit/Fakes/FakeAuthenticationStateProvider.cs`
        - Update to emit Auth0-style claims: `sub`, `name`, `email`, `roles` (if needed) instead of Identity GUIDs.
    - File: `tests/AINotesApp.Tests.Unit/Helpers/TestAuthHelper.cs`
        - Provide constants for sample subjects (e.g., `auth0|123456`).

2. **Component tests**
    - Update the following files to expect Auth0 claims and new UI components:
        - `tests/AINotesApp.Tests.Unit/Components/Layout/MainLayoutTests.cs`
        - `tests/AINotesApp.Tests.Unit/Components/Pages/Notes/NotesListTests.cs`
        - `tests/AINotesApp.Tests.Unit/Components/Pages/Notes/NoteDetailsTests.cs`
        - `tests/AINotesApp.Tests.Unit/Components/Pages/Notes/NoteEditorTests.cs`
        - `tests/AINotesApp.Tests.Unit/Components/Pages/AuthTests.cs` (if present).
        - Ensure tests render new `Login`/`Logout` components and verify `AuthorizeView` behavior.

3. **Integration tests**
    - Project: `tests/AINotesApp.Tests.Integration`
        - Replace Identity test server setup with Auth0 test handler (e.g., register a fake OIDC handler that issues
          claims with `sub`).
        - Update seed data to use `OwnerSubject`.

4. **Architecture tests**
    - Project: `tests/AINotesApp.Tests.Architecture`
        - Ensure references to Identity namespaces are removed from expected lists.

5. **Documentation updates**
    - File: `README.md`
        - Update prerequisites to mention Auth0 tenant requirement and how to configure `appsettings` values.
        - Replace Identity setup instructions with Auth0 login flow.
    - File: `docs/SECURITY.md`
        - Document Auth0 authentication, RBAC role `notes.admin`, and secret management (`Auth0:ClientSecret`).
    - File: `docs/REFERENCES.md`
        - Add Auth0 docs links used.
    - Add `plans/convert-to-auth0/migration-notes.md` summarizing user migration steps (export Identity users, import
      into Auth0, map IDs).

6. **CI/CD adjustments**
    - If workflows exist under `.github/workflows`, update to inject `Auth0__Domain`, `Auth0__ClientId`,
      `Auth0__ClientSecret` via secrets.

7. **Verification**
    - Run all tests: `dotnet test AINotesApp.sln`.
    - Lint documentation for typos (manual review).