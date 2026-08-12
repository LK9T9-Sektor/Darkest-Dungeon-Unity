# Build (when needed) and run the game without the Unity editor.
#
# Builds Build\Darkest Dungeon\Darkest Dungeon.exe when it is missing and then launches
# it. Pass through options are forwarded to the underlying scripts.
#
# Usage: pwsh tools\dev-run.ps1 [-UnityEditorPath <path>] [-BuildDir <path>] [-AppId <uint>] [-SkipProvision]

param(
    [string]$UnityEditorPath = "",
    [string]$BuildDir = "",
    [uint32]$AppId = 480,
    [switch]$SkipProvision
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $BuildDir) {
    $BuildDir = Join-Path $repoRoot "Build\Darkest Dungeon"
}
$executablePath = Join-Path $BuildDir "Darkest Dungeon.exe"

if (-not (Test-Path $executablePath)) {
    Write-Host "==> Build not found, building first"
    $buildParams = @{}
    if ($UnityEditorPath) { $buildParams.UnityEditorPath = $UnityEditorPath }
    if ($BuildDir) { $buildParams.BuildDir = $BuildDir }
    $buildParams.AppId = $AppId
    if ($SkipProvision) { $buildParams.SkipProvision = $true }

    & (Join-Path $PSScriptRoot "build-game.ps1") @buildParams
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed; the game was not launched."
    }
}

$runParams = @{}
if ($BuildDir) { $runParams.BuildDir = $BuildDir }
$runParams.AppId = $AppId
& (Join-Path $PSScriptRoot "run-game.ps1") @runParams
