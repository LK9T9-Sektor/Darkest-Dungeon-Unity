param(
    [string[]]$Roots = @("$PSScriptRoot\..\src", "$PSScriptRoot\..\tests")
)

# Lint: owned C# (src\, tests\ except src\External\ and generated obj/bin) must place ALL using
# directives at the top of the file, before the namespace declaration. Indented `using` lines
# inside a namespace body violate the convention (StyleCop SA1200) and fail the check.
# `using (...) {...}` statements and `using var` declarations (resource disposal inside method
# bodies) are not directives and are ignored.
#
# Usage:
#   pwsh tools\check-using-placement.ps1            # scan the default owned roots
#   pwsh tools\check-using-placement.ps1 -Roots x,y # scan arbitrary roots (negative tests)
# Exit code 0 when clean; 1 and a file list when violations are found.

$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$violations = @()

foreach ($root in $Roots) {
    if (-not (Test-Path -LiteralPath $root)) {
        continue
    }

    Get-ChildItem -LiteralPath $root -Recurse -Filter "*.cs" | ForEach-Object {
        $path = $_.FullName
        if ($path -match "\\(obj|bin|External)\\" ) {
            return
        }

        $lines = [IO.File]::ReadAllLines($path)
        $indentedUsing = $lines | Where-Object {
            $_ -match "^[ \t]+using " -and $_ -notmatch "^[ \t]+using (\(|var )"
        } | Select-Object -First 1
        if ($indentedUsing) {
            $violations += $path.Substring($repo.Length + 1)
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Output "Using-directive placement violation(s) in $($violations.Count) file(s):"
    $violations | Sort-Object | ForEach-Object { Write-Output "  $_" }
    exit 1
}

Write-Output "OK: all owned C# files keep using directives above the namespace declaration."
exit 0