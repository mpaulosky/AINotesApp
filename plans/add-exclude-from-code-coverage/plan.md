# Add ExcludeFromCodeCoverage Attribute to Test Classes

**Branch:** `chore/add-exclude-from-code-coverage`
**Description:** Ensures all test classes have the [ExcludeFromCodeCoverage] attribute to exclude test code from coverage metrics.

## Goal
Add the missing `[ExcludeFromCodeCoverage]` attribute to 3 test classes that currently don't have it. This ensures test code itself is not included in code coverage calculations, which is a best practice for accurate production code coverage metrics.

## Current State
- **Total test files:** 49
- **Files with attribute:** 46 ✓
- **Files missing attribute:** 3 ✗

## Implementation Steps

### Step 1: Add [ExcludeFromCodeCoverage] to Missing Test Classes

**Files:**
- `tests/AINotesApp.Tests.Unit/Features/Notes/BackfillTagsHandlerTests.cs`
- `tests/AINotesApp.Tests.Unit/Features/Notes/RegenerateEmbeddingsHandlerTests.cs`
- `tests/AINotesApp.Tests.Unit/Components/Pages/Account/RedirectToLoginTests.cs`

**What:** 
Add the `using System.Diagnostics.CodeAnalysis;` directive and `[ExcludeFromCodeCoverage]` attribute to each of the 3 test classes. This brings them into alignment with the 46 other test classes that already follow this pattern.

**Changes for each file:**
1. Add `using System.Diagnostics.CodeAnalysis;` with other using statements
2. Add `[ExcludeFromCodeCoverage]` attribute immediately above the class declaration

**Testing:** 
1. Build the solution: `dotnet build`
2. Run all tests to ensure no breaking changes: `dotnet test`
3. Verify all 555 tests still pass
4. Optional: Run code coverage to confirm test classes are excluded from metrics

## Notes
- This is a non-functional change that affects code quality and coverage reporting only
- No behavioral changes to tests or application code
- Follows existing codebase pattern established in 46 other test files
- Completes the standardization of test class attributes across the entire test suite
