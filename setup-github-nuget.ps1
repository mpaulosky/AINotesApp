# GitHub NuGet Package Source Setup Script
# Run this script to configure authentication for GitHub Packages

Write-Host "GitHub NuGet Package Source Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if GITHUB_TOKEN is already set
$existingToken = [System.Environment]::GetEnvironmentVariable('GITHUB_TOKEN', 'User')
if ($existingToken) {
    Write-Host "? GITHUB_TOKEN environment variable is already set" -ForegroundColor Green
    $response = Read-Host "Do you want to update it? (y/n)"
    if ($response -ne 'y') {
        Write-Host "Skipping environment variable setup" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "To create a GitHub Personal Access Token:" -ForegroundColor Yellow
Write-Host "1. Go to https://github.com/settings/tokens" -ForegroundColor White
Write-Host "2. Click 'Generate new token (classic)'" -ForegroundColor White
Write-Host "3. Select scope: read:packages (and write:packages if publishing)" -ForegroundColor White
Write-Host "4. Generate and copy the token" -ForegroundColor White
Write-Host ""

$username = Read-Host "Enter your GitHub username (default: mpaulosky)"
if ([string]::IsNullOrWhiteSpace($username)) {
    $username = "mpaulosky"
}

$token = Read-Host "Enter your GitHub Personal Access Token" -AsSecureString
$tokenPlainText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($token))

if ([string]::IsNullOrWhiteSpace($tokenPlainText)) {
    Write-Host "? Token cannot be empty!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setting environment variables..." -ForegroundColor Cyan

# Set environment variables
[System.Environment]::SetEnvironmentVariable('GITHUB_USERNAME', $username, 'User')
[System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', $tokenPlainText, 'User')

Write-Host "? Environment variables set successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT: You must restart Visual Studio for the changes to take effect!" -ForegroundColor Yellow
Write-Host ""

# Ask if user wants to add credentials to dotnet nuget
Write-Host "Alternatively, you can add credentials directly to .NET NuGet config (more reliable):" -ForegroundColor Cyan
$addToDotnet = Read-Host "Add credentials to .NET NuGet config? (y/n)"

if ($addToDotnet -eq 'y') {
    Write-Host ""
    Write-Host "Removing existing GitHub source..." -ForegroundColor Cyan
    dotnet nuget remove source github 2>$null
    
    Write-Host "Adding GitHub source with credentials..." -ForegroundColor Cyan
    dotnet nuget add source https://nuget.pkg.github.com/mpaulosky/index.json `
        --name github `
        --username $username `
        --password $tokenPlainText `
        --store-password-in-clear-text
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Credentials added to .NET NuGet config!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Testing connection..." -ForegroundColor Cyan
        
        # Test by listing sources
        dotnet nuget list source
        
        Write-Host ""
        Write-Host "? Setup complete! You can now restore packages from GitHub." -ForegroundColor Green
    } else {
        Write-Host "? Failed to add credentials to .NET NuGet config" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Restart Visual Studio if using environment variables" -ForegroundColor White
Write-Host "2. Try: dotnet restore" -ForegroundColor White
Write-Host "3. Try: dotnet add package YourPackageName --source github" -ForegroundColor White
