# Run the standalone build without the Unity editor.
#
# Launches Build\Darkest Dungeon\Darkest Dungeon.exe with the executable folder as the
# working directory, so SteamAPI_Init picks up steam_appid.txt placed next to the game.
# When the executable is missing, prints a hint to build it first.
#
# Usage: pwsh tools\unity-run-game.ps1 [-ProjectPath <project>] [-BuildDir <path>] [-AppId <uint>]

param(
    [string]$ProjectPath = "",
    [string]$BuildDir = "",
    [uint32]$AppId = 480
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
    Write-Error "Build not found: $executablePath. Run tools\unity-build-game.ps1 (or tools\unity-dev-run.ps1) first."
    exit 1
}

$appIdFile = Join-Path $BuildDir "steam_appid.txt"
if (-not (Test-Path $appIdFile)) {
    Set-Content -Path $appIdFile -Value $AppId -NoNewline
    Write-Host "==> Created $appIdFile (AppID $AppId)"
}

Write-Host "==> Launching: $executablePath"
Start-Process -FilePath $executablePath -WorkingDirectory $BuildDir
