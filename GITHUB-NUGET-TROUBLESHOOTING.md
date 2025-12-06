# GitHub NuGet Troubleshooting Guide

## Common Issues and Solutions

### 1. 401 Unauthorized Error

**Problem:** `Response status code does not indicate success: 401 (Unauthorized)`

**Solutions:**

#### A. Use .NET CLI to Add Credentials (Recommended)
```powershell
# Remove existing source
dotnet nuget remove source github

# Add with credentials
dotnet nuget add source https://nuget.pkg.github.com/mpaulosky/index.json `
    --name github `
    --username mpaulosky `
    --password YOUR_GITHUB_PAT `
    --store-password-in-clear-text
```

#### B. Verify Your GitHub PAT
1. Go to https://github.com/settings/tokens
2. Check your token has `read:packages` scope
3. Verify token hasn't expired
4. Try generating a new token if unsure

#### C. Check Environment Variables (if using)
```powershell
# Check if variables are set
$env:GITHUB_USERNAME
$env:GITHUB_TOKEN

# Set if missing
[System.Environment]::SetEnvironmentVariable('GITHUB_USERNAME', 'mpaulosky', 'User')
[System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', 'YOUR_PAT', 'User')

# Restart Visual Studio after setting
```

### 2. Package Not Found

**Problem:** Package exists on GitHub but isn't found

**Check:**
```powershell
# List all sources
dotnet nuget list source

# Try searching with specific source
dotnet search YourPackage --source github
```

### 3. Clear NuGet Cache

If you're having persistent issues:
```powershell
# Clear all NuGet caches
dotnet nuget locals all --clear

# Try restore again
dotnet restore
```

### 4. Verify GitHub Package Visibility

- Ensure the package is published to GitHub Packages
- Check package visibility (public vs private)
- Verify you have access to the repository/organization

### 5. Check NuGet Config Hierarchy

NuGet reads from multiple config files in this order:
1. Project-level: `E:\github\AINotesApp\nuget.config` ?
2. Solution-level: (if exists)
3. User-level: `%APPDATA%\NuGet\NuGet.Config`
4. Computer-level: `%ProgramFiles(x86)%\NuGet\Config\`

**View effective configuration:**
```powershell
dotnet nuget list source
```

### 6. Test Connection Manually

```powershell
# Test if you can authenticate to GitHub Packages
$headers = @{
    "Authorization" = "Bearer YOUR_GITHUB_PAT"
}

Invoke-RestMethod -Uri "https://nuget.pkg.github.com/mpaulosky/query?q=&prerelease=true" `
    -Headers $headers `
    -Method Get
```

## Quick Setup Script

Run the included setup script:
```powershell
.\setup-github-nuget.ps1
```

## Where Credentials Are Stored

### Option 1: Environment Variables
- **Location:** User environment variables
- **View:** System Properties ? Environment Variables
- **Requires:** Restart Visual Studio after setting

### Option 2: .NET NuGet Config
- **Location:** `%APPDATA%\NuGet\NuGet.Config`
- **Advantage:** No restart needed
- **View:** 
  ```powershell
  notepad "$env:APPDATA\NuGet\NuGet.Config"
  ```

### Option 3: Project nuget.config
- **Location:** `E:\github\AINotesApp\nuget.config`
- **Warning:** Don't commit credentials to Git!
- **Use:** Only with environment variables

## Verify Setup

```powershell
# 1. Check sources
dotnet nuget list source

# 2. Try restore
dotnet restore --verbosity detailed

# 3. Try searching for a package
dotnet search MyMediator --source github
```

## Still Having Issues?

1. **Check Visual Studio Output Window**
   - View ? Output
   - Show output from: Package Manager

2. **Enable Verbose Logging**
   ```powershell
   dotnet restore --verbosity diagnostic > restore-log.txt
   ```

3. **Verify GitHub Status**
   - Check https://www.githubstatus.com/

## Security Best Practices

? Use Personal Access Tokens (PAT), not passwords  
? Grant minimal scopes (only `read:packages` if just consuming)  
? Set token expiration dates  
? Never commit tokens to source control  
? Use different tokens for different purposes  
? Rotate tokens periodically  

## Contact

If you continue to have issues:
- Check package exists: `https://github.com/mpaulosky?tab=packages`
- Verify repository access
- Try with a fresh GitHub PAT
