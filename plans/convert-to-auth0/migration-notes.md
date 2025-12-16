# Identity to Auth0 Migration Guide

## Overview

This document provides step-by-step instructions for migrating from ASP.NET Core Identity to Auth0 authentication, including user data migration strategies and rollback procedures.

**Target Audience:** Development team, DevOps engineers, Database administrators  
**Estimated Migration Time:** 4-6 hours (excluding user data migration)  
**Risk Level:** Medium - Requires careful user data migration

---

## Migration Strategy

### Option 1: Fresh Start (Recommended for New Projects)

If the application is not yet in production or has no existing users:

1. ✅ Complete Auth0 code migration (already done)
2. ✅ Remove Identity package references (already done)
3. Create fresh database with Auth0 schema
4. Deploy application
5. Users create new accounts via Auth0

**Pros:**
- Clean slate with no legacy data
- Simplest approach
- No user data migration risks

**Cons:**
- Existing users must create new accounts
- Loses historical user data

### Option 2: User Migration with Import (For Production with Existing Users)

Export existing Identity users and import them into Auth0, preserving user identities.

**Pros:**
- Preserves existing user base
- Maintains user continuity
- Can preserve some user metadata

**Cons:**
- More complex migration process
- Password hashes cannot be migrated directly
- Requires user password resets

---

## Pre-Migration Checklist

Before beginning the migration:

- [ ] **Backup Production Database** - Full backup before any changes
- [ ] **Auth0 Tenant Created** - Production Auth0 tenant configured
- [ ] **Auth0 Application Configured** - Application settings match production URLs
- [ ] **Auth0 API Created** - API identifier configured
- [ ] **Secrets Configured** - `Auth0:ClientSecret` in production environment
- [ ] **Connection Strings Updated** - Production database connection configured
- [ ] **Testing Complete** - All 272 tests passing in staging environment
- [ ] **Rollback Plan Documented** - Clear steps to revert if needed
- [ ] **Communication Plan** - Users notified of maintenance window
- [ ] **Monitoring Setup** - Application Insights or logging configured

---

## Step 1: Export Identity Users

### Export User Data from Identity Database

```sql
-- Export AspNetUsers to CSV for Auth0 import
SELECT 
    Id AS user_id,
    UserName AS username,
    Email AS email,
    EmailConfirmed AS email_verified,
    PhoneNumber AS phone_number,
    PhoneNumberConfirmed AS phone_verified,
    TwoFactorEnabled AS mfa_enabled,
    LockoutEnd AS lockout_end,
    AccessFailedCount AS failed_login_count
FROM AspNetUsers
WHERE Email IS NOT NULL
ORDER BY Email;
```

Save results to: `identity_users_export.csv`

### Export User Notes for Data Mapping

```sql
-- Export Notes with UserId for data migration mapping
SELECT 
    Id AS note_id,
    UserId AS old_user_id,
    Title,
    Content,
    Tags,
    Summary,
    CreatedOn,
    ModifiedOn
FROM Notes
ORDER BY UserId, CreatedOn;
```

Save results to: `notes_export_backup.csv`

---

## Step 2: Import Users to Auth0

### Method A: Auth0 Management API (Programmatic)

**Prerequisites:**
- Auth0 Management API Application created
- API token with `create:users` and `update:users` scopes

**PowerShell Script for User Import:**

```powershell
# Import-Auth0Users.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$CsvPath,
    
    [Parameter(Mandatory=$true)]
    [string]$Auth0Domain,
    
    [Parameter(Mandatory=$true)]
    [string]$ManagementToken
)

$users = Import-Csv -Path $CsvPath

foreach ($user in $users) {
    $body = @{
        email = $user.email
        email_verified = [bool]$user.email_verified
        user_metadata = @{
            legacy_user_id = $user.user_id
        }
        app_metadata = @{
            migrated_from = "identity"
            migration_date = (Get-Date).ToString("o")
        }
        connection = "Username-Password-Authentication"
    } | ConvertTo-Json

    $headers = @{
        "Authorization" = "Bearer $ManagementToken"
        "Content-Type" = "application/json"
    }

    try {
        $response = Invoke-RestMethod `
            -Uri "https://$Auth0Domain/api/v2/users" `
            -Method Post `
            -Headers $headers `
            -Body $body
        
        Write-Host "✓ Imported user: $($user.email) -> Auth0 ID: $($response.user_id)" -ForegroundColor Green
        
        # Store mapping for notes migration
        Add-Content -Path "user_id_mapping.csv" -Value "$($user.user_id),$($response.user_id)"
    }
    catch {
        Write-Host "✗ Failed to import user: $($user.email)" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds 250  # Rate limiting
}
```

**Usage:**
```powershell
.\Import-Auth0Users.ps1 `
    -CsvPath "identity_users_export.csv" `
    -Auth0Domain "your-tenant.us.auth0.com" `
    -ManagementToken "your-management-api-token"
```

### Method B: Auth0 User Import Extension

1. Install Auth0 User Import/Export Extension
2. Prepare CSV in Auth0 format:

```csv
email,email_verified,user_metadata.legacy_user_id
user1@example.com,true,old-guid-1
user2@example.com,true,old-guid-2
```

3. Upload CSV via extension
4. Review import results

---

## Step 3: Migrate Note Ownership

### Update Notes Table with Auth0 Subject IDs

**Prerequisites:**
- `user_id_mapping.csv` from Step 2 (columns: `old_user_id,auth0_subject`)

**SQL Script:**

```sql
-- Step 3A: Add OwnerSubject column (if not exists)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Notes') 
               AND name = 'OwnerSubject')
BEGIN
    ALTER TABLE Notes
    ADD OwnerSubject NVARCHAR(256) NULL;
END
GO

-- Step 3B: Create temporary mapping table
CREATE TABLE #UserIdMapping (
    OldUserId NVARCHAR(450),
    Auth0Subject NVARCHAR(256)
);

-- Step 3C: Import mapping data
BULK INSERT #UserIdMapping
FROM 'C:\path\to\user_id_mapping.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2  -- Skip header
);

-- Step 3D: Update Notes with Auth0 subjects
UPDATE n
SET n.OwnerSubject = m.Auth0Subject
FROM Notes n
INNER JOIN #UserIdMapping m ON n.UserId = m.OldUserId;

-- Step 3E: Verify migration
SELECT 
    COUNT(*) AS TotalNotes,
    COUNT(OwnerSubject) AS MigratedNotes,
    COUNT(*) - COUNT(OwnerSubject) AS UnmigratedNotes
FROM Notes;

-- Step 3F: Check for unmigrated notes (should be 0)
SELECT Id, UserId, Title
FROM Notes
WHERE OwnerSubject IS NULL;

-- Step 3G: Make OwnerSubject NOT NULL after verification
ALTER TABLE Notes
ALTER COLUMN OwnerSubject NVARCHAR(256) NOT NULL;

-- Step 3H: Create index on OwnerSubject
CREATE NONCLUSTERED INDEX IX_Notes_OwnerSubject
ON Notes(OwnerSubject)
INCLUDE (Id, Title, Summary, CreatedOn, ModifiedOn);

-- Step 3I: Drop old UserId column (AFTER verifying everything works)
-- ALTER TABLE Notes DROP COLUMN UserId;

-- Cleanup
DROP TABLE #UserIdMapping;
```

---

## Step 4: User Password Reset Communication

Since password hashes cannot be migrated from Identity to Auth0, users must reset their passwords.

### Option A: Forced Password Reset

Configure Auth0 to require password reset on first login:

```json
{
  "change_password": {
    "enabled": true
  }
}
```

### Option B: Password Reset Email Campaign

Send bulk password reset emails via Auth0:

```powershell
# Send-PasswordResetEmails.ps1
$users = Import-Csv -Path "user_id_mapping.csv"

foreach ($user in $users) {
    $body = @{
        client_id = "your-client-id"
        email = $user.email
        connection = "Username-Password-Authentication"
    } | ConvertTo-Json

    Invoke-RestMethod `
        -Uri "https://your-tenant.us.auth0.com/dbconnections/change_password" `
        -Method Post `
        -Headers @{"Content-Type" = "application/json"} `
        -Body $body
    
    Write-Host "✓ Password reset sent to: $($user.email)"
    Start-Sleep -Milliseconds 500
}
```

### User Communication Template

**Subject:** Action Required: Reset Your AINotesApp Password

**Body:**
```
Hi [Name],

We've upgraded AINotesApp to use Auth0 for improved security and reliability.

ACTION REQUIRED:
1. Visit: https://yourapp.com/auth/reset-password
2. Enter your email address
3. Check your email for reset instructions
4. Create a new password

Your notes and data remain safe and unchanged.

Questions? Contact support@yourapp.com

Thank you,
The AINotesApp Team
```

---

## Step 5: Deploy Auth0-Enabled Application

### Deployment Checklist

- [ ] **Environment Variables Set:**
  - `Auth0__Domain`
  - `Auth0__ClientId`
  - `Auth0__ClientSecret`
  - `Auth0__Audience`

- [ ] **Database Migration Applied:**
  ```bash
  dotnet ef database update --project AINotesApp
  ```

- [ ] **Application Settings Verified:**
  - `appsettings.Production.json` contains correct Auth0 configuration
  - Connection strings point to production database

- [ ] **Health Checks:**
  - Application starts successfully
  - Auth0 login flow works
  - User can access their notes

### Azure App Service Deployment

```bash
# Set Auth0 configuration
az webapp config appsettings set \
  --name your-app-name \
  --resource-group your-rg \
  --settings \
    Auth0__Domain="your-tenant.us.auth0.com" \
    Auth0__ClientId="your-client-id" \
    Auth0__ClientSecret="your-client-secret" \
    Auth0__Audience="https://api.yourapp.com"

# Deploy application
az webapp deployment source config-zip \
  --name your-app-name \
  --resource-group your-rg \
  --src ./publish.zip
```

---

## Step 6: Verification & Testing

### Test Suite

1. **Authentication Tests:**
   - [ ] User can log in via Auth0
   - [ ] User can log out
   - [ ] Unauthorized access redirects to login
   - [ ] Access denied page displays correctly

2. **Data Access Tests:**
   - [ ] User can view their notes
   - [ ] User cannot view other users' notes
   - [ ] Create note works
   - [ ] Update note works
   - [ ] Delete note works

3. **AI Features Tests:**
   - [ ] Note summarization works
   - [ ] Tag generation works
   - [ ] Semantic search works

4. **Admin Features Tests:**
   - [ ] Admin user with `notes.admin` role can access admin pages
   - [ ] Non-admin users cannot access admin pages

### Automated Test Execution

```bash
# Run all tests
dotnet test

# Expected Results:
# - Total: 272
# - Passed: 272
# - Failed: 0
```

---

## Rollback Plan

If issues occur during migration, follow these steps to rollback:

### Immediate Rollback (< 1 hour after deployment)

1. **Revert to Previous Deployment:**
   ```bash
   az webapp deployment slot swap \
     --name your-app-name \
     --resource-group your-rg \
     --slot staging
   ```

2. **Restore Database Backup:**
   ```sql
   RESTORE DATABASE AINotesAppDb
   FROM DISK = 'C:\Backups\pre-auth0-migration.bak'
   WITH REPLACE;
   ```

3. **Verify Identity Authentication Works**

### Extended Rollback (> 1 hour, with new Auth0 users)

If Auth0 migration has been live and new users have registered:

1. **Export Auth0 Users Created Since Migration**
2. **Merge with Identity Users**
3. **Restore Database with Merged Data**
4. **Update Configuration to Use Identity**

**Note:** This scenario is complex. Prevention is better than cure - thoroughly test before production deployment.

---

## Monitoring & Troubleshooting

### Key Metrics to Monitor

1. **Authentication Success Rate:**
   - Monitor Auth0 logs for failed logins
   - Target: > 95% success rate

2. **Authorization Failures:**
   - Check application logs for 403 errors
   - Investigate if > 1% of requests fail

3. **Note Access Patterns:**
   - Verify users can access their notes
   - Monitor for cross-user access attempts (should be 0)

4. **Performance:**
   - Monitor page load times
   - Auth0 token validation should be < 100ms

### Common Issues & Solutions

**Issue:** Users cannot log in  
**Solution:** 
- Verify Auth0 application callback URLs
- Check `Auth0:ClientSecret` is set correctly
- Review Auth0 tenant logs for errors

**Issue:** Users see 403 Forbidden errors  
**Solution:**
- Verify Auth0 API audience configuration
- Check that access tokens include required claims
- Ensure user subject (`sub`) is being extracted correctly

**Issue:** Notes not displaying for migrated users  
**Solution:**
- Verify `user_id_mapping.csv` was applied correctly
- Check `OwnerSubject` column populated for all notes
- Query: `SELECT * FROM Notes WHERE OwnerSubject IS NULL`

**Issue:** New users cannot create notes  
**Solution:**
- Verify Auth0 subject extraction in handlers
- Check that `HttpContext.User.FindFirst("sub")?.Value` returns valid ID
- Review application logs for null reference exceptions

---

## Post-Migration Cleanup

After successful migration and verification (recommend waiting 30 days):

### Remove Identity Tables

```sql
-- Backup Identity tables before dropping
SELECT * INTO AspNetUsers_Backup FROM AspNetUsers;
SELECT * INTO AspNetRoles_Backup FROM AspNetRoles;
SELECT * INTO AspNetUserRoles_Backup FROM AspNetUserRoles;

-- Drop Identity tables
DROP TABLE AspNetUserTokens;
DROP TABLE AspNetUserRoles;
DROP TABLE AspNetUserLogins;
DROP TABLE AspNetUserClaims;
DROP TABLE AspNetRoleClaims;
DROP TABLE AspNetRoles;
DROP TABLE AspNetUsers;

-- Drop old UserId column from Notes (if not done in Step 3)
ALTER TABLE Notes DROP COLUMN UserId;
```

### Remove Identity Code References

All Identity code references have already been removed in the Auth0 migration.

---

## Resources

### Auth0 Documentation
- [User Migration Overview](https://auth0.com/docs/manage-users/user-migration)
- [Management API v2](https://auth0.com/docs/api/management/v2)
- [Bulk User Import](https://auth0.com/docs/manage-users/user-migration/bulk-user-imports)
- [Password Reset](https://auth0.com/docs/connections/database/password-change)

### Application Documentation
- [SECURITY.md](../../docs/SECURITY.md) - Auth0 configuration details
- [README.md](../../README.md) - Setup instructions
- [STATUS-REVIEW.md](./STATUS-REVIEW.md) - Migration status

---

## Summary

This migration guide provides a comprehensive approach to migrating from ASP.NET Core Identity to Auth0 authentication, including:

✅ User data export from Identity database  
✅ User import to Auth0 via Management API  
✅ Note ownership migration to Auth0 subjects  
✅ Password reset communication strategy  
✅ Deployment procedures  
✅ Verification and testing steps  
✅ Rollback plan  
✅ Monitoring and troubleshooting  
✅ Post-migration cleanup  

**Estimated Total Migration Time:** 4-6 hours (excluding user password resets)

**Risk Mitigation:**
- Full database backups before migration
- Comprehensive testing in staging environment
- Clear rollback procedures
- User communication plan
- Post-migration monitoring

---

**Last Updated:** January 2025  
**Migration Status:** Ready for execution  
**Contact:** Development Team
