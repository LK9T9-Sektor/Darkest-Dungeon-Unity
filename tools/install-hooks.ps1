# Installs the repository git hooks (committed under .githooks\) by pointing git at that folder.
# The pre-commit hook runs tools\unity-check-script-references.ps1 on both Unity trees so stale script
# GUIDs in scenes/prefabs are rejected before they are committed.
#
# Usage: pwsh tools\install-hooks.ps1

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

$configured = git config core.hooksPath
if ($configured -eq ".githooks") {
    Write-Host "==> Git hooks already installed (.githooks)."
}
else {
    git config core.hooksPath .githooks
    Write-Host "==> Git hooks path set to .githooks."
}

Write-Host "==> Hooks present:"
Get-ChildItem (Join-Path $repoRoot ".githooks") -File | ForEach-Object { Write-Host "  $($_.Name)" }
