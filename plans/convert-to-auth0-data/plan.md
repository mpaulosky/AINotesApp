# Plan: Auth0 Data & Feature Updates

## Scope

Align the persistence layer and core Notes features with Auth0 subject identifiers instead of Identity GUIDs.

## Prerequisites

- Auth0 bootstrap and UI integration plans completed.
- Auth0 `sub` values available for all users (migration plan handles mapping).

## Steps

1. **Introduce `AppUser` entity**
    - File: `AINotesApp/Data/AppUser.cs` (new).
    - Properties: `public string Auth0Subject { get; set; } = null!;` (key), `Name`, `Email`, `CreatedUtc`.
    - Configure `ApplicationDbContext` to expose `DbSet<AppUser>`.

2. **Update `ApplicationDbContext`**
    - File: `AINotesApp/Data/ApplicationDbContext.cs`
        - Change base class from `IdentityDbContext<ApplicationUser>` to `DbContext`.
        - Remove Identity-specific OnModelCreating calls.
        - Configure `AppUser` entity with key `Auth0Subject` and indexes for email.
        - Update `Note` configuration to use `OwnerSubject` instead of `OwnerId` (string length 64).

3. **Adjust `Note` entity**
    - File: `AINotesApp/Data/Note.cs`
        - Rename `OwnerId` to `OwnerSubject` (string, required).
        - Update constructors and helper methods accordingly.

4. **Migration script**
    - Add EF Core migration `Auth0SubjectMigration`.
        - Add column `OwnerSubject` to `Notes`.
        - Populate using mapping table or script (document placeholder for now: set to existing `OwnerId` as string to
          unblock compile; actual mapping handled externally).
        - Drop `OwnerId` once data migration confirmed.
        - Remove Identity tables only after Auth0 rollout (can remain for now but mark for cleanup).

5. **Notes feature updates**
    - Files under `AINotesApp/Features/Notes/**`:
        - `CreateNote`, `UpdateNote`, `DeleteNote`, `ListNotes`, `SearchNotes`, `GetNoteDetails`, `GetRelatedNotes`,
          `BackfillTags`, `SeedNotes`.
        - Replace usages of `context.User.FindFirst(ClaimTypes.NameIdentifier)` with `FindFirst("sub")`.
        - Update queries to filter by `OwnerSubject`.
        - Ensure DTOs, response models, and validators use string subject IDs.

6. **Services relying on user ID**
    - Any helper classes (e.g., AI services) that cache by user id must switch to string subjects.

7. **Verification**
    - Run
      `dotnet ef migrations add Auth0SubjectMigration --project AINotesApp/AINotesApp.csproj --startup-project AINotesApp/AINotesApp.csproj`.
    - Apply to local DB.
    - `dotnet test tests/AINotesApp.Tests.Integration` focusing on Notes scenarios.