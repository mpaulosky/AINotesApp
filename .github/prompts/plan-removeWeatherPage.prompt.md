# Plan: Remove Weather Page and Related Components

**Branch:** `remove-weather-page`
**Description:** Remove the demo Weather page component and all associated tests, navigation links, and documentation references.

## Goal
The Weather page is a demo/template component that is not needed for the AI Notes App functionality. This removal will clean up the codebase by eliminating unnecessary demo code, tests, and documentation references, keeping the application focused on its core note-taking features.

## Implementation Steps

### Step 1: Remove Weather Component and Tests
**Files:** 
- `AINotesApp/Components/Pages/Weather.razor`
- `tests/AINotesApp.Tests.Unit/Components/Pages/WeatherTests.cs`

**What:** Delete the Weather Blazor component (65 lines) and its complete unit test suite (162 lines, 10 tests covering rendering, loading states, data display, and temperature conversion).

**Testing:** Verify the solution builds successfully with `dotnet build` and confirm no compilation errors reference the deleted Weather component.

### Step 2: Remove Navigation Menu References
**Files:** 
- `AINotesApp/Components/Layout/NavMenu.razor`
- `tests/AINotesApp.Tests.Unit/Components/Layout/NavMenuTests.cs`

**What:** Remove the Weather navigation link (lines 36-39 in NavMenu.razor: the nav-item div containing the weather NavLink). Remove the `NavMenu_ShouldRender_WeatherLink` test (lines 71-77 in NavMenuTests.cs) that verifies the Weather link exists.

**Testing:** Run `dotnet test tests/AINotesApp.Tests.Unit/AINotesApp.Tests.Unit.csproj` to verify NavMenu tests pass without the Weather link test. Manually verify the navigation menu renders correctly without the Weather option.

### Step 3: Update Test Documentation
**Files:**
- `tests/TEST_COVERAGE_SUMMARY.md`
- `tests/README.md`

**What:** Remove Weather references from test documentation. In TEST_COVERAGE_SUMMARY.md, remove the Weather section (lines 55-57: "Weather - 10 tests" with bullet points). In README.md, remove "Weather - 10 tests" entry (line 23) from the Page components list. Update test counts if totals are affected.

**Testing:** Review both documentation files to ensure Weather is completely removed and the document formatting remains consistent with proper markdown structure.

## Further Considerations

1. **Test Count Updates:** Should we update the total test count numbers in documentation headers/summaries, or are those automatically calculated? If manual, they need adjustment (reducing by 11 tests: 10 Weather + 1 NavMenu).

2. **Related Seed Data:** The file `AINotesApp/Features/Notes/SeedNotes/SeedNotes.cs` (line 108) contains "dynamic weather" in a note about Zelda. This is unrelated to the Weather component—should we leave this content unchanged?

3. **Commit Strategy:** This can be implemented as a single commit since all changes are simple deletions with no logic modifications, or split into 3 commits matching the steps above for clearer history. What's your preference?

## Summary of Changes

### Files to Delete (2)
- `AINotesApp/Components/Pages/Weather.razor` (65 lines)
- `tests/AINotesApp.Tests.Unit/Components/Pages/WeatherTests.cs` (162 lines)

### Files to Modify (4)
- `AINotesApp/Components/Layout/NavMenu.razor` (remove 4 lines: Weather nav link)
- `tests/AINotesApp.Tests.Unit/Components/Layout/NavMenuTests.cs` (remove 7 lines: Weather link test)
- `tests/TEST_COVERAGE_SUMMARY.md` (remove 3 lines: Weather test section)
- `tests/README.md` (remove 1 line: Weather test entry)

### Impact
- **Tests Removed:** 11 total (10 Weather component tests + 1 NavMenu Weather link test)
- **Lines of Code Removed:** ~242 lines
- **Breaking Changes:** None (Weather is a demo page with no dependencies)
- **User-Facing Changes:** Weather link removed from navigation menu