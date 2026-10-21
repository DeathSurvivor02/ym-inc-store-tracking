<#
.SYNOPSIS
    Automates creating and pushing version iteration tags to GitHub.

.DESCRIPTION
    Updates csproj version metadata, commits changes, creates an annotated git tag,
    and pushes to GitHub so the automated Release CI workflow packages the release.

.PARAMETER Version
    The semantic version string, e.g. "0.1.1-alpha", "0.2.0".

.PARAMETER Message
    Release summary or changelog summary for the tag message.

.PARAMETER Push
    Switch to immediately push the commit and tag to origin.

.EXAMPLE
    .\scripts\tag_version.ps1 -Version "0.1.1-alpha" -Message "Fix: layout adjustments and db pooling" -Push
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
    $propGroup = $xml.Project.PropertyGroup
    if ($propGroup -is [System.Array]) {
        $propGroup = $propGroup[0]
    }

    $cleanNumericVersion = ($rawVersion -split "-")[0]
    $segments = $cleanNumericVersion.Split('.')
    while ($segments.Count -lt 4) {
        $cleanNumericVersion += ".0"
        $segments = $cleanNumericVersion.Split('.')
    }

    $propGroup.Version = $rawVersion
    $propGroup.AssemblyVersion = $cleanNumericVersion
    $propGroup.FileVersion = $cleanNumericVersion
    $propGroup.InformationalVersion = $rawVersion

    $xml.Save($csprojPath)
    Write-Host "Saved updated csproj." -ForegroundColor Green
}

# 2. Update CHANGELOG if needed & commit csproj
git add "$csprojPath"
$hasStaged = git diff --cached --name-only
if ($hasStaged) {
    git commit -m "chore(release): bump version to $rawVersion"
}

# 3. Create git tag
Write-Host "==> Creating git tag: $tag" -ForegroundColor Cyan
git tag -a $tag -m "$Message" -f

if ($LASTEXITCODE -eq 0) {
    Write-Host "Successfully created local tag: $tag" -ForegroundColor Green
} else {
    Write-Error "Failed to create tag $tag."
    exit $LASTEXITCODE
}

# 4. Push if requested
if ($Push) {
    Write-Host "==> Pushing changes and tag $tag to origin..." -ForegroundColor Cyan
    git push origin HEAD
    git push origin $tag -f
    Write-Host "Successfully pushed tag $tag to origin! GitHub Actions workflow will trigger." -ForegroundColor Green
} else {
    Write-Host "To push to GitHub, run:" -ForegroundColor Yellow
    Write-Host "  git push origin HEAD"
    Write-Host "  git push origin $tag -f"
}
