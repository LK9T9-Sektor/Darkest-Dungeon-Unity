# Rewrites owned prefabs and materials into the Unity 7 serialization format without the GUI.
#
# Opening a legacy 2017-format asset in the current editor rewrites it on save, which shows up
# as recurring noise in every unrelated commit. This script does that rewrite once, for every
# own prefab and material, through AssetDatabase.ForceReserializeAssets.
#
# 1. Locates the Unity editor (known roots / UNITY_EDITOR_PATH / -UnityEditorPath) and checks
#    that it matches ProjectSettings\ProjectVersion.txt.
# 2. Aborts when the project is open in the editor (Temp\UnityLockfile).
# 3. Runs the editor in batch mode with -executeMethod BatchReserialize.Run.
# 4. Fails when the log contains errors or the [RESERIALIZE] Done marker is missing.
# 5. Reports how many prefabs/materials git sees as modified, then runs
#    tools\unity-check-script-references.ps1 to prove script GUIDs still resolve.
#
# Vendor folders (Plugins, Photon Unity Networking, PhotonChatApi, Spine,
# MobileDependencyResolver) are skipped by the editor method.
#
# Usage: pwsh tools\unity-reserialize.ps1 [-ProjectPath <project>] [-UnityEditorPath <path>]

param(
    [string]$ProjectPath = "",
    [string]$UnityEditorPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) {
    $ProjectPath = "unity"
}
$projectRoot = Join-Path $repoRoot $ProjectPath

function Find-UnityEditor {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path (Join-Path $Preferred "Editor\Unity.exe"))) {
        return $Preferred
    }
    if ($env:UNITY_EDITOR_PATH -and (Test-Path (Join-Path $env:UNITY_EDITOR_PATH "Editor\Unity.exe"))) {
        return $env:UNITY_EDITOR_PATH
    }

    $knownRoots = @(
        "C:\Program Files\Unity\Hub\Editor\7000.0.0a10"
    )
    foreach ($root in $knownRoots) {
        if (Test-Path (Join-Path $root "Editor\Unity.exe")) {
            return $root
        }
    }

    $scanRoots = @(
        "C:\Program Files\Unity\Hub\Editor",
        "C:\Program Files\Unity Hub\Editor"
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

function Find-EditorByVersion {
    param([string]$Wanted)

    $roots = @(
        "C:\Program Files\Unity\Hub\Editor",
        "C:\Program Files\Unity Hub\Editor",
        "D:\ProgramFiles",
        "D:\Program Files"
    )
    foreach ($root in $roots) {
        if (-not (Test-Path $root)) { continue }
        $candidates = @($root)
        if (-not (Test-Path (Join-Path $root "Editor\Unity.exe"))) {
            $candidates = Get-ChildItem $root -Directory -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
        }
        foreach ($candidate in $candidates) {
            $exe = Join-Path $candidate "Editor\Unity.exe"
            if (Test-Path $exe) {
                $version = (Get-Item $exe).VersionInfo.ProductVersion
                if ($version -and $version.StartsWith($Wanted)) { return $candidate }
            }
        }
    }

    return ""
}

# Blocks when an editor is actually open on this project; a leftover Temp\UnityLockfile
# from a crashed or failed batch run (no matching Unity process) is removed instead.
function Assert-ProjectNotLocked {
    $lockPath = Join-Path $projectRoot "Temp\UnityLockfile"
    if (-not (Test-Path $lockPath)) {
        return
    }

    $running = Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine.Contains($projectRoot) }
    if ($running) {
        throw "The project is open in the Unity editor. Close it first, then rerun the reserialize."
    }

    Write-Host "==> Removing stale Unity lock file ($lockPath)"
    Remove-Item $lockPath -Force
}

function Test-ReserializeResult {
    param([string]$LogPath)

    if (-not (Test-Path $LogPath)) {
        return $true
    }

    $logText = Get-Content $LogPath -Raw
    $hasErrorMarker = $logText -match "\[RESERIALIZE\] Failed|error CS\d+|Compilation failed|Scripts have compiler errors"
    $hasDoneMarker = $logText -match "\[RESERIALIZE\] Done:"
    return ($hasErrorMarker -or -not $hasDoneMarker)
}

if (-not $UnityEditorPath) {
    $UnityEditorPath = Find-UnityEditor
}
if (-not $UnityEditorPath) {
    throw "Unity editor not found. Pass -UnityEditorPath <editor root> (folder containing Editor\Unity.exe)."
}
$unityExe = Join-Path $UnityEditorPath "Editor\Unity.exe"

$projectVersionFile = Join-Path $projectRoot "ProjectSettings\ProjectVersion.txt"
if (Test-Path $projectVersionFile) {
    $wanted = ([regex]::Match((Get-Content $projectVersionFile -Raw), "m_EditorVersion:\s*(\S+)")).Groups[1].Value
    $actual = (Get-Item $unityExe).VersionInfo.ProductVersion
    if ($wanted -and $actual -and -not $actual.StartsWith($wanted)) {
        $match = Find-EditorByVersion -Wanted $wanted
        if ($match) {
            $UnityEditorPath = $match
            $unityExe = Join-Path $UnityEditorPath "Editor\Unity.exe"
            Write-Host "==> Editor version switched to match project: $wanted"
        } else {
            throw "Unity editor $wanted required by '$ProjectPath' is not installed (auto-found $actual at $UnityEditorPath). Install it or pass -UnityEditorPath."
        }
    }
}
Write-Host "==> Unity editor: $UnityEditorPath"

Assert-ProjectNotLocked

$logPath = Join-Path $env:TEMP ("unity-reserialize-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")

# Unity.exe is a GUI subsystem application: a plain "&" call returns immediately, so
# the process is waited on explicitly and its real exit code is captured.
$unityArguments = @(
    "-batchmode", "-quit", "-nographics",
    "-projectPath", ('"' + $projectRoot + '"'),
    "-executeMethod", "BatchReserialize.Run",
    "-logFile", ('"' + $logPath + '"')
)

$unityProcess = Start-Process -FilePath $unityExe -ArgumentList $unityArguments -PassThru -Wait
$unityExitCode = $unityProcess.ExitCode
Write-Host "==> Unity exited with code $unityExitCode. Log: $logPath"

if ($unityExitCode -ne 0 -or (Test-ReserializeResult -LogPath $logPath)) {
    Write-Host "==> Reserialization FAILED. Last lines of the log:"
    if (Test-Path $logPath) {
        Get-Content $logPath -Tail 40
    }
    exit 1
}

$listPath = Join-Path $projectRoot "Temp\BatchReserialize.assets.txt"
if (Test-Path $listPath) {
    $processed = (Get-Content $listPath | Measure-Object).Count
    Write-Host "==> Processed assets: $processed (list: $listPath)"
}

$changed = @(git -C $repoRoot status --porcelain -- "unity/**/*.prefab" "unity/**/*.mat")
Write-Host "==> Prefabs/materials modified by git: $($changed.Count)"

# Stale script GUIDs after a mass rewrite would drop components silently at load time.
& (Join-Path $PSScriptRoot "unity-check-script-references.ps1") -ProjectPath $ProjectPath
if ($LASTEXITCODE -ne 0) {
    exit 1
}

Write-Host "==> Reserialization succeeded."
