# Rebuilds the BattleTest scene (Assets\Scenes\BattleTest.unity) for one of the Unity projects.
#
# Runs the Unity editor in batch mode with the BattleTestSceneBuilder.Generate editor entry point,
# which rebuilds the scene from scratch (camera, battlefield, HUD, driver, config panel, event
# system). The scene is a standalone core-driven battle test and does not depend on legacy raid
# objects or the DarkestDungeonManager prefab.
#
# Usage: pwsh tools\unity-generate-battle-test-scene.ps1 [-ProjectPath <unity|unity-2017>] [-UnityEditorPath <root>]

param(
    [string]$ProjectPath = "unity",
    [string]$UnityEditorPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectRoot = Join-Path $repoRoot $ProjectPath

function Find-UnityEditor {
    param([string]$ProjectPath)

    if ($env:UNITY_EDITOR_PATH -and (Test-Path (Join-Path $env:UNITY_EDITOR_PATH "Editor\Unity.exe"))) {
        return $env:UNITY_EDITOR_PATH
    }

    $knownRoots = @(
        "C:\Program Files\Unity\Hub\Editor\6000.5.8f1",
        "E:\ProgramFiles\Unity2017.4.40f1",
        "D:\ProgramFiles\Unity2017.4.40f1",
        "D:\Program Files\Unity2017.4.40f1",
        "C:\Program Files\Unity\Hub\Editor\2017.4.40f1"
    )
    if ($ProjectPath -eq "unity-2017") {
        $knownRoots = @(
            "E:\ProgramFiles\Unity2017.4.40f1",
            "D:\ProgramFiles\Unity2017.4.40f1",
            "D:\Program Files\Unity2017.4.40f1",
            "C:\Program Files\Unity\Hub\Editor\2017.4.40f1"
        )
    }

    foreach ($root in $knownRoots) {
        if (Test-Path (Join-Path $root "Editor\Unity.exe")) {
            return $root
        }
    }
    return $null
}

if (-not $UnityEditorPath) {
    $UnityEditorPath = Find-UnityEditor -ProjectPath $ProjectPath
}
if (-not $UnityEditorPath) {
    throw "Unity editor not found. Pass -UnityEditorPath <editor root> (folder containing Editor\Unity.exe)."
}

$unityExe = Join-Path $UnityEditorPath "Editor\Unity.exe"
$logPath = Join-Path $env:TEMP ("unity-battletest-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")

Write-Host "==> Generating BattleTest scene for $ProjectPath with $UnityEditorPath"

$arguments = @(
    "-batchmode", "-quit", "-nographics",
    "-projectPath", ('"' + $projectRoot + '"'),
    "-executeMethod", "BattleTestSceneBuilder.Generate",
    "-logFile", ('"' + $logPath + '"')
)

$process = Start-Process -FilePath $unityExe -ArgumentList $arguments -PassThru -Wait
if ($process.ExitCode -ne 0) {
    Get-Content $logPath -Tail 40
    throw "Unity exited with code $($process.ExitCode). Log: $logPath"
}

Write-Host "==> BattleTest scene generated for $ProjectPath. Log: $logPath"
Write-Host "==> Verify: pwsh tools\unity-compile-check.ps1 -ProjectPath $ProjectPath"