param(
    [string]$SpritesRoot = "$PSScriptRoot\..\unity\Assets\Resources\Data\Heroes\Sprites",
    [string]$InfoRoot = "$PSScriptRoot\..\unity\Assets\Resources\Data\Heroes\Info",
    [string]$SpineHeroesRoot = "$PSScriptRoot\..\unity\Assets\Sprites\Heroes",
    [string]$OutputPath = "$PSScriptRoot\..\src\Wpf\Sektor.DarkestDungeon.Wpf\HeroIconsContent.props"
)

# Generates HeroIconsContent.props: links the hero icons of every class from the unity sprite
# assets into Content\HeroIcons\<class_id>\ of the WPF client build:
#   <class_id>\<A-D>\<class_id>_portrait_roster.png -> portrait.<A-D>.png  (85x85 roster portraits)
#   <class_id>\<class_id>_guild_header.png          -> header.png          (wide guild banner art)
#   <class_id>\<class_id>.ability.<one..seven>.png  -> ability.<icon>.png  (72x72 combat skill icons)
# The class id is the snake_case id of the hero content folder (man_at_arms), so consumers can key
# icons by the same id the core catalogs use.
#
# The script also verifies that every hero known to the spine demo (unity\Assets\Sprites\Heroes,
# PascalCase folder per hero) has an icon folder, and that every ability icon referenced by the
# hero content art: block (combat_skill/riposte_skill .icon) exists - a new hero or skill without
# art fails the run instead of silently showing an empty slot.
#
# Usage:
#   pwsh tools\hero-icons-content.ps1            # regenerate from unity assets
#   pwsh tools\hero-icons-content.ps1 -SpritesRoot <dir> -OutputPath <file>
#
# Exit code 0 when written; 1 with a problem list when a hero misses a portrait/banner/ability icon.

$ErrorActionPreference = "Stop"

$spritesRoot = (Resolve-Path -LiteralPath $SpritesRoot).Path
$infoRootPath = (Resolve-Path -LiteralPath $InfoRoot).Path
$spineHeroesRoot = (Resolve-Path -LiteralPath $SpineHeroesRoot).Path
$outputPath = [IO.Path]::GetFullPath($OutputPath)
$unityRelative = "..\..\..\unity\Assets\Resources\Data\Heroes\Sprites"
$outfits = @("A", "B", "C", "D")

$entries = [System.Collections.Generic.List[string]]::new()
$problems = [System.Collections.Generic.List[string]]::new()
$heroCount = 0

function Add-IconEntry {
    param(
        [string]$SourceRelative,
        [string]$LinkPath
    )

    $script:entries.Add("    <None Include=`"`$(MSBuildThisFileDirectory)$SourceRelative`" Link=`"$LinkPath`" CopyToOutputDirectory=`"PreserveNewest`" />")
}

function ConvertTo-ClassId {
    param([string]$HeroName)

    $builder = [System.Text.StringBuilder]::new()
    for ($i = 0; $i -lt $HeroName.Length; $i++) {
        $current = $HeroName[$i]
        if ([char]::IsUpper($current)) {
            if ($i -gt 0 -and $HeroName[$i - 1] -ne '_') {
                [void]$builder.Append('_')
            }
            [void]$builder.Append([char]::ToLowerInvariant($current))
        }
        else {
            [void]$builder.Append($current)
        }
    }

    return $builder.ToString()
}

foreach ($heroDir in Get-ChildItem -LiteralPath $spritesRoot -Directory | Sort-Object Name) {
    $classId = $heroDir.Name
    $heroCount++

    foreach ($outfit in $outfits) {
        $portraitName = "${classId}_portrait_roster.png"
        $portraitPath = Join-Path $heroDir.FullName "$outfit\$portraitName"
        if (-not (Test-Path -LiteralPath $portraitPath)) {
            $problems.Add("${classId}: missing portrait $outfit ($portraitPath)")
            continue
        }

        Add-IconEntry -SourceRelative "$unityRelative\$classId\$outfit\$portraitName" `
            -LinkPath "Content\HeroIcons\$classId\portrait.$outfit.png"
    }

    $headerName = "${classId}_guild_header.png"
    $headerPath = Join-Path $heroDir.FullName $headerName
    if (-not (Test-Path -LiteralPath $headerPath)) {
        $problems.Add("${classId}: missing guild header ($headerPath)")
    }
    else {
        Add-IconEntry -SourceRelative "$unityRelative\$classId\$headerName" `
            -LinkPath "Content\HeroIcons\$classId\header.png"
    }

    $abilityFiles = @(Get-ChildItem -LiteralPath $heroDir.FullName -File -Filter "$classId.ability.*.png" | Sort-Object Name)
    if ($abilityFiles.Count -eq 0) {
        $problems.Add("${classId}: no ability icons ($classId.ability.*.png)")
    }
    foreach ($abilityFile in $abilityFiles) {
        $iconId = $abilityFile.BaseName.Substring($classId.Length + ".ability.".Length)
        Add-IconEntry -SourceRelative "$unityRelative\$classId\$($abilityFile.Name)" `
            -LinkPath "Content\HeroIcons\$classId\ability.$iconId.png"
    }
}

foreach ($spineHero in Get-ChildItem -LiteralPath $spineHeroesRoot -Directory | Sort-Object Name) {
    $classId = ConvertTo-ClassId -HeroName $spineHero.Name
    if (-not (Test-Path -LiteralPath (Join-Path $spritesRoot $classId))) {
        $problems.Add("spine hero $($spineHero.Name): no icon folder $classId in $spritesRoot")
    }
}

$artIconCount = 0
foreach ($infoFile in (Get-ChildItem -LiteralPath $infoRootPath -File -Filter '*.bytes' | Sort-Object Name)) {
    $text = [IO.File]::ReadAllText($infoFile.FullName)
    $nameMatch = [regex]::Match($text, '(?m)^[ \t]*name:[ \t]*(\S+)')
    if (-not $nameMatch.Success) {
        $problems.Add("$($infoFile.Name): missing name: header")
        continue
    }

    $classId = $nameMatch.Groups[1].Value.Trim().ToLowerInvariant()
    $artMatch = [regex]::Match($text, '(?ms)^[ \t]*art:[ \t]*\r?\n(.*?)^[ \t]*\.end[ \t]*\r?$')
    if (-not $artMatch.Success) {
        $problems.Add("${classId}: missing art: section")
        continue
    }

    $iconMatches = [regex]::Matches(
        $artMatch.Groups[1].Value,
        '(?m)^[ \t]*(?:combat_skill|riposte_skill):.*?\.icon[ \t]+"([^"]+)"')
    if ($iconMatches.Count -eq 0) {
        $problems.Add("${classId}: art: section has no skill icons")
        continue
    }

    foreach ($iconMatch in $iconMatches) {
        $iconId = $iconMatch.Groups[1].Value.ToLowerInvariant()
        $artIconCount++
        $iconPath = Join-Path (Join-Path $spritesRoot $classId) "$classId.ability.$iconId.png"
        if (-not (Test-Path -LiteralPath $iconPath)) {
            $problems.Add("${classId}: missing ability icon '$iconId' ($iconPath)")
        }
    }
}

if ($problems.Count -gt 0) {
    Write-Output "Incomplete hero icons:"
    $problems | ForEach-Object { Write-Output "  $_" }
    exit 1
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("<Project>")
$lines.Add("")
$lines.Add("  <!-- Generated by tools\hero-icons-content.ps1 - do not edit by hand. Links the hero icons")
$lines.Add("       (roster portraits of outfits A-D, the guild header banner and the combat ability icons)")
$lines.Add("       from unity\Assets\Resources\Data\Heroes\Sprites into Content\HeroIcons\<ClassId>\ so WPF")
$lines.Add("       screens can show a hero icon or an ability icon by the class id. -->")
$lines.Add("  <ItemGroup>")
foreach ($entry in $entries) {
    $lines.Add($entry)
}
$lines.Add("  </ItemGroup>")
$lines.Add("")
$lines.Add("</Project>")

[IO.File]::WriteAllLines($outputPath, $lines, [Text.UTF8Encoding]::new($false))
Write-Output "Written ${outputPath}: heroes=$heroCount entries=$($entries.Count) artIcons=$artIconCount"
