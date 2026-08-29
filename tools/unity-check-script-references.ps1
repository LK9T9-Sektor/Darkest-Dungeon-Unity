# Verifies that every MonoBehaviour script reference in Unity scenes/prefabs resolves to a
# committed .meta GUID. Catches the "stale GUID" failure mode where Unity regenerated script
# metas (new GUIDs) while scenes kept the old GUIDs, which silently drops components at load
# time (black screens, NullReferenceExceptions).
#
# 1. Indexes every guid found under Assets\**\*.meta.
# 2. Scans all .unity/.prefab files for m_Script: {fileID: 11500000, guid: X} references.
# 3. Reports any guid that resolves neither to a project meta nor to a known built-in/package
#    guid (old Unity 2017 UI components, the com.unity.ugui package, Photon demo scenes).
# 4. Flags .cs files that are missing their .cs.meta (Unity would regenerate a new guid).
# Exit code 0 when clean, 1 otherwise.
#
# Uses ripgrep when available (fast parallel file scan); falls back to a plain PowerShell
# implementation otherwise. Both paths produce the same result and error report.
#
# Usage: pwsh tools\unity-check-script-references.ps1 [-ProjectPath <project>]

param(
    [string]$ProjectPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) {
    $ProjectPath = "unity"
}
$projectRoot = Join-Path $repoRoot $ProjectPath
$assetsRoot = Join-Path $projectRoot "Assets"

if (-not (Test-Path $assetsRoot)) {
    throw "Project Assets folder not found: $assetsRoot"
}

# Built-in / package script GUIDs that are legitimately not backed by an Assets .meta:
# - Old Unity 2017 UnityEngine.UI component scripts (Image/Text/Button/LayoutElement/LayoutGroup)
#   referenced by legacy prefabs; Unity resolves them for backward compatibility.
# - The com.unity.ugui package scripts (same UI assembly, newer references).
# - Photon demo scenes.
$builtinGuids = @(
    "fe87c0e1cc204ed48ad3b37840f39efc", # 2017 UnityEngine.UI.Image
    "5f7201a12d95ffc409449d95f23cf332", # 2017 UnityEngine.UI.Text
    "306cc8c2b49d7114eaa3623786fc2126", # 2017 UnityEngine.UI.LayoutElement
    "30649d3a9faa99c48a7b1166b86bf2a0", # 2017 UnityEngine.UI Horizontal/VerticalLayoutGroup
    "67db9e8f0e2ae9c40bc1e2b64352a6b4", # 2017 UnityEngine.UI.Button
    "beaae63f7865d7c41b3aced4e96e790d", # Photon DemoRockPaperScissors
    "a9b53cc9db43d39428412f981834d9c1", # Photon MarcoPolo demo
    "f5f67c52d1564df4a8936ccd202a3bd8"  # com.unity.ugui package scripts
)
$builtinSet = @{}
foreach ($guid in $builtinGuids) { $builtinSet[$guid] = $true }

$errors = New-Object System.Collections.Generic.List[string]

$rg = Get-Command rg -ErrorAction SilentlyContinue
if ($null -ne $rg) {
    # --- Fast path: ripgrep scans the tree in parallel. ---

    # 1. Index of every guid declared in Assets\**\*.meta (output is the hex guid only).
    $projectGuids = @{}
    & $rg.Source -o --no-filename --replace '$1' '^guid: ([0-9a-f]+)' $assetsRoot -g '*.meta' 2>$null |
        ForEach-Object { $projectGuids[$_] = $true }

    # 2. Every m_Script reference in scenes/prefabs. rg prints "<path>:<guid>"; the guid is the
    #    trailing 32 hex chars, so splitting by length is robust against colons in Windows paths.
    & $rg.Source -o --no-heading --replace '$1' 'm_Script: \{fileID: 11500000, guid: ([0-9a-f]+)' $assetsRoot -g '*.unity' -g '*.prefab' 2>$null |
        ForEach-Object {
            $guid = $_.Substring($_.Length - 32)
            $path = $_.Substring(0, $_.Length - 33)
            if (-not $projectGuids.ContainsKey($guid) -and -not $builtinSet.ContainsKey($guid)) {
                $rel = $path.Substring($repoRoot.Length + 1)
                $errors.Add("Unresolved script guid $guid referenced in $rel")
            }
        }

    # 3. .cs scripts missing their .cs.meta (Unity would regenerate a new guid).
    & $rg.Source --files $assetsRoot -g '*.cs' 2>$null |
        ForEach-Object {
            if (-not (Test-Path "$_.meta")) {
                $errors.Add("Missing .meta for script " + $_.Substring($repoRoot.Length + 1) +
                    " (Unity would regenerate a new guid and break scene references)")
            }
        }
}
else {
    # --- Fallback: plain PowerShell scan (used when ripgrep is not installed). ---

    $projectGuids = @{}
    Get-ChildItem $assetsRoot -Recurse -Filter "*.meta" -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notmatch "\.meta\.meta$" } |
        ForEach-Object {
            $m = [regex]::Match([System.IO.File]::ReadAllText($_.FullName), "(?m)^guid: ([0-9a-f]+)")
            if ($m.Success) {
                $projectGuids[$m.Groups[1].Value] = $_.FullName
            }
        }

    Get-ChildItem $assetsRoot -Recurse -Include "*.unity", "*.prefab" -ErrorAction SilentlyContinue |
        ForEach-Object {
            $rel = $_.FullName.Substring($repoRoot.Length + 1)
            $text = [System.IO.File]::ReadAllText($_.FullName)
            foreach ($m in [regex]::Matches($text, 'm_Script: \{fileID: 11500000, guid: ([0-9a-f]+)')) {
                $guid = $m.Groups[1].Value
                if (-not $projectGuids.ContainsKey($guid) -and -not $builtinSet.ContainsKey($guid)) {
                    $errors.Add("Unresolved script guid $guid referenced in $rel")
                }
            }
        }

    Get-ChildItem $assetsRoot -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue |
        ForEach-Object {
            $meta = $_.FullName + ".meta"
            if (-not (Test-Path $meta)) {
                $errors.Add("Missing .meta for script " + $_.FullName.Substring($repoRoot.Length + 1) +
                    " (Unity would regenerate a new guid and break scene references)")
            }
        }
}

if ($errors.Count -gt 0) {
    Write-Host "==> Script reference check FAILED for ${ProjectPath}:"
    $errors | ForEach-Object { Write-Host "  $_" }
    exit 1
}

Write-Host "==> Script reference check passed for $ProjectPath."
exit 0