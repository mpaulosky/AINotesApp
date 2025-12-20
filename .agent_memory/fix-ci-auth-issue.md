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

## Solution Strategy

Create a custom `WebApplicationFactory` that:

1. Detects test/CI environment
2. Overrides Auth0 configuration with dummy/test values
3. OR mocks the authentication entirely for integration tests

## Current State

- ✅ Identified the issue in `AuthenticationServiceExtensions.cs` line 40-43
- ✅ Located integration tests in `ProgramTests.cs`
- ❌ No custom WebApplicationFactory exists
- ❌ No test configuration for Auth0

## Next Steps

1. Create custom `WebApplicationFactory` for integration tests
2. Override configuration to provide dummy Auth0 values
3. Update `ProgramTests.cs` to use the custom factory
4. Optionally: Add test environment secrets to GitHub Actions workflow
