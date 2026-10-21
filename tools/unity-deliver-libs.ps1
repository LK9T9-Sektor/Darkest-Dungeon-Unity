# Build every managed library that ships to Unity. Each project's PostBuild target copies
# its DLL/PDB into unity\ and unity-2017\ Assets\Plugins\Internal (gitignored).
#
# The Unity editor must be closed while the build runs: Windows locks loaded DLLs.
#
# Usage: pwsh tools\unity-deliver-libs.ps1 [-Configuration Debug|Release]

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $repoRoot "src"
$unityRoots = @(
    (Join-Path $repoRoot "unity"),
    (Join-Path $repoRoot "unity-2017")
)
$internalRelativePath = "Assets\Plugins\Internal"
$started = Get-Date

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet was not found in PATH."
}

function Get-DeliveringProjects {
    Get-ChildItem -Path $srcRoot -Filter "*.csproj" -File -Recurse |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
        Where-Object { Select-String -LiteralPath $_.FullName -Pattern "CoreUnityPluginFolders" -Quiet } |
        Sort-Object FullName
}

function Assert-UnityClosed {
    foreach ($unityRoot in $unityRoots) {
        if (-not (Test-Path -LiteralPath $unityRoot)) {
            throw "Unity project not found: $unityRoot"
        }

        $lockPath = Join-Path $unityRoot "Temp\UnityLockfile"
        if (-not (Test-Path -LiteralPath $lockPath)) {
            continue
        }

        $running = Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
            Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($unityRoot, [StringComparison]::OrdinalIgnoreCase) -ge 0 }
        if ($running) {
            throw "Unity is open on '$unityRoot'. Close the editor, then rerun the delivery."
        }

        Write-Host "==> Ignoring stale Unity lock: $lockPath"
    }
}

function Get-BuildArtifact {
    param(
        [string]$ProjectDirectory,
        [string]$FileName
    )

    $binRoot = Join-Path $ProjectDirectory "bin\$Configuration"
    if (-not (Test-Path -LiteralPath $binRoot)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $binRoot -Filter $FileName -File -Recurse |
        Where-Object { $_.DirectoryName -notmatch '\\(ref|refint)(\\|$)' } |
        Select-Object -First 1
}

function Assert-Delivered {
    param(
        [System.IO.FileInfo]$Project
    )

    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($Project.Name)
    $dll = Get-BuildArtifact -ProjectDirectory $Project.DirectoryName -FileName "$assemblyName.dll"
    $pdb = Get-BuildArtifact -ProjectDirectory $Project.DirectoryName -FileName "$assemblyName.pdb"
    if (-not $dll -or -not $pdb) {
        throw "Build artifacts not found for $assemblyName ($Configuration)."
    }

    foreach ($unityRoot in $unityRoots) {
        $destinationDirectory = Join-Path $unityRoot $internalRelativePath
        foreach ($artifact in @($dll, $pdb)) {
            $destination = Join-Path $destinationDirectory $artifact.Name
            if (-not (Test-Path -LiteralPath $destination)) {
                throw "Not delivered: $destination"
            }

            $sourceHash = (Get-FileHash -LiteralPath $artifact.FullName -Algorithm SHA256).Hash
            $destinationHash = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
            if ($sourceHash -ne $destinationHash) {
                throw "Stale delivery: $destination"
            }
        }
    }
}

$projects = @(Get-DeliveringProjects)
if ($projects.Count -eq 0) {
    throw "No delivering projects found under $srcRoot."
}

Assert-UnityClosed
foreach ($unityRoot in $unityRoots) {
    New-Item -ItemType Directory -Force -Path (Join-Path $unityRoot $internalRelativePath) | Out-Null
}

Write-Host "==> Delivering $($projects.Count) libraries ($Configuration) to unity and unity-2017"
$failed = @()
for ($i = 0; $i -lt $projects.Count; $i++) {
    $project = $projects[$i]
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
    Write-Host ("  [{0}/{1}] {2}" -f ($i + 1), $projects.Count, $assemblyName)

    $output = & dotnet build $project.FullName --configuration $Configuration --nologo -v:q 2>&1
    if ($LASTEXITCODE -ne 0) {
        $failed += $assemblyName
        Write-Host ($output -join [Environment]::NewLine)
    }
}

if ($failed.Count -gt 0) {
    throw "Build failed: $($failed -join ', ')."
}

foreach ($project in $projects) {
    Assert-Delivered -Project $project
}

$elapsed = (Get-Date) - $started
Write-Host "==> Verified $($projects.Count) DLL/PDB pairs in both Unity trees in $([math]::Round($elapsed.TotalSeconds, 1))s."
Write-Host "Open Unity (or press Ctrl+R there) to reimport the delivered assemblies."
