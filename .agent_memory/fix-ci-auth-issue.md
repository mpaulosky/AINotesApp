# Fix CI/CD Auth0 Configuration Issue

## Problem

Integration tests failing in CI/CD with:
`System.InvalidOperationException : Both Client Secret and Client Assertion can not be null when requesting an access token`

## Root Cause

The Auth0 configuration requires `Auth0:ClientSecret` which is:

- Set via user secrets locally
- NOT set in CI environment
- Integration tests use `WebApplicationFactory<Program>` which starts the real app
- App tries to configure Auth0 authentication, but secrets are missing in CI

## Solution Implemented ✅

### 1. Created CustomWebApplicationFactory

- File: `tests/AINotesApp.Tests.Integration/Helpers/CustomWebApplicationFactory.cs`
- Overrides Auth0 configuration with dummy test values
- Replaces SQL Server DbContext with InMemory provider
- Provides dummy OpenAI configuration

### 2. Updated ProgramTests.cs

- Changed from `WebApplicationFactory<Program>` to `CustomWebApplicationFactory`
- All tests now use test-safe configuration

### 3. Updated appsettings.json

- Added Auth0 configuration block (was missing in base config)

### 4. Updated GitHub Actions Workflow

- Added environment variables for Auth0 test configuration
- Added OpenAI test configuration
- These serve as fallback/documentation

## Additional Fix (Dec 20, 2025) ✅

### Issue

Two integration tests were failing with:
`System.ArgumentException: Value cannot be an empty string. (Parameter 'key')`

**Failing tests:**

- `Required_Services_Are_Registered`
- `AiService_Is_Scoped_Not_Singleton`

### Root Cause

The OpenAI configuration section name was incorrect:

- Used: `AiService:ApiKey`
- Correct: `OpenAI:ApiKey` (per `AiServiceOptions.SectionName = "OpenAI"`)

The OpenAI SDK (ChatClient/EmbeddingClient) requires a non-empty API key, and the configuration wasn't binding correctly.

### Fix Applied

Updated `CustomWebApplicationFactory.cs` and `build-and-test.yml` to use correct configuration keys:

- Changed `AiService:*` to `OpenAI:*`
- Updated `OpenAI:ChatModel` to use correct default value `"gpt-4o-mini"`

## Test Results ✅

All 28 integration tests pass locally:

- Test summary: total: 28, failed: 0, succeeded: 28, skipped: 0

## Files Changed

1. `tests/AINotesApp.Tests.Integration/Helpers/CustomWebApplicationFactory.cs` (NEW/MODIFIED)
2. `tests/AINotesApp.Tests.Integration/ProgramTests.cs` (MODIFIED)
3. `AINotesApp/appsettings.json` (MODIFIED - added Auth0 config)
4. `.github/workflows/build-and-test.yml` (MODIFIED - corrected env vars)
