param(
    [string]$ManifestPath = "$PSScriptRoot\..\docs\EXTRACTION_STATUS.md"
)

# Verifies that the paths referenced in docs/EXTRACTION_STATUS.md exist.
# Parses the markdown tables ("| unity | core | status |"), strips notes and
# secondary paths, and checks the primary paths against the filesystem.
# Exit code 0 when no expected path is missing.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if (-not (Test-Path -LiteralPath $ManifestPath)) {
    throw "Manifest not found: $ManifestPath"
}

function Get-CleanPath([string]$cell) {
    # Drop parenthesized notes, secondary paths and quotes; keep the primary path.
    $paren = $cell.IndexOf(" (")
    if ($paren -ge 0) { $cell = $cell.Substring(0, $paren) }
    $primary = ($cell -split '[,+]')[0]
    return $primary.Trim().Trim('`', ' ')
}

$checked = 0
$missing = New-Object System.Collections.Generic.List[string]
$missingPartial = New-Object System.Collections.Generic.List[string]

foreach ($line in Get-Content -LiteralPath $ManifestPath) {
    if (-not ($line -match '^\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*(.+?)\s*\|$')) { continue }

    $unityCell = $Matches[1].Trim()
    $coreCell = $Matches[2].Trim()
    $statusCell = $Matches[3].Trim()

    if ($unityCell -eq "Unity (legacy)" -or $unityCell -eq "Unity") { continue }
    if ($unityCell -match '^-+$' -or $coreCell -match '^-+$') { continue }

    $unityPath = Get-CleanPath $unityCell
    $corePath = Get-CleanPath $coreCell
    if ($unityPath.Length -eq 0 -and $corePath.Length -eq 0) { continue }

    $status = if ($statusCell -match '^вынесено\b') { "вынесено" }
             elseif ($statusCell -match '^частично\b') { "частично" }
             elseif ($statusCell -match '^не вынесено\b') { "не вынесено" }
             else { $null }

    if ($null -eq $status) { continue }

    $checked++

    $unityExists = $unityPath.Length -gt 0 -and (Test-Path -LiteralPath (Join-Path $root $unityPath))
    if (-not $unityExists) {
        $missing.Add("Unity source not found: $unityPath [$status]")
    }

    if ($status -eq "вынесено") {
        if ($corePath.Length -eq 0 -or $corePath -eq "—") {
            $missing.Add("Extracted row without a core twin: $unityPath")
        }
        elseif (-not (Test-Path -LiteralPath (Join-Path $root $corePath))) {
            $missing.Add("Core twin not found: $corePath [<- $unityPath]")
        }
    }
    elseif ($status -eq "частично" -and $corePath.Length -gt 0 -and $corePath -ne "—") {
        if (-not (Test-Path -LiteralPath (Join-Path $root $corePath))) {
            $missingPartial.Add("Core twin not found (partial): $corePath [<- $unityPath]")
        }
    }
}

Write-Host "==> EXTRACTION_STATUS check: $checked rows in $ManifestPath"
foreach ($item in $missing) { Write-Host "    FAIL: $item" }
foreach ($item in $missingPartial) { Write-Host "    WARN: $item" }

if ($missing.Count -eq 0) {
    Write-Host "==> OK: all expected paths exist."
    exit 0
}

Write-Host "==> $($missing.Count) expected path(s) missing."
exit 1