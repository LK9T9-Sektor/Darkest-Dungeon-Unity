# Build (when needed) and run the game without the Unity editor.
#
# Builds Build\Darkest Dungeon\Darkest Dungeon.exe when it is missing and then launches
# it. Pass through options are forwarded to the underlying scripts.
#
# Usage: pwsh tools\unity-dev-run.ps1 [-ProjectPath <project>] [-UnityEditorPath <path>] [-BuildDir <path>] [-AppId <uint>] [-SkipProvision]

param(
    [string]$ProjectPath = "",
    [string]$UnityEditorPath = "",
    [string]$BuildDir = "",
    [uint32]$AppId = 480,
    [switch]$SkipProvision
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) {
    $ProjectPath = "unity"
}
$projectRoot = Join-Path $repoRoot $ProjectPath

if (-not $BuildDir) {
    $BuildDir = Join-Path $projectRoot "Build\Darkest Dungeon"
}
$executablePath = Join-Path $BuildDir "Darkest Dungeon.exe"

if (-not (Test-Path $executablePath)) {
    Write-Host "==> Build not found, building first"
    $buildParams = @{}
    if ($ProjectPath) { $buildParams.ProjectPath = $ProjectPath }
    if ($UnityEditorPath) { $buildParams.UnityEditorPath = $UnityEditorPath }
    if ($BuildDir) { $buildParams.BuildDir = $BuildDir }
    $buildParams.AppId = $AppId
    if ($SkipProvision) { $buildParams.SkipProvision = $true }

    & (Join-Path $PSScriptRoot "unity-build-game.ps1") @buildParams
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed; the game was not launched."
    }
}

$runParams = @{}
if ($ProjectPath) { $runParams.ProjectPath = $ProjectPath }
if ($BuildDir) { $runParams.BuildDir = $BuildDir }
$runParams.AppId = $AppId
& (Join-Path $PSScriptRoot "unity-run-game.ps1") @runParams
