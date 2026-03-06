param (
    [Parameter(Mandatory = $true)]
    [string]$NewVersion
)

# Function to update the version in the Package.wxs file
function Update-VersionInPackageWxs {
    param (
        [string]$FilePath,
        [string]$Version
    )

    Write-Host "Updating version in Package.wxs file: $FilePath" -ForegroundColor Green

    # Auto-expand version to X.Y.0 if it's in the format X.Y
    if ($Version -match '^\d+\.\d+$') {
        $Version = "$Version.0"
    }

    # Load the file content
    $content = Get-Content -Path $FilePath

    # Update the Version attribute in the Package node
    $updatedContent = $content -replace '(?<=<Package[^>]*\sVersion=")[^"]*', "$Version"

    # Save the updated content back to the file
    Set-Content -Path $FilePath -Value $updatedContent
}

# Find the Package.wxs file
$packageWxsFile = Get-ChildItem -Path . -Recurse -Include Package.wxs

# Update version in the Package.wxs file if it exists
if ($packageWxsFile) {
    Update-VersionInPackageWxs -FilePath $packageWxsFile.FullName -Version $NewVersion
} else {
    Write-Host "Package.wxs file not found." -ForegroundColor Yellow
}

Write-Host "Version update completed for setup file." -ForegroundColor Cyan