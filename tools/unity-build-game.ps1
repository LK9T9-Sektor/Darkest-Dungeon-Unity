# Build the standalone Windows x64 player without opening the Unity editor.
#
# 1. Locates the Unity editor (known roots / UNITY_EDITOR_PATH / -UnityEditorPath).
# 2. Aborts when the project is already open in the editor (Temp\UnityLockfile).
# 3. Provisions the Lan transport plugins (tools\unity-provision-plugins.ps1).
# 4. Runs Unity in batch mode: BuildGame.Build -> Build\Darkest Dungeon\Darkest Dungeon.exe.
# 5. Fails (exit 1) on compilation/build errors.
# 6. Drops steam_appid.txt next to the executable so SteamAPI_Init works in the player.
#
# Usage: pwsh tools\unity-build-game.ps1 [-ProjectPath <project>] [-UnityEditorPath <path>] [-BuildDir <path>] [-AppId <uint>] [-SkipProvision]

param(
    [string]$ProjectPath = "",
    [string]$UnityEditorPath = "",
    [string]$BuildDir = "",
    [uint32]$AppId = 480,
    [switch]$SkipProvision
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) {
    $ProjectPath = "unity"
}
$projectRoot = Join-Path $repoRoot $ProjectPath

if (-not $BuildDir) {
    $BuildDir = Join-Path $projectRoot "Build\Darkest Dungeon"
}
$executableName = "Darkest Dungeon.exe"
$executablePath = Join-Path $BuildDir $executableName

function Find-UnityEditor {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path (Join-Path $Preferred "Editor\Unity.exe"))) {
        return $Preferred
    }
    if ($env:UNITY_EDITOR_PATH -and (Test-Path (Join-Path $env:UNITY_EDITOR_PATH "Editor\Unity.exe"))) {
        return $env:UNITY_EDITOR_PATH
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
        throw "The project is open in the Unity editor. Close it first, then rerun the build."
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

if (-not $SkipProvision) {
    Write-Host "==> Provisioning Lan transport plugins"
    & (Join-Path $PSScriptRoot "unity-provision-plugins.ps1") -ProjectPath $ProjectPath -UnityEditorPath $UnityEditorPath -AppId $AppId
    if ($LASTEXITCODE -ne 0) {
        throw "Plugin provisioning failed."
    }
}

Write-Host "==> Building player to: $BuildDir"
New-Item -ItemType Directory -Force -Path $BuildDir | Out-Null

$logPath = Join-Path $env:TEMP ("unity-build-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")
$previousBuildDir = $env:DD_BUILD_DIR
$env:DD_BUILD_DIR = $BuildDir

# Unity.exe is a GUI subsystem application: a plain "&" call returns immediately, so
# the process is waited on explicitly and its real exit code is captured.
$unityArguments = @(
    "-batchmode", "-quit", "-nographics",
    "-projectPath", ('"' + $projectRoot + '"'),
    "-executeMethod", "BuildGame.Build",
    "-logFile", ('"' + $logPath + '"')
)
try {
    $unityProcess = Start-Process -FilePath $unityExe -ArgumentList $unityArguments -PassThru -Wait
    $unityExitCode = $unityProcess.ExitCode
}
finally {
    if ($previousBuildDir -eq $null) {
        Remove-Item Env:DD_BUILD_DIR -ErrorAction SilentlyContinue
    }
    else {
        $env:DD_BUILD_DIR = $previousBuildDir
    }
}

Write-Host "==> Unity exited with code $unityExitCode. Log: $logPath"

$failed = $false
if (Test-Path $logPath) {
    $logText = Get-Content $logPath -Raw
    if ($logText -match "error CS\d+|Compilation failed|Build failed|BuildGame\.Build.*failed|: error :") {
        $failed = $true
    }
}

if (-not (Test-Path $executablePath)) {
    $failed = $true
}

if ($failed) {
    Write-Host "==> Build FAILED. Last lines of the log:"
    if (Test-Path $logPath) {
        Get-Content $logPath -Tail 40
    }
    exit 1
}

Write-Host "==> Build succeeded: $executablePath"

Write-Host "==> Placing steam_appid.txt next to the executable"
$appIdFile = Join-Path $repoRoot "steam_appid.txt"
$targetAppIdFile = Join-Path $BuildDir "steam_appid.txt"
if (-not (Test-Path $targetAppIdFile)) {
    if (Test-Path $appIdFile) {
        Copy-Item -Path $appIdFile -Destination $targetAppIdFile -Force
    }
    else {
        Set-Content -Path $targetAppIdFile -Value $AppId -NoNewline
    }
}

Write-Host "Done. Run with tools\unity-run-game.ps1 or tools\unity-dev-run.ps1"
