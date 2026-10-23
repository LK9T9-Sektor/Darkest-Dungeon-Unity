# Semantic diff of Unity YAML assets: compares the committed blob with the working copy
# and separates "serialization format only" differences from real data changes.
#
# Used to verify a mass reserialization (tools\unity-reserialize.ps1): a format upgrade
# rewrites every field, so a plain diff proves nothing. The script matches objects by
# fileID, compares their data ignoring format keys, tolerates float rounding and reports
# only the changes that carry real data: lost references, changed values, new objects.
#
# Usage:
#   git diff --name-only -- unity/Assets > changed.txt
#   pwsh tools\unity-asset-diff.ps1 -FileList changed.txt
#   pwsh tools\unity-asset-diff.ps1 -Files unity/Assets/Resources/Prefabs/Heroes/crusader.prefab

param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$FileList = "",
    [string[]]$Files = @()
)

$ErrorActionPreference = "Stop"

# Keys that only exist because of the serializer/format version.
$ignoredKeyPattern = '^(m_ObjectHideFlags|m_PrefabParentObject|m_PrefabInternal|m_CorrespondingSourceObject|m_PrefabInstance|m_PrefabAsset|serializedVersion|m_LocalEulerAnglesHint|m_ConstrainProportionsScale|m_RaycastPadding|m_Maskable|m_PixelsPerUnitMultiplier|m_UseSpriteMesh|m_CullTransparentMesh|m_PersistentCalls|m_Calls|m_LockedProperties|m_Parent|m_ModifiedSerializedProperties|m_ValidKeywords|m_InvalidKeywords|m_ShaderKeywords|m_BuildTextureStacks|m_AllowLocking|m_Ints|m_RootOrder|m_LayoutPriority|m_MaxHeight|m_MaxWidth|m_ChildControlWidth|m_ChildControlHeight|m_ChildScaleWidth|m_ChildScaleHeight|m_ReverseArrangement|m_SelectedColor|m_SelectedSprite|m_SelectedTrigger|m_WrapAround|m_AnimatePhysics|m_KeepAnimatorStateOnDisable|m_StabilizeFeet|m_WriteDefaultValuesOnDisable|m_AlwaysNormalize|m_ApplyRootMotion|m_Script|m_TypeName):'

$ignoredListPrefixes = @('- component:', '- 114:', '- 4:', '- 224:', '- 222:', '- 225:', '- 95:', '- 65:', '- 33:', '- 23:', '- 54:', '- 212:', '- 213:', '- 135:', '- 1001:')

function Get-UnityObjects {
    param([string]$Text)

    $objects = @{}
    $regex = [regex]'(?ms)^--- !u!(?<type>\d+) &(?<id>-?\d+)[^\n]*\n.*?(?=^--- !u!|\z)'
    foreach ($match in $regex.Matches($Text)) {
        $objects[$match.Groups['id'].Value] = [pscustomobject]@{
            Type = $match.Groups['type'].Value
            Body = $match.Value
        }
    }

    return $objects
}

# Old materials store their properties as "- first: / name: X / second: value"; the current
# format stores "- X: value". Both are normalized to "prop/X = value" so the comparison only
# reports real value changes.
function Get-BodyLines {
    param([string]$Body)

    $lines = New-Object System.Collections.Generic.List[string]
    $currentName = $null
    $keywordState = $false
    foreach ($line in ($Body -split "`r?`n")) {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed -match '^--- !u!') { continue }
        $indent = $line.Length - $line.TrimStart().Length

        if ($currentName -and $indent -le 4 -and $trimmed -notmatch '^- ') { $currentName = $null }

        $isListPrefix = $trimmed -match '^- \d+: \{fileID:' -or $trimmed -match '^- component: \{fileID:'
        if (-not $isListPrefix) {
            foreach ($prefix in $ignoredListPrefixes) {
                if ($trimmed.StartsWith($prefix)) { $isListPrefix = $true; break }
            }
        }
        if ($isListPrefix) { continue }

        # Shader keywords moved from one space-separated string to a YAML list.
        if ($trimmed -match '^m_ShaderKeywords:(?: (.*))?$') {
            if ($Matches[1]) {
                foreach ($token in ($Matches[1].Trim() -split ' ')) {
                    if ($token) { $lines.Add('kw/' + $token) }
                }
            }
            continue
        }
        if ($keywordState -and $trimmed -match '^- (\S+)$') {
            $lines.Add('kw/' + $Matches[1])
            continue
        }
        if ($trimmed -match $ignoredKeyPattern) {
            $keywordState = $trimmed -match '^m_(Valid|Invalid)Keywords:'
            continue
        }
        $keywordState = $false

        if ($trimmed -eq '- first:') { continue }
        if ($trimmed -match '^name: (.+)$') { $currentName = $Matches[1].Trim(); continue }
        if ($trimmed -match '^second:(?: (.*))?$') {
            if ($Matches[1]) { $lines.Add('prop/' + $currentName + ' = ' + $Matches[1].Trim()) }
            continue
        }
        if ($trimmed -match '^- (?!first:)([A-Za-z_][\w ]*):(?: (.*))?$') {
            $currentName = $Matches[1].Trim()
            if ($Matches[2]) { $lines.Add('prop/' + $currentName + ' = ' + $Matches[2].Trim()) }
            continue
        }
        if ($currentName -and $indent -gt 4) {
            $lines.Add('prop/' + $currentName + '/' + $trimmed)
            continue
        }

        $lines.Add($trimmed)
    }

    return $lines
}

function Get-ScriptRef {
    param([string]$Body)

    $match = [regex]::Match($Body, 'm_Script: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d+\}')
    if (-not $match.Success) { return '' }
    return $match.Groups[2].Value + '/' + $match.Groups[1].Value
}

function Get-FieldValue {
    param([string]$Body, [string]$Key)

    $match = [regex]::Match($Body, '(?m)^\s{2}' + [regex]::Escape($Key) + ':\s*(.*)$')
    if (-not $match.Success) { return '' }
    return $match.Groups[1].Value.Trim()
}

function Test-NumericMatch {
    param([string]$Left, [string]$Right)

    $leftNumbers = [regex]::Matches($Left, '-?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?') | ForEach-Object { [double]$_.Value }
    $rightNumbers = [regex]::Matches($Right, '-?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?') | ForEach-Object { [double]$_.Value }
    if ($leftNumbers.Count -ne $rightNumbers.Count) { return $false }
    for ($i = 0; $i -lt $leftNumbers.Count; $i++) {
        $delta = [math]::Abs($leftNumbers[$i] - $rightNumbers[$i])
        $scale = [math]::Max(1.0, [math]::Abs($leftNumbers[$i]))
        if ($delta -gt 1e-4 * $scale) { return $false }
    }

    return $true
}

function Test-DefaultValue {
    param([string]$Line)

    $parts = $Line -split ':', 2
    if ($parts.Count -lt 2) { return $true }
    $value = $parts[1].Trim()
    if ($value -eq '') { return $true }
    return $value -in @('0', '1', '-1', '[]', '{}', 'false', 'true', '{fileID: 0}', 'None', 'Untagged', '(0, 0)')
}

function Compare-File {
    param([string]$Path)

    $oldText = (git -C $RepoRoot show "HEAD:$Path") -join "`n"
    $newText = Get-Content (Join-Path $RepoRoot $Path) -Raw
    $old = Get-UnityObjects $oldText
    $new = Get-UnityObjects $newText

    $result = [pscustomobject]@{
        Path = $Path
        RemovedIds = @()
        AddedIds = @()
        ChangedTypes = @()
        Hierarchy = @()
        ScriptPairs = @{}
        Substantive = @()
        FloatNoise = 0
    }

    foreach ($id in $old.Keys) {
        if (-not $new.ContainsKey($id)) { $result.RemovedIds += ($old[$id].Type + '/' + $id) }
    }
    foreach ($id in $new.Keys) {
        if (-not $old.ContainsKey($id)) { $result.AddedIds += ($new[$id].Type + '/' + $id) }
    }

    foreach ($id in $old.Keys) {
        if (-not $new.ContainsKey($id)) { continue }
        if ($old[$id].Type -ne $new[$id].Type) { $result.ChangedTypes += $id }

        $oldChildren = Get-FieldValue $old[$id].Body 'm_Children'
        $newChildren = Get-FieldValue $new[$id].Body 'm_Children'
        $oldFather = Get-FieldValue $old[$id].Body 'm_Father'
        $newFather = Get-FieldValue $new[$id].Body 'm_Father'
        if ($oldChildren -ne $newChildren -or $oldFather -ne $newFather) {
            $result.Hierarchy += ("id=$id children[$oldChildren] -> [$newChildren] father[$oldFather] -> [$newFather]")
        }

        $oldScript = Get-ScriptRef $old[$id].Body
        $newScript = Get-ScriptRef $new[$id].Body
        if ($oldScript -and $newScript -and $oldScript -ne $newScript) {
            $result.ScriptPairs[$oldScript + ' -> ' + $newScript] = $true
        }

        $oldGroups = Get-BodyLines $old[$id].Body | Group-Object | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Count = $_.Count } }
        $newGroups = Get-BodyLines $new[$id].Body | Group-Object | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Count = $_.Count } }
        $diff = Compare-Object $oldGroups $newGroups -Property Name, Count
        if (-not $diff) { continue }

        $removed = @($diff | Where-Object { $_.SideIndicator -eq '<=' } | ForEach-Object { $_.Name })
        $added = @($diff | Where-Object { $_.SideIndicator -eq '=>' } | ForEach-Object { $_.Name })

        foreach ($removedLine in $removed) {
            $removedKey = ($removedLine -split ':', 2)[0]
            $matched = $false
            foreach ($addedLine in $added) {
                if (($addedLine -split ':', 2)[0] -ne $removedKey) { continue }
                if (Test-NumericMatch -Left $removedLine -Right $addedLine) { $matched = $true; $result.FloatNoise++; break }
            }
            if (-not $matched) {
                if (Test-DefaultValue $removedLine) { $result.FloatNoise++ } else { $result.Substantive += ('id=' + $id + ' - ' + $removedLine) }
            }
        }

        foreach ($addedLine in $added) {
            $addedKey = ($addedLine -split ':', 2)[0]
            $matched = $false
            foreach ($removedLine in $removed) {
                if (($removedLine -split ':', 2)[0] -ne $addedKey) { continue }
                if (Test-NumericMatch -Left $removedLine -Right $addedLine) { $matched = $true; break }
            }
            if (-not $matched) {
                if (Test-DefaultValue $addedLine) { $result.FloatNoise++ } else { $result.Substantive += ('id=' + $id + ' + ' + $addedLine) }
            }
        }
    }

    return $result
}

$assetPaths = @()
if ($Files) {
    $assetPaths = @($Files)
} elseif ($FileList) {
    $assetPaths = @(Get-Content $FileList | Where-Object { $_.Trim() -ne '' })
}

$results = New-Object System.Collections.Generic.List[object]
foreach ($assetPath in $assetPaths) {
    $results.Add((Compare-File $assetPath.Trim()))
}

Write-Host ("Files compared: " + $results.Count)
$withData = @($results | Where-Object { $_.Substantive.Count -gt 0 })
$withHierarchy = @($results | Where-Object { $_.Hierarchy.Count -gt 0 })
$withIds = @($results | Where-Object { $_.RemovedIds.Count -gt 0 -or $_.AddedIds.Count -gt 0 })
$withTypes = @($results | Where-Object { $_.ChangedTypes.Count -gt 0 })
$floatNoise = ($results | Measure-Object -Property FloatNoise -Sum).Sum

Write-Host ("Files with substantive data changes: " + $withData.Count)
Write-Host ("Files with hierarchy changes: " + $withHierarchy.Count)
Write-Host ("Files with object id changes: " + $withIds.Count)
Write-Host ("Files with component type changes: " + $withTypes.Count)
Write-Host ("Float-noise lines suppressed: " + $floatNoise)

$scriptPairs = @{}
foreach ($result in $results) {
    foreach ($pair in $result.ScriptPairs.Keys) { $scriptPairs[$pair] = $true }
}
Write-Host ("Script reference remaps: " + $scriptPairs.Count)
foreach ($pair in ($scriptPairs.Keys | Sort-Object)) { Write-Host ("  " + $pair) }

if ($withIds.Count -gt 0) {
    Write-Host "`n=== Object id changes ==="
    foreach ($result in $withIds) {
        Write-Host ($result.Path + " -" + ($result.RemovedIds -join ',') + " +" + ($result.AddedIds -join ','))
    }
}

if ($withHierarchy.Count -gt 0) {
    Write-Host "`n=== Hierarchy changes ==="
    foreach ($result in $withHierarchy) {
        Write-Host $result.Path
        $result.Hierarchy | Select-Object -First 10 | ForEach-Object { Write-Host ("  " + $_) }
    }
}

if ($withData.Count -gt 0) {
    Write-Host "`n=== Substantive data changes ==="
    foreach ($result in $withData) {
        Write-Host $result.Path
        $result.Substantive | Select-Object -First 25 | ForEach-Object { Write-Host ("  " + $_) }
        if ($result.Substantive.Count -gt 25) { Write-Host ("  ... " + ($result.Substantive.Count - 25) + " more") }
    }
}
