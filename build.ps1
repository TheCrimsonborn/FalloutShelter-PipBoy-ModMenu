param(
    [switch]$Install
)

$ErrorActionPreference = "Stop"

$game    = "C:\Program Files (x86)\Steam\steamapps\common\Fallout Shelter"
$managed = Join-Path $game "FalloutShelter_Data\Managed"
$plugins = Join-Path $game "BepInEx\plugins"
$csc     = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"

$unityRefs = @(Get-ChildItem $managed -Filter "UnityEngine*.dll" | ForEach-Object { $_.FullName })

function Invoke-Compile {
    param(
        [string[]]$Sources,
        [string[]]$References,
        [string]$Output
    )

    $args = @("/nologo", "/target:library", "/optimize+", "/out:$Output", "/nowarn:0618")
    foreach ($r in $References) { $args += "/r:$r" }
    foreach ($s in $Sources) { $args += $s }

    & $csc @args
    if ($LASTEXITCODE -ne 0) { throw "Derleme basarisiz: $Output (exit $LASTEXITCODE)" }

    $item = Get-Item $Output
    "Derlendi: $($item.Name) ($($item.Length) bayt)"
}

# ---------------------------------------------------------------- 1) ana eklenti
$mainDll  = Join-Path $PSScriptRoot "FSAutoQuest.dll"
$mainRefs = @(
    (Join-Path $game "BepInEx\core\BepInEx.dll"),
    (Join-Path $managed "Unity.InputSystem.dll"),
    (Join-Path $managed "netstandard.dll")
) + $unityRefs

Invoke-Compile -Sources @(
    (Join-Path $PSScriptRoot "ProbePlugin.cs"),
    (Join-Path $PSScriptRoot "AutoQuestPlugin.cs")
) -References $mainRefs -Output $mainDll

# ---------------------------------------------------------------- 2) mod menu adaptoru & temalar
$menuDll  = Join-Path $PSScriptRoot "FSAutoQuestMenu.dll"
$menuRefs = @(
    (Join-Path $game "BepInEx\core\BepInEx.dll"),
    (Join-Path $game "BepInEx\core\0Harmony.dll"),
    (Join-Path $managed "netstandard.dll"),
    (Join-Path $managed "Assembly-CSharp.dll"),
    $mainDll,
    (Join-Path $plugins "CTDynamicModMenu.dll")
) + $unityRefs

Invoke-Compile -Sources @(
    (Join-Path $PSScriptRoot "MenuCommands.cs"),
    (Join-Path $PSScriptRoot "FalloutThemes.cs"),
    (Join-Path $PSScriptRoot "QuestRequirementsPatch.cs")
) -References $menuRefs -Output $menuDll

# ---------------------------------------------------------------- 3) kurulum
if ($Install) {
    Copy-Item -LiteralPath $mainDll -Destination (Join-Path $plugins 'FSAutoQuest.dll') -Force
    Copy-Item -LiteralPath $menuDll -Destination (Join-Path $plugins 'FSAutoQuestMenu.dll') -Force
    "Kuruldu: $plugins"
}
