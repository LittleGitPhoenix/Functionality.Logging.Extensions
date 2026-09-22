#Requires -Version 7
<#
.SYNOPSIS
    Local CI runner for Logging.Extensions - thin wrapper around Automation.Workflows' shared Invoke-CI.ps1.
.DESCRIPTION
    Resolves the shared script via the PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH environment
    variable (must point to a local clone of https://github.com/LittleGitPhoenix/Automation.Workflows)
    and forwards this repository's root + ci-config.json to it.
.EXAMPLE
    $env:PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH = 'D:\GIT\Phoenix\Automation.Workflows'
    .\.github\scripts\Invoke-CI.ps1 -Mode Validate
#>
param(
    [ValidateSet('Validate', 'Release', 'All')]
    [string] $Mode,
    [switch] $ShowTags = $true
)

$ErrorActionPreference = 'Stop'

$toolsPath = $env:PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH
if (-not $toolsPath) {
    Write-Host "❌ Environment variable 'PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH' is not set." -ForegroundColor Red
    Write-Host "   Set it to a local clone of https://github.com/LittleGitPhoenix/Automation.Workflows, e.g.:" -ForegroundColor Red
    Write-Host '   $env:PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH = ''D:\GIT\Phoenix\Automation.Workflows''' -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $toolsPath)) {
    Write-Host "❌ Path referenced by 'PHOENIX_REUSABLE_WORKFLOWS_REPOSITORY_PATH' does not exist: $toolsPath" -ForegroundColor Red
    exit 1
}

$baseScript = Join-Path $toolsPath 'scripts/Invoke-CI.ps1'
if (-not (Test-Path $baseScript)) {
    Write-Host "❌ Could not find 'scripts/Invoke-CI.ps1' under '$toolsPath'. Is this a valid Automation.Workflows checkout?" -ForegroundColor Red
    exit 1
}

$repoRoot = [System.IO.Path]::GetFullPath("$PSScriptRoot/../..")

$params = @{ WorkspaceRoot = $repoRoot; ShowTags = $ShowTags }
if ($Mode) { $params['Mode'] = $Mode }

& $baseScript @params
exit $LASTEXITCODE
