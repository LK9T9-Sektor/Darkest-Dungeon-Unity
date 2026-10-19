param(
    [string]$HeroesRoot = "$PSScriptRoot\..\unity\Assets\Sprites\Heroes",
    [string]$OutputPath = "$PSScriptRoot\..\src\Wpf\Sektor.DarkestDungeon.Wpf\SpineDemoContent.props"
)

# Generates SpineDemoContent.props: one entry per hero animation set, linked from the unity sprite
# assets into Content\Spine\<Hero>\<Anim>\ of the WPF client build. Each skeleton file carries
# exactly one animation, so the spine demo switches animation sets (skeleton + atlas + outfit A
# page texture) to switch characters and animations. The page PNG keeps its original name because
# the atlas resolves it by the name embedded in the atlas file.
# Two set kinds are linked: base sets from <Hero>\Skeleton\<Set> (page texture in Outfits\A) and
# skill sets from <Hero>\Effects\<Skill> (page texture next to the skeleton) - the latter carry the
# real timed skill animations (base Attack* sets are zero-duration pose stubs).
#
# Usage:
#   pwsh tools\spine-demo-content.ps1            # regenerate from unity assets
#   pwsh tools\spine-demo-content.ps1 -HeroesRoot <dir> -OutputPath <file>
#
# Exit code 0 when written; 1 with a problem list when a set is incomplete (missing skel, atlas
# or page texture) - fix or remove the offending set instead of shipping a broken link.

$ErrorActionPreference = "Stop"

$heroesRoot = (Resolve-Path -LiteralPath $HeroesRoot).Path
$outputPath = [IO.Path]::GetFullPath($OutputPath)
$unityRelative = "..\..\..\unity\Assets\Sprites\Heroes"

$entries = [System.Collections.Generic.List[string]]::new()
$problems = [System.Collections.Generic.List[string]]::new()
$heroCount = 0

function Add-SpineSet {
    param(
        [string]$SetName,
        [string]$SourceFullPath,
        [string]$SourceRelative,
        [string]$PngPath,
        [string]$PngSourceRelative
    )

    $skel = @(Get-ChildItem -LiteralPath $SourceFullPath -Filter "*.skel.bytes" -File)
    $atlas = @(Get-ChildItem -LiteralPath $SourceFullPath -Filter "*.atlas.txt" -File)
    if ($skel.Count -ne 1 -or $atlas.Count -ne 1) {
        $script:problems.Add("${SetName}: expected exactly one .skel.bytes and one .atlas.txt (skel=$($skel.Count) atlas=$($atlas.Count))")
        return
    }

    $baseName = $skel[0].Name -replace "\.skel\.bytes$", ""
    if (-not (Test-Path -LiteralPath $PngPath)) {
        $script:problems.Add("${SetName}: missing page texture $PngPath")
        return
    }

    $linkDir = "Content\Spine\$SetName"
    $script:entries.Add("    <None Include=`"`$(MSBuildThisFileDirectory)$SourceRelative\$($skel[0].Name)`" Link=`"$linkDir\$baseName.skel`" CopyToOutputDirectory=`"PreserveNewest`" />")
    $script:entries.Add("    <None Include=`"`$(MSBuildThisFileDirectory)$SourceRelative\$($atlas[0].Name)`" Link=`"$linkDir\$baseName.atlas`" CopyToOutputDirectory=`"PreserveNewest`" />")
    $script:entries.Add("    <None Include=`"`$(MSBuildThisFileDirectory)$PngSourceRelative`" Link=`"$linkDir\$baseName.png`" CopyToOutputDirectory=`"PreserveNewest`" />")
}

foreach ($heroDir in Get-ChildItem -LiteralPath $heroesRoot -Directory | Sort-Object Name) {
    $skeletonRoot = Join-Path $heroDir.FullName "Skeleton"
    if (-not (Test-Path -LiteralPath $skeletonRoot)) {
        continue
    }

    $heroCount++
    foreach ($animDir in Get-ChildItem -LiteralPath $skeletonRoot -Directory | Sort-Object Name) {
        $setName = "$($heroDir.Name)\$($animDir.Name)"
        $skel = @(Get-ChildItem -LiteralPath $animDir.FullName -Filter "*.skel.bytes" -File)
        if ($skel.Count -ne 1) {
            $problems.Add("${setName}: expected exactly one .skel.bytes (skel=$($skel.Count))")
            continue
        }

        $baseName = $skel[0].Name -replace "\.skel\.bytes$", ""
        $pngName = "$baseName.png"
        Add-SpineSet -SetName $setName `
            -SourceFullPath $animDir.FullName `
            -SourceRelative "$unityRelative\$($heroDir.Name)\Skeleton\$($animDir.Name)" `
            -PngPath (Join-Path $heroDir.FullName "Outfits\A\$pngName") `
            -PngSourceRelative "$unityRelative\$($heroDir.Name)\Outfits\A\$pngName"
    }

    $effectsRoot = Join-Path $heroDir.FullName "Effects"
    if (-not (Test-Path -LiteralPath $effectsRoot)) {
        continue
    }

    foreach ($fxDir in Get-ChildItem -LiteralPath $effectsRoot -Directory | Sort-Object Name) {
        $setName = "$($heroDir.Name)\$($fxDir.Name)"
        $fxSkel = @(Get-ChildItem -LiteralPath $fxDir.FullName -Filter "*.skel.bytes" -File)
        if ($fxSkel.Count -ne 1) {
            $problems.Add("${setName}: expected exactly one .skel.bytes (skel=$($fxSkel.Count))")
            continue
        }

        $fxBase = $fxSkel[0].Name -replace "\.skel\.bytes$", ""
        $fxPng = "$fxBase.png"
        $fxSource = "$unityRelative\$($heroDir.Name)\Effects\$($fxDir.Name)"
        Add-SpineSet -SetName $setName `
            -SourceFullPath $fxDir.FullName `
            -SourceRelative $fxSource `
            -PngPath (Join-Path $fxDir.FullName $fxPng) `
            -PngSourceRelative "$fxSource\$fxPng"
    }
}

if ($problems.Count -gt 0) {
    Write-Output "Incomplete spine sets:"
    $problems | ForEach-Object { Write-Output "  $_" }
    exit 1
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("<Project>")
$lines.Add("")
$lines.Add("  <!-- Generated by tools\spine-demo-content.ps1 - do not edit by hand. Links every hero animation set")
$lines.Add("       (skeleton, atlas and outfit A page texture) from unity\Assets\Sprites\Heroes into")
$lines.Add("       Content\Spine\<Hero>\<Anim>\ so the spine demo can switch characters and animations. -->")
$lines.Add("  <ItemGroup>")
foreach ($entry in $entries) {
    $lines.Add($entry)
}
$lines.Add("  </ItemGroup>")
$lines.Add("")
$lines.Add("</Project>")

[IO.File]::WriteAllLines($outputPath, $lines, [Text.UTF8Encoding]::new($false))
Write-Output "Written ${outputPath}: heroes=$heroCount sets=$($entries.Count / 3) entries=$($entries.Count)"
