# Build and run the WPF desktop client from the repository root.
#
# Usage:
#   pwsh .\run-wpf.ps1
#   pwsh .\run-wpf.ps1 -Configuration Release
#   pwsh .\run-wpf.ps1 -NoBuild
#   pwsh .\run-wpf.ps1 -DryRun
#
# Requires the .NET 8 SDK (`dotnet` on PATH). Paths are resolved from $PSScriptRoot,
# so the script can be launched from any working directory.

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [switch]$NoBuild,

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\Wpf\Sektor.DarkestDungeon.Wpf\Sektor.DarkestDungeon.Wpf.csproj"

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "WPF project not found: $projectPath"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK not found. Install the .NET 8 SDK and reopen the terminal."
}

$dotnetArgs = @(
    "run",
    "--project", $projectPath,
    "--configuration", $Configuration
)
if ($NoBuild) {
    $dotnetArgs += "--no-build"
}

Write-Host ("==> dotnet " + ($dotnetArgs -join " "))
if ($DryRun) {
    return
}

& dotnet @dotnetArgs
exit $LASTEXITCODE
