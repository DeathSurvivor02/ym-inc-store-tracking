<#
.SYNOPSIS
    Automates creating and pushing version iteration tags to GitHub.

.DESCRIPTION
    Updates csproj version metadata (optional), commits changes, creates an annotated git tag,
    and pushes to GitHub so the automated Release CI workflow packages the release.

.PARAMETER Version
    The semantic version string, e.g. "0.1.0-alpha", "0.1.0", "0.2.0".

.PARAMETER Message
    Release summary or changelog summary for the tag message.

.PARAMETER Push
    Switch to immediately push the commit and tag to origin.

.EXAMPLE
    .\scripts\tag_version.ps1 -Version "0.1.0-alpha" -Message "Auth system, user management, and security overhaul" -Push
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [string]$Message = "Release $Version",

    [switch]$Push
)

$tag = if ($Version.StartsWith("v")) { $Version } else { "v$Version" }
$rawVersion = $Version.TrimStart("v")

Write-Host "==> Preparing version iteration: $tag (Raw: $rawVersion)" -ForegroundColor Cyan

# 1. Update csproj Version
$csprojPath = Join-Path $PSScriptRoot "..\desktop-app\YMInc.StoreDeploymentApp.csproj"
if (Test-Path $csprojPath) {
    Write-Host "Updating $csprojPath with version $rawVersion..." -ForegroundColor Yellow
    [xml]$xml = Get-Content $csprojPath
    $propGroup = $xml.Project.PropertyGroup[0]

    # Ensure elements exist
    $cleanNumericVersion = ($rawVersion -split "-")[0]
    while (($cleanNumericVersion.Split('.').Count) -lt 4) {
        $cleanNumericVersion += ".0"
    }

    if (-not $propGroup.Version) { $e = $xml.CreateElement("Version"); [void]$propGroup.AppendChild($e) }
    $propGroup.Version = $rawVersion

    if (-not $propGroup.AssemblyVersion) { $e = $xml.CreateElement("AssemblyVersion"); [void]$propGroup.AppendChild($e) }
    $propGroup.AssemblyVersion = $cleanNumericVersion

    if (-not $propGroup.FileVersion) { $e = $xml.CreateElement("FileVersion"); [void]$propGroup.AppendChild($e) }
    $propGroup.FileVersion = $cleanNumericVersion

    if (-not $propGroup.InformationalVersion) { $e = $xml.CreateElement("InformationalVersion"); [void]$propGroup.AppendChild($e) }
    $propGroup.InformationalVersion = $rawVersion

    $xml.Save($csprojPath)
    Write-Host "Saved updated csproj." -ForegroundColor Green
}

# 2. Check Git status
Write-Host "==> Creating git tag: $tag" -ForegroundColor Cyan
git tag -a $tag -m "$Message"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Successfully created local tag: $tag" -ForegroundColor Green
} else {
    Write-Error "Failed to create tag $tag."
    exit $LASTEXITCODE
}

# 3. Push if requested
if ($Push) {
    Write-Host "==> Pushing changes and tag $tag to origin..." -ForegroundColor Cyan
    git push origin HEAD
    git push origin $tag
    Write-Host "Successfully pushed tag $tag to origin! GitHub Actions workflow will trigger." -ForegroundColor Green
} else {
    Write-Host "To push to GitHub, run:" -ForegroundColor Yellow
    Write-Host "  git push origin HEAD"
    Write-Host "  git push origin $tag"
}
