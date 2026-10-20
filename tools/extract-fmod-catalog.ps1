# Builds a deterministic runtime audio catalog from Unity's FMOD Studio cache.
#
# The cache (FMODStudioCache.asset) is the only source that reliably maps event paths to the
# bank that contains them. The binary .strings.bank files are not parsed here on purpose: they
# are opaque, versioned FMOD payloads. Dynamic event paths emitted by gameplay are recorded with
# their actual value at runtime by DarkestSoundManager/GameEventRecording, so this catalog only
# lists static events known to the editor.
#
# Usage:
#   pwsh tools\extract-fmod-catalog.ps1
#   pwsh tools\extract-fmod-catalog.ps1 -CachePath <asset> -OutputPath <json> -MarkdownPath <md>
#
# Dependencies: PowerShell 7+, the committed Unity FMOD cache. No FMOD runtime is needed.
# If the cache is missing or a bank reference cannot be resolved, the script fails with a
# non-zero exit code and writes the full exception to the log.
#
# Log: %TEMP%\extract-fmod-catalog.log (created at startup, append-only per run).

param(
    [string]$CachePath = "",
    [string]$OutputPath = "",
    [string]$MarkdownPath = "",
    [string]$LogPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $CachePath) {
    $CachePath = Join-Path $repoRoot "unity\Assets\FMODStudioCache.asset"
}
if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot "docs\audio\audio_catalog.json"
}
if (-not $MarkdownPath) {
    $MarkdownPath = Join-Path $repoRoot "docs\audio\audio_catalog.md"
}
if (-not $LogPath) {
    $LogPath = Join-Path ([System.IO.Path]::GetTempPath()) "extract-fmod-catalog.log"
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$logDirectory = Split-Path -Parent $LogPath
if ($logDirectory -and -not (Test-Path -LiteralPath $logDirectory)) {
    New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
}
[System.IO.File]::WriteAllText($LogPath, "", $utf8)

function Write-Log {
    param(
        [string]$Level,
        [string]$Message
    )
    $line = "{0:o} [{1}] {2}" -f (Get-Date).ToUniversalTime(), $Level, $Message
    [System.IO.File]::AppendAllText($LogPath, $line + [Environment]::NewLine, $utf8)
}

function Write-Utf8File {
    param(
        [string]$Path,
        [string]$Content
    )
    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    [System.IO.File]::WriteAllText($Path, $Content, $utf8)
}

try {
    Write-Log "INFO" "Start cache=$CachePath output=$OutputPath markdown=$MarkdownPath"
    if (-not (Test-Path -LiteralPath $CachePath)) {
        throw "FMOD cache not found: $CachePath"
    }

    $lines = [System.IO.File]::ReadAllLines($CachePath, $utf8)
    Write-Log "INFO" "Read $($lines.Count) lines"
    $text = [string]::Join([Environment]::NewLine, $lines)

    $sectionRegex = [regex]'(?ms)^--- !u!114 &(?<id>\d+)\r?\n(?<body>.*?)(?=^--- |\z)'
    $pathRegex = [regex]'(?m)^  Path: (?<path>.+?)\r?$'
    $positiveIntRegex = [regex]'^  (?<name>IsStream|Is3D|IsOneShot): (?<value>[01])$'
    $bankBlockRegex = [regex]'(?ms)^  Banks:\r?\n(?<block>(?:  - \{fileID: \d+\}\r?\n?)+)'
    $bankRefRegex = [regex]'fileID: (?<id>\d+)'

    $banksById = @{}
    $banksByName = @{}
    $eventEntries = 0
    $events = [ordered]@{}
    $unresolvedBanks = New-Object System.Collections.Generic.List[string]
    $sections = $sectionRegex.Matches($text)

    # Bank entries are appended to the cache after the events that reference them, so collect
    # every bank first and resolve event -> bank in a second pass.
    foreach ($section in $sections) {
        $sectionId = $section.Groups["id"].Value
        $body = $section.Groups["body"].Value
        $pathMatch = $pathRegex.Match($body)
        if (-not $pathMatch.Success) {
            continue
        }

        $path = $pathMatch.Groups["path"].Value.Trim()
        if ($path -notlike "*.bank") {
            continue
        }

        $bankName = [System.IO.Path]::GetFileNameWithoutExtension($path)
        $banksById[$sectionId] = [pscustomobject]@{
            name = $bankName
            path = $path
        }
        if (-not $banksByName.ContainsKey($bankName)) {
            $banksByName[$bankName] = [pscustomobject]@{
                name = $bankName
                path = $path
            }
        }
        Write-Log "DEBUG" "Bank $sectionId -> $bankName"
    }

    foreach ($section in $sections) {
        $sectionId = $section.Groups["id"].Value
        $body = $section.Groups["body"].Value
        $pathMatch = $pathRegex.Match($body)
        if (-not $pathMatch.Success) {
            continue
        }

        $path = $pathMatch.Groups["path"].Value.Trim()
        if ($path -like "*.bank" -or -not $path.StartsWith("event:/")) {
            continue
        }

        $eventEntries++
        $bankIds = New-Object System.Collections.Generic.List[string]
        $bankBlock = $bankBlockRegex.Match($body)
        if ($bankBlock.Success) {
            foreach ($reference in $bankRefRegex.Matches($bankBlock.Groups["block"].Value)) {
                $bankIds.Add($reference.Groups["id"].Value)
            }
        }

        $isStream = $false
        $is3D = $false
        $isOneShot = $false
        foreach ($line in ($body -split "\r?\n")) {
            $flagMatch = [regex]::Match($line, '^  (?<name>IsStream|Is3D|IsOneShot): (?<value>[01])$')
            if (-not $flagMatch.Success) {
                continue
            }
            $flagValue = $flagMatch.Groups["value"].Value -eq "1"
            switch ($flagMatch.Groups["name"].Value) {
                "IsStream" { $isStream = $flagValue }
                "Is3D" { $is3D = $flagValue }
                "IsOneShot" { $isOneShot = $flagValue }
            }
        }

        if (-not $events.Contains($path)) {
            $events[$path] = [pscustomobject]@{
                path = $path
                banks = New-Object System.Collections.Generic.HashSet[string]
                isStream = $isStream
                is3D = $is3D
                isOneShot = $isOneShot
            }
        }

        $event = $events[$path]
        $event.isStream = $event.isStream -or $isStream
        $event.is3D = $event.is3D -or $is3D
        $event.isOneShot = $event.isOneShot -or $isOneShot

        foreach ($bankId in $bankIds) {
            if ($banksById.ContainsKey($bankId)) {
                [void]$event.banks.Add($banksById[$bankId].name)
            }
            else {
                $message = "Event '$path' references unknown bank fileID $bankId"
                if (-not $unresolvedBanks.Contains($message)) {
                    $unresolvedBanks.Add($message)
                }
            }
        }
    }

    Write-Log "INFO" "Parsed $eventEntries event entries and $($banksByName.Count) banks"
    if ($unresolvedBanks.Count -gt 0) {
        foreach ($message in $unresolvedBanks) {
            Write-Log "ERROR" $message
        }
        throw "Unresolved bank references: $($unresolvedBanks.Count)"
    }
    if ($events.Count -eq 0) {
        throw "No event:/ paths found in $CachePath"
    }

    $eventObjects = New-Object System.Collections.Generic.List[object]
    foreach ($event in $events.Values) {
        $kind = ($event.path.Substring("event:/".Length) -split "/")[0]
        $eventObjects.Add([pscustomobject]@{
            path = $event.path
            kind = $kind
            banks = @($event.banks | Sort-Object)
            isStream = $event.isStream
            is3D = $event.is3D
            isOneShot = $event.isOneShot
        })
    }
    $sortedEvents = @($eventObjects | Sort-Object -Property path)

    $bankObjects = New-Object System.Collections.Generic.List[object]
    foreach ($bank in ($banksByName.Values | Sort-Object -Property name)) {
        $eventCount = @($sortedEvents | Where-Object { $_.banks -contains $bank.name }).Count
        $bankObjects.Add([pscustomobject]@{
            name = $bank.name
            path = $bank.path
            eventCount = $eventCount
        })
    }

    $musicEvents = @($sortedEvents | Where-Object { $_.kind -eq "music" })
    $groups = @($sortedEvents | Group-Object -Property kind | Sort-Object -Property Name | ForEach-Object {
        [pscustomobject]@{
            name = $_.Name
            eventCount = $_.Count
        }
    })

    $catalog = [pscustomobject]@{
        schemaVersion = 1
        source = "unity/Assets/FMODStudioCache.asset"
        generatedBy = "tools/extract-fmod-catalog.ps1"
        counts = [pscustomobject]@{
            eventEntries = $eventEntries
            uniqueEvents = $sortedEvents.Count
            banks = $bankObjects.Count
            musicEvents = $musicEvents.Count
        }
        groups = $groups
        banks = $bankObjects
        events = $sortedEvents
    }

    $json = $catalog | ConvertTo-Json -Depth 8
    Write-Utf8File -Path $OutputPath -Content ($json + [Environment]::NewLine)

    $markdown = New-Object System.Text.StringBuilder
    [void]$markdown.AppendLine("# FMOD audio catalog")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("Generated by ``tools\extract-fmod-catalog.ps1`` from")
    [void]$markdown.AppendLine("``unity\Assets\FMODStudioCache.asset``. The full machine-readable list is")
    [void]$markdown.AppendLine("``docs\audio\audio_catalog.json``.")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("Dynamic gameplay paths (built from hero/skill names at runtime) are not listed;")
    [void]$markdown.AppendLine("the recorder stores the actual ``event:/`` path that was requested.")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("## Counts")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("| Item | Count |")
    [void]$markdown.AppendLine("|---|---:|")
    [void]$markdown.AppendLine("| Event entries in cache | $eventEntries |")
    [void]$markdown.AppendLine("| Unique event paths | $($sortedEvents.Count) |")
    [void]$markdown.AppendLine("| Banks | $($bankObjects.Count) |")
    [void]$markdown.AppendLine("| Music events | $($musicEvents.Count) |")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("## Banks")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("| Bank | Path | Events |")
    [void]$markdown.AppendLine("|---|---|---:|")
    foreach ($bank in $bankObjects) {
        [void]$markdown.AppendLine("| $($bank.name) | ``$($bank.path)`` | $($bank.eventCount) |")
    }
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("## Music events")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("| Event | Banks | Stream |")
    [void]$markdown.AppendLine("|---|---|---|")
    foreach ($event in $musicEvents) {
        [void]$markdown.AppendLine("| ``$($event.path)`` | $($event.banks -join ', ') | $($event.isStream) |")
    }
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("## Events by group")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("| Group | Events |")
    [void]$markdown.AppendLine("|---|---:|")
    foreach ($group in $groups) {
        [void]$markdown.AppendLine("| $($group.name) | $($group.eventCount) |")
    }
    Write-Utf8File -Path $MarkdownPath -Content $markdown.ToString()

    Write-Log "INFO" "Wrote $($sortedEvents.Count) unique events, $($bankObjects.Count) banks, $($musicEvents.Count) music events"
    Write-Log "INFO" "Done"
    Write-Output "Wrote $OutputPath ($($sortedEvents.Count) events, $($bankObjects.Count) banks)"
    Write-Output "Wrote $MarkdownPath"
}
catch {
    Write-Log "ERROR" $_.Exception.ToString()
    throw
}
