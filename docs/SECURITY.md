# Security Policy

## Supported Versions

The following versions of AINotesApp are currently supported with security updates:

| Version | Supported          |
|---------|--------------------|
| 0.1.x   | :white_check_mark: |
| < 0.1   | :x:                |

**Note:** This is an early-stage project. Security updates will be provided for the latest 0.1.x release. Once the
project reaches 1.0, we will maintain security support for the current major version and one previous major version.

## Security Features

AINotesApp implements the following security measures:

### Authentication & Authorization

- **Auth0 Authentication** - Enterprise-grade OAuth 2.0 and OpenID Connect authentication
- **Auth0 Universal Login** - Secure, centralized login experience with MFA support
- **Role-Based Access Control (RBAC)** - Auth0 roles for authorization (e.g., `notes.admin`)
- **Per-user data isolation** - Users can only access their own notes
- **Authorization checks** - All CQRS handlers verify user ownership using Auth0 subject (`sub` claim)
- **Secure session management** - OIDC tokens with configurable expiration

### Data Protection

- **SQL injection protection** - Entity Framework Core parameterized queries
- **XSS protection** - Blazor's automatic HTML encoding
- **CSRF protection** - Built-in anti-forgery tokens
- **HTTPS enforcement** - Recommended for production deployments

### API Security

- **OpenAI API key protection** - Stored in user secrets or environment variables
- **Input validation** - All commands validate user input
- **Error handling** - Sensitive information not exposed in error messages

### Database Security

- **User isolation** - Database queries filtered by Auth0 subject (`OwnerSubject`)
- **Migration safety** - Code-first migrations with version control
- **Connection string security** - Stored in appsettings.json (excluded from source control for production)

### Auth0 Configuration

AINotesApp requires proper Auth0 tenant configuration for secure authentication:

#### Required Configuration

The following settings must be configured in `appsettings.json`:

```json
"Auth0": {
  "Domain": "your-tenant.us.auth0.com",
  "ClientId": "your-client-id",
  "Audience": "https://api.yourapp.com",
  "CallbackPath": "/auth/callback",
  "LogoutPath": "/auth/logout"
}
```

#### Required User Secrets

The Auth0 Client Secret **MUST NOT** be stored in `appsettings.json`. Use one of these secure methods:

**Local Development - User Secrets:**
```bash
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
```

**Production - Environment Variables:**
```bash
# Linux/Mac
export Auth0__ClientSecret="your-client-secret"

# Windows PowerShell
$env:Auth0__ClientSecret = "your-client-secret"

# Azure App Service - Set in Configuration > Application Settings
Auth0__ClientSecret = your-client-secret
```

#### Auth0 Tenant Setup

1. **Create Auth0 Application:**
   - Type: Regular Web Application
   - Allowed Callback URLs: `https://your-domain.com/auth/callback`
   - Allowed Logout URLs: `https://your-domain.com/`

2. **Configure API:**
   - Create an API in Auth0 with identifier matching `Auth0:Audience`
   - Enable RBAC
   - Enable "Add Permissions in the Access Token"

3. **Configure Roles (Optional):**
   - Create role: `notes.admin` for administrative access
   - Assign to users requiring admin features

4. **Configure Token Settings:**
   - Token Expiration: 86400 seconds (24 hours) recommended
   - Refresh Tokens: Enable for seamless re-authentication

## Reporting a Vulnerability

If you discover a security vulnerability in AINotesApp, please report it responsibly:

### How to Report

**Email:** matthew.paulosky@outlook.com  
**Subject:** [SECURITY] AINotesApp Vulnerability Report

**Please do NOT open a public GitHub issue for security vulnerabilities.**

### What to Include
  - **Required:** `Auth0:ClientSecret` must be stored in user secrets
  - Command: `dotnet user-secrets set "Auth0:ClientSecret" "your-secret"`
- Use **Environment Variables** for production (e.g., Azure App Service Configuration)
  - Format: `Auth0__ClientSecret` (double underscore)
- Use **Azure Key Vault** or similar for production secrets management
- Never commit `appsettings.Production.json` with secrets
- Never commit `appsettings.Development.json` with Auth0:ClientSecret
- Add sensitive files to `.gitignore`

**Security Check:**
```bash
# Verify no secrets in version control
git grep -i "clientsecret" -- "*.json"  # Should return no results
``
1. **Description** - Clear description of the vulnerability
2. **Impact** - Potential security impact and severity
3. **Steps to Reproduce** - Detailed steps to reproduce the vulnerability
4. **Affected Versions** - Which versions are affected
5. **Suggested Fix** - If you have ideas for mitigation (optional)
6. **Your Contact Info** - How we can reach you for follow-up

### Response Timeline

- **Initial Response:** Within 48 hours of report submission
- **Status Update:** Within 7 days with assessment and timeline
- **Fix Timeline:**
    - Critical vulnerabilities: Within 7 days
    - High severity: Within 14 days
    - Medium/Low severity: Within 30 days

### Disclosure Policy

- We will work with you to understand and validate the vulnerability
- We will develop and test a fix before public disclosure
- We will credit you in the security advisory (unless you prefer anonymity)
- We request that you do not publicly disclose the vulnerability until we have released a fix

### Security Advisories

Security updates will be published:

- In the [GitHub Security Advisories](https://github.com/mpaulosky/AINotesApp/security/advisories)
- In the project [CHANGELOG.md](../CHANGELOG.md) (if one exists)
- In release notes for security-related releases

## Security Best Practices for Contributors

When contributing to AINotesApp, please follow these security guidelines:

### Code Review

- All code changes require review before merging
- Security-sensitive changes require additional scrutiny
- Never commit secrets, API keys, or passwords

### Testing

- Add security-focused tests for authorization checks
- Test boundary conditions and edge cases
- Verify user isolation in integration tests

### Dependencies

- Keep NuGet packages up to date
- Review dependency security advisories
- Use `dotnet list package --vulnerable` to check for known vulnerabilities

### Secrets Management

- Use **User Secrets** for local development (`dotnet user-secrets`)
- Use **Environment Variables** for production
- Never commit `appsettings.Production.json` with secrets
- Add sensitive files to `.gitignore`

### Data Validation
Auth0 Free Tier** - Free tier has limits (7,000 active users, 2 social connections)
- **No rate limiting** - Consider implementing rate limiting for production
- **No audit logging** - User actions are not currently logged (consider Auth0 Logs for authentication events)
- Use parameterized queries (Entity Framework Core does this automatically)
- Sanitize data before rendering in Blazor components (Blazor does this automatically)
 (required for Auth0)
2. **Secure connection strings** - Use Azure Key Vault or similar
3. **Secure Auth0 secrets** - Store `Auth0:ClientSecret` in Azure Key Vault or environment variables
4. **Enable Auth0 MFA** - Require multi-factor authentication for sensitive accounts
5. **Monitor Auth0 Logs** - Enable and monitor authentication logs in Auth0 dashboard
6. **Enable logging** - Add application security event logging
7. **Rate limiting** - Implement API rate limiting (consider Auth0 Anomaly Detection)
8. **Regular updates** - Keep .NET, Auth0 SDK, and dependencies updated
9. **Security headers** - Add security headers (CSP, X-Frame-Options, etc.)
10. **Monitor dependencies** - Use GitHub Dependabot for security alerts
11. **Auth0 Attack Protection** - Enable brute-force protection and suspicious IP throttlingon
- **No rate limiting** - Consider implementing rate limiting for production
- **No audit logging** - User actions are not currently logged

### Recommendations for Production

1. **Use HTTPS** - Enable HTTPS and HSTS
2. **Secure connection strings** - Use Azure Key Vault or similar
3. **Enable logging** - Add security event logging
4. **Rate limiting** - Implement API rate limiting
5. **Regular updates** - Keep .NET and dependencies updated
6. **Security headers** - Add security headers (CSP, X-Frame-Options, etc.)
7. **Monitor dependencies** - Use GitHub Dependabot for security alerts

## Security Resources
- [Auth0 Security Best Practices](https://auth0.com/docs/secure)
- [Auth0 Attack Protection](https://auth0.com/docs/secure/attack-protection)
- [OpenID Connect Security](https://openid.net/specs/openid-connect-core-1_0.html#Security)

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/aspnet/core/security/)
- [Entity Framework Core Security](https://learn.microsoft.com/ef/core/miscellaneous/security)
- [Blazor Security](https://learn.microsoft.com/aspnet/core/blazor/security/)

---

Thank you for helping keep AINotesApp secure!