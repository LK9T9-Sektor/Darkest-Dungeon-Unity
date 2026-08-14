# Provision Unity plugin delivery for the Lan transport and the core Content module.
#
# 1. Builds the Lan transport (dotnet, Steam project builds Contracts transitively);
#    its post-build target copies the managed assemblies into each Unity project's
#    Assets\Plugins\Internal\ (gitignored) for unity\ and unity-2017\.
# 2. Builds the core Content module; its post-build target copies the DLL/PDB into
#    the same Assets\Plugins\Internal\ folders (unity\ and unity-2017\).
# 3. Copies the .NET Standard facade shims from the installed Unity editor into the
#    target project's Assets\Plugins\Internal\ so the old Mono runtime resolves the
#    BCL types referenced by the netstandard2.0 assemblies (see src\docs\COMPABILITY.md).
#    Required only for Unity 2017.4; Unity 6+ resolves those types natively and
#    the MonoBleedingEdge unityjit\Facades folder no longer exists, so it is skipped.
# 4. Copies steam_api64.dll into the target project's Assets\Plugins\x86_64\ as a native plugin.
# 5. Ensures a local (gitignored) steam_appid.txt exists for editor/dev runs.
#
# Usage: pwsh tools\provision-unity-plugins.ps1 [-ProjectPath <project>] [-UnityEditorPath <path>] [-AppId <uint>]

param(
    [string]$ProjectPath = "",
    [string]$UnityEditorPath = "",
    [uint32]$AppId = 480
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) {
    $ProjectPath = "unity"
}
$projectRoot = Join-Path $repoRoot $ProjectPath
$internalDir = Join-Path $projectRoot "Assets\Plugins\Internal"
$x86_64Dir = Join-Path $projectRoot "Assets\Plugins\x86_64"
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
        "C:\Program Files\Unity\Hub\Editor\6000.5.8f1",
        "D:\ProgramFiles\Unity2017.4.40f1",
        "D:\Program Files\Unity2017.4.40f1",
        "E:\ProgramFiles\Unity2017.4.40f1",
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

Write-Host "==> Building Lan transport (Steam project builds Contracts transitively)"
dotnet build (Join-Path $repoRoot "src\Lan\Sektor.DarkestDungeon.Lan.Steam\Sektor.DarkestDungeon.Lan.Steam.csproj") --nologo -v q -p:AllowMissingPrunePackageData=true
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "==> Building core Content module"
dotnet build (Join-Path $repoRoot "src\Core\Sektor.DarkestDungeon.Core.Content\Sektor.DarkestDungeon.Core.Content.csproj") --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "==> Copying .NET Standard facades"
if (-not (Test-Path $facadesSource)) {
    Write-Warning "Facades folder not found: $facadesSource. Not required for Unity 6+ (native type forwarding); skipping facade delivery."
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
