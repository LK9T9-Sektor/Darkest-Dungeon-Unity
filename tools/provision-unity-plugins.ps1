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
    [string]$UnityEditorPath = "",
    [uint32]$AppId = 480
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$internalDir = Join-Path $repoRoot "Assets\Plugins\Internal"
$x86_64Dir = Join-Path $repoRoot "Assets\Plugins\x86_64"
$steamDll = Join-Path $repoRoot "src\Lan\Sektor.DarkestDungeon.Lan.Steam\steam_api64.dll"
$appIdFile = Join-Path $repoRoot "steam_appid.txt"

function Find-UnityEditor {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path (Join-Path $Preferred "Editor\Unity.exe"))) {
        return $Preferred
    }
    if ($env:UNITY_EDITOR_PATH -and (Test-Path (Join-Path $env:UNITY_EDITOR_PATH "Editor\Unity.exe"))) {
        return $env:UNITY_EDITOR_PATH
    }

    $hubEditors = Join-Path $env:APPDATA "UnityHub\editors.json"
    if (Test-Path $hubEditors) {
        try {
            $hub = Get-Content $hubEditors -Raw | ConvertFrom-Json
            foreach ($entry in @($hub.editors)) {
                if ($entry.path -and (Test-Path (Join-Path $entry.path "Editor\Unity.exe"))) {
                    return $entry.path
                }
            }
        }
        catch { }
    }

    $knownRoots = @(
        "E:\ProgramFiles\Unity2017.4.40f1",
        "D:\Program Files\Unity2017.4.40f1",
        "C:\Program Files\Unity\Hub\Editor\2017.4.40f1"
    )
    foreach ($root in $knownRoots) {
        if (Test-Path (Join-Path $root "Editor\Unity.exe")) {
            return $root
        }
    }

    $scanRoots = @(
        "C:\Program Files\Unity",
        "C:\Program Files\Unity*",
        "D:\Program Files\Unity*",
        "E:\ProgramFiles\Unity*"
    )
    foreach ($scanRoot in $scanRoots) {
        $found = Get-ChildItem -Path $scanRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { Test-Path (Join-Path $_.FullName "Editor\Unity.exe") } |
            Select-Object -First 1
        if ($found) {
            return $found.FullName
        }
    }

    return ""
}

if (-not $UnityEditorPath) {
    $UnityEditorPath = Find-UnityEditor
}
if (-not $UnityEditorPath) {
    throw "Unity editor not found. Pass -UnityEditorPath <editor root> (folder containing Editor\Unity.exe)."
}
Write-Host "==> Unity editor: $UnityEditorPath"

$facadesSource = Join-Path $UnityEditorPath "Editor\Data\MonoBleedingEdge\lib\mono\unityjit\Facades"

Write-Host "==> Building Lan solution"
dotnet build (Join-Path $repoRoot "Sektor.DarkestDungeon.slnx") --nologo -v q -p:AllowMissingPrunePackageData=true
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "==> Copying .NET Standard facades"
if (-not (Test-Path $facadesSource)) {
    Write-Warning "Facades folder not found: $facadesSource. Skipping facade delivery."
} else {
    New-Item -ItemType Directory -Force -Path $internalDir | Out-Null
    Copy-Item -Path (Join-Path $facadesSource "*.dll") -Destination $internalDir -Force
}

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
