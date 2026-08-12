# Compile-only verification of the Unity scripts without building the player.
#
# Imports the project in batch mode (Unity compiles all scripts, including Assets\Editor)
# and then checks the log for compilation errors. Much faster than a full player build,
# so it is the recommended check after changing code in Assets\Scripts or Assets\Editor.
#
# 1. Locates the Unity 2017.4 editor (known roots / UNITY_EDITOR_PATH / -UnityEditorPath).
# 2. Aborts when the project is already open in the editor (Temp\UnityLockfile).
# 3. Optionally provisions the Lan transport plugins (-Provision); by default they are
#    expected to be present in Assets\Plugins\Internal (gitignored).
# 4. Runs Unity in batch mode (no BuildPlayer), then parses the log for errors.
# 5. Exit code 0 when compilation succeeded, 1 otherwise.
#
# Usage: pwsh tools\compile-check.ps1 [-UnityEditorPath <path>] [-Provision]

param(
    [string]$UnityEditorPath = "",
    [switch]$Provision
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Find-UnityEditor {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path (Join-Path $Preferred "Editor\Unity.exe"))) {
        return $Preferred
    }
    if ($env:UNITY_EDITOR_PATH -and (Test-Path (Join-Path $env:UNITY_EDITOR_PATH "Editor\Unity.exe"))) {
        return $env:UNITY_EDITOR_PATH
    }

    $knownRoots = @(
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

# Blocks when an editor is actually open on this project; a leftover Temp\UnityLockfile
# from a crashed or failed batch run (no matching Unity process) is removed instead.
function Assert-ProjectNotLocked {
    $lockPath = Join-Path $repoRoot "Temp\UnityLockfile"
    if (-not (Test-Path $lockPath)) {
        return
    }

    $running = Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine.Contains($repoRoot) }
    if ($running) {
        throw "The project is open in the Unity editor. Close it first, then rerun the check."
    }

    Write-Host "==> Removing stale Unity lock file ($lockPath)"
    Remove-Item $lockPath -Force
}

if (-not $UnityEditorPath) {
    $UnityEditorPath = Find-UnityEditor
}
if (-not $UnityEditorPath) {
    throw "Unity editor not found. Pass -UnityEditorPath <editor root> (folder containing Editor\Unity.exe)."
}
$unityExe = Join-Path $UnityEditorPath "Editor\Unity.exe"
Write-Host "==> Unity editor: $UnityEditorPath"

Assert-ProjectNotLocked

if ($Provision) {
    Write-Host "==> Provisioning Lan transport plugins"
    & (Join-Path $PSScriptRoot "provision-unity-plugins.ps1") -UnityEditorPath $UnityEditorPath
    if ($LASTEXITCODE -ne 0) {
        throw "Plugin provisioning failed."
    }
}

$logPath = Join-Path $env:TEMP ("unity-compile-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")

# Unity.exe is a GUI subsystem application: a plain "&" call returns immediately, so
# the process is waited on explicitly and its real exit code is captured.
$unityArguments = @(
    "-batchmode", "-quit", "-nographics",
    "-projectPath", ('"' + $repoRoot + '"'),
    "-logFile", ('"' + $logPath + '"')
)

function Test-CompileResult {
    param([string]$CompileLogPath)

    if (-not (Test-Path $CompileLogPath)) {
        return $true
    }

    $logText = Get-Content $CompileLogPath -Raw
    $hasErrorMarker = $logText -match "error CS\d+|Compilation failed|: error :"
    $hasLoadMarker = $logText -match "Compilation succeeded|Reloading assemblies after script compilation|successfully reloaded assembly"
    return ($hasErrorMarker -or -not $hasLoadMarker)
}

$unityProcess = Start-Process -FilePath $unityExe -ArgumentList $unityArguments -PassThru -Wait
$unityExitCode = $unityProcess.ExitCode
Write-Host "==> Unity exited with code $unityExitCode. Log: $logPath"

$failed = Test-CompileResult -CompileLogPath $logPath

# A deleted source file can leave a stale entry in the incremental compile cache
# (error CS2001); clear the compiled script assemblies and retry once.
if ($failed -and (Test-Path $logPath)) {
    $logText = Get-Content $logPath -Raw
    if ($logText -match "error CS2001:") {
        Write-Host "==> Stale script cache detected (deleted source file). Clearing Library\ScriptAssemblies and retrying."
        Remove-Item (Join-Path $repoRoot "Library\ScriptAssemblies\*") -Force -ErrorAction SilentlyContinue

        $logPath = Join-Path $env:TEMP ("unity-compile-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")
        $unityArguments[$unityArguments.Length - 1] = ('"' + $logPath + '"')
        $unityProcess = Start-Process -FilePath $unityExe -ArgumentList $unityArguments -PassThru -Wait
        $unityExitCode = $unityProcess.ExitCode
        Write-Host "==> Unity exited with code $unityExitCode. Log: $logPath"
        $failed = Test-CompileResult -CompileLogPath $logPath
    }
}

if ($failed) {
    Write-Host "==> Compilation FAILED. Last lines of the log:"
    if (Test-Path $logPath) {
        Get-Content $logPath -Tail 40
    }
    exit 1
}

Write-Host "==> Compilation succeeded."
