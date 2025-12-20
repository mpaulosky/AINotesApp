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

## Test Results ✅
All 28 integration tests pass locally:
- Test summary: total: 28, failed: 0, succeeded: 28, skipped: 0

## Files Changed
1. `tests/AINotesApp.Tests.Integration/Helpers/CustomWebApplicationFactory.cs` (NEW)
2. `tests/AINotesApp.Tests.Integration/ProgramTests.cs` (MODIFIED)
3. `AINotesApp/appsettings.json` (MODIFIED - added Auth0 config)
4. `.github/workflows/build-and-test.yml` (MODIFIED - added env vars)

