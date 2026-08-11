# Provision Unity plugin delivery for the Lan Steam transport.
#
# 1. Builds the Lan solution (dotnet), whose post-build targets copy the managed
#    assemblies into Assets\Plugins\Internal\ (gitignored).
# 2. Copies the .NET Standard facade shims from the installed Unity editor into
#    Assets\Plugins\Internal\ so the old Mono runtime resolves the BCL types
#    referenced by the netstandard2.0 assemblies (see src\docs\COMPABILITY.md).
# 3. Copies steam_api64.dll into Assets\Plugins\x86_64\ as a native plugin.
# 4. Ensures a local (gitignored) steam_appid.txt exists for editor/dev runs.
#
# Usage: pwsh tools\provision-unity-plugins.ps1 [-UnityEditorPath <path>] [-AppId <uint>]

param(
    [string]$UnityEditorPath = "D:\Program Files\Unity2017.4.40f1",
    [uint32]$AppId = 480
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$facadesSource = Join-Path $UnityEditorPath "Editor\Data\MonoBleedingEdge\lib\mono\unityjit\Facades"
$internalDir = Join-Path $repoRoot "Assets\Plugins\Internal"
$x86_64Dir = Join-Path $repoRoot "Assets\Plugins\x86_64"
$steamDll = Join-Path $repoRoot "src\Lan\Sektor.DarkestDungeon.Lan.Steam\steam_api64.dll"
$appIdFile = Join-Path $repoRoot "steam_appid.txt"

Write-Host "==> Building Lan solution"
dotnet build (Join-Path $repoRoot "Sektor.DarkestDungeon.slnx") --nologo -v q -p:AllowMissingPrunePackageData=true
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "==> Copying .NET Standard facades"
if (-not (Test-Path $facadesSource)) { throw "Facades folder not found: $facadesSource" }
New-Item -ItemType Directory -Force -Path $internalDir | Out-Null
Copy-Item -Path (Join-Path $facadesSource "*.dll") -Destination $internalDir -Force

Write-Host "==> Copying steam_api64.dll native plugin"
New-Item -ItemType Directory -Force -Path $x86_64Dir | Out-Null
Copy-Item -Path $steamDll -Destination (Join-Path $x86_64Dir "steam_api64.dll") -Force

Write-Host "==> Ensuring local steam_appid.txt"
if (-not (Test-Path $appIdFile)) {
    Set-Content -Path $appIdFile -Value $AppId -NoNewline
    Write-Host "Created $appIdFile with dev AppID $AppId"
}

$steamSourceAppIdFile = Join-Path $repoRoot "src\Lan\Sektor.DarkestDungeon.Lan.Steam\steam_appid.txt"
if (-not (Test-Path $steamSourceAppIdFile)) {
    Set-Content -Path $steamSourceAppIdFile -Value $AppId -NoNewline
    Write-Host "Created $steamSourceAppIdFile with dev AppID $AppId"
}

Write-Host "Done."
