param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$gameDir    = "C:\Program Files (x86)\Steam\steamapps\common\Fallout Shelter"
$scriptDir  = $PSScriptRoot
$releaseDir = Join-Path $scriptDir "release"

Write-Host ">>> 1. En son DLL'ler derleniyor..." -ForegroundColor Cyan
& (Join-Path $scriptDir "build.ps1")

# Release klasorunu temizle
if (Test-Path $releaseDir) {
    Get-ChildItem $releaseDir -Filter "*.zip" | Remove-Item -Force
} else {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}

$tempFull    = Join-Path $releaseDir "temp_full"
$tempPlugins = Join-Path $releaseDir "temp_plugins"

New-Item -ItemType Directory -Path (Join-Path $tempFull "BepInEx\core") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $tempFull "BepInEx\plugins") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $tempPlugins "BepInEx\plugins") -Force | Out-Null

Write-Host ">>> 2. Dosyalar hazirlaniyor..." -ForegroundColor Cyan

# BepInEx Core
Copy-Item (Join-Path $gameDir "BepInEx\core\*") (Join-Path $tempFull "BepInEx\core") -Recurse -Force

# Injector & Config
Copy-Item (Join-Path $gameDir "winhttp.dll") (Join-Path $tempFull "winhttp.dll") -Force
Copy-Item (Join-Path $gameDir "doorstop_config.ini") (Join-Path $tempFull "doorstop_config.ini") -Force
if (Test-Path (Join-Path $gameDir "BepInEx\config")) {
    Copy-Item (Join-Path $gameDir "BepInEx\config") (Join-Path $tempFull "BepInEx") -Recurse -Force
}
if (Test-Path (Join-Path $gameDir ".doorstop_version")) {
    Copy-Item (Join-Path $gameDir ".doorstop_version") (Join-Path $tempFull ".doorstop_version") -Force
}

# Plugins
$pluginDlls = @("CTDynamicModMenu.dll", "FSCheat.dll", "FSAutoQuest.dll", "FSAutoQuestMenu.dll", "VATS.dll")
foreach ($dll in $pluginDlls) {
    $srcLocal = Join-Path $scriptDir $dll
    $srcGame = Join-Path $gameDir "BepInEx\plugins\$dll"
    $srcVats = Join-Path $scriptDir "..\Vault Accelerated Time System\$dll"
    $src = if (Test-Path $srcLocal) { $srcLocal } elseif (Test-Path $srcVats) { $srcVats } else { $srcGame }
    if (Test-Path $src) {
        Copy-Item $src (Join-Path $tempFull "BepInEx\plugins\$dll") -Force
        Copy-Item $src (Join-Path $tempPlugins "BepInEx\plugins\$dll") -Force
    }
}

# Kilavuz dosyalari olustur
$readmeTr = @"
=======================================================================
   FALLOUT SHELTER - PIP-BOY TERMINAL MOD MENU & ULTIMATE CHEAT v$Version
=======================================================================
Gelistirici / Modder: Halil & Antigravity
Temel Mod Menusu Destegi: Toemmsen96 (CTDynamicModMenu & FSCheats)

-----------------------------------------------------------------------
[ 1. OZELLIKLER ]
-----------------------------------------------------------------------
- 10 OZEL FALLOUT TEMASI:
  * Brotherhood of Steel (BoS) [Taktik Turuncu/Celik]
  * The Institute [Siber Neon Turkuaz/Obsidyen]
  * Minutemen [Milis Mavisi/Altin Sari]
  * The Railroad [Golge Grikoyu/Fener Sarisi]
  * Party / Nuka-World [Neon Pembe/Kuantum Mavi]
  * Vault 33 [Otantik Vault-Tec Lacivert/Sari]
  * Halloween [Korkunc Balkabagi Turuncusu/Zombi Yesili]
  * Christmas [Yilbasi Cami Yesili/Elma Sekeri Kirmizisi]
  * Thanksgiving [Sonbahar Kizili/Misir Sarisi]
  * New Vegas [Mr. House / Lucky 38 Altin Amber]

- CIFTE DIL DESTEGI (TR / EN):
  * Tek tikla aninda Turkce <-> Ingilizce degisimi (Sag ust buton).
  * 47'den fazla hile, tum kategoriler ve aciklamalar tamamen Turkcelestirildi.

- COKMEYEN GUCLENDIRILMIS SAKIN SISTEMI (Safe Max Level Dwellers):
  * Orijinal FSCheat'in 20-30 sakinden sonra oyunu donduran / cokerten
    hatasi giderildi. 200 sakin bile olsa takilmadan guvenle 50. seviyeye cikarir.

- OTOMATIK GOREV SISTEMI (Auto Quest):
  * Arka planda gorevleri otomatik tamamlar, esyalari toplar ve siginaga doner.

- GOREV SARTLARINI KALDIRMA (Bypass Quest Requirements):
  * Gorevlerin istedigi sakin seviyesi (ornegin Lv. 45), ozel silahlar (Gauss Rifle vb.),
    kiyafetler ve SPECIAL nitelik gereksinimlerini tamamen baypas ederek her goreve aninda katilmanizi saglar.

- V.A.T.S. ZAMAN HIZLANDIRMA SISTEMI (Vault Accelerated Time System):
  * Oyunun dahili zaman motorunu (TimeMgr ve Time.timeScale) 10x veya 50x hizlandirir.
  * Uretim, egitim, hamilelikler, uretim atolyeleri ve corak toprak kesiflerini isik hizinda tamamlar.
  * Akilli Otomatik Duraklatma (Smart Auto-Pause): Siginakta yangin/akrep/yagmaci felaketi ciktiginda veya Gizemli Yabanci belirdiginde zamani otomatik normale alir.
  * CRT Ekran Efekti (Overlay): Gecen oyun suresini ve hiz carpanini gosteren retro bant efekti.
  * Hem Mod Menusunden butonlarla, hem de kisayol tuslariyla (Ctrl+Alt, Shift+Alt vb.) tam uyumlu ve senkronize calisir.

- PIP-BOY TERMINAL TASARIMI:
  * Yari saydam CRT terminal tasarimi, dinamik fare vurgulari (hover),
    parlayan kenarliklar, ekran boyutu olceklendirme (slider) ve cok satirli
    disari tasmayan sekmeler.
  * Sol ustte dweller sayacini kapatmayan ozel acma dugmesi.

-----------------------------------------------------------------------
[ 2. KURULUM ]
-----------------------------------------------------------------------
1. Fallout Shelter oyununuzun kurulu oldugu ana klasoru acin:
   (Steam icin varsayilan: C:\Program Files (x86)\Steam\steamapps\common\Fallout Shelter)
2. Bu ZIP arsivinin icindeki TUM DOSYA VE KLASORLERI (BepInEx, winhttp.dll, doorstop_config.ini vb.)
   dogrudan Fallout Shelter ana klasorune surukleyip birakin.
3. Oyunu Steam uzerinden normal sekilde baslatin.

-----------------------------------------------------------------------
[ 3. KULLANIM ]
-----------------------------------------------------------------------
- F4 Tusu: Mod menusunu acar ve kapatir.
- Ekranin sol ustundeki [ ⚙ MOD MENUSU (F4) ] dugmesine tiklayarak da acabilirsiniz.
- Sag ustteki [DIL: TR / LANG: EN] dugmesiyle dili istediginiz an degistirebilirsiniz.
- Basligin altindaki [◄] ve [►] oklariyla 10 tema arasinda gezinebilirsiniz.
- Alt kısımdaki 'Olcek' cubugu ile menuyu ekran cozunurlugunuze gore buyutup kucultebilirsiniz.

-----------------------------------------------------------------------
[ 4. KALDIRMA ]
-----------------------------------------------------------------------
Modu kaldirmak isterseniz oyun klasorundeki su dosyalari silmeniz yeterlidir:
- BepInEx (klasor)
- winhttp.dll
- doorstop_config.ini
- .doorstop_version
Oyununuz tamamen orijinal haline donecektir.
"@

$readmeEn = @"
=======================================================================
   FALLOUT SHELTER - PIP-BOY TERMINAL MOD MENU & ULTIMATE CHEAT v$Version
=======================================================================
Author / Modder: Halil & Antigravity
Base Framework: Toemmsen96 (CTDynamicModMenu & FSCheats)

-----------------------------------------------------------------------
[ 1. FEATURES ]
-----------------------------------------------------------------------
- 10 AUTHENTIC FALLOUT FACTION & HOLIDAY THEMES:
  * Brotherhood of Steel (BoS) [Tactical Amber/Steel]
  * The Institute [Cyberpunk Cyan/Obsidian]
  * Minutemen [Colonial Navy/Freedom Gold]
  * The Railroad [Stealth Grey/Lantern Amber]
  * Party / Nuka-World [Neon Pink/Quantum Cyan]
  * Vault 33 [Classic Vault-Tec Blue/Yellow]
  * Halloween [Spooky Orange/Toxic Green]
  * Christmas [Pine Green/Candy Apple Red]
  * Thanksgiving [Autumn Hearth/Maize Gold]
  * New Vegas [Mr. House Lucky 38 Amber]

- BILINGUAL (ENGLISH & TURKISH):
  * One-click instant language switch (Upper-right button).
  * 47+ cheats, full categories, and tooltips localized in both EN and TR.

- CRASH-FREE DWELLER LEVELING (Safe Max Level Dwellers):
  * Fixes the notorious FSCheat freeze/crash when leveling large vaults.
  * Safely processes up to 200 dwellers smoothly to level 50 with zero lag.

- AUTO QUEST AUTOMATION:
  * Automatically completes wasteland quests in background, loots, and returns.

- BYPASS QUEST REQUIREMENTS:
  * Allows starting any wasteland quest without meeting dweller level requirements (e.g. Lv. 45),
    specific weapons (Gauss Rifle etc.), outfits, or SPECIAL stat thresholds.

- V.A.T.S. TIME ACCELERATION ENGINE (Vault Accelerated Time System):
  * Accelerates internal game clock and animations (10x boost or 50x hyper warp).
  * Speeds up dweller training, room production, pregnancies, crafting, and wasteland exploration.
  * Smart Auto-Pause: Automatically resets to normal vanilla speed during vault incidents (fires, scorpions, raiders) and when the Mysterious Stranger appears.
  * Retro CRT Overlay: Displays real-time game clock elapsed counter and active multiplier.
  * Works simultaneously via Mod Menu buttons or hotkeys (Ctrl+Alt, Shift+Alt, Left Ctrl) with synchronized state.

- CRT PIP-BOY TERMINAL DESIGN:
  * Semi-transparent terminal aesthetic, dynamic interactive hover glows,
    scale slider, and multi-row non-overflowing category tabs.
  * Sleek toggle button below the dweller HUD icon.

-----------------------------------------------------------------------
[ 2. INSTALLATION ]
-----------------------------------------------------------------------
1. Open your Fallout Shelter root game directory:
   (Steam default: C:\Program Files (x86)\Steam\steamapps\common\Fallout Shelter)
2. Extract ALL contents of this ZIP (BepInEx folder, winhttp.dll, doorstop_config.ini, etc.)
   directly into your Fallout Shelter game directory.
3. Launch the game normally via Steam.

-----------------------------------------------------------------------
[ 3. CONTROLS ]
-----------------------------------------------------------------------
- F4 Key: Opens and closes the mod menu.
- Or click the [ ⚙ MOD MENU (F4) ] button below the dweller count icon.
- Click [LANG: EN / DIL: TR] in top right to switch language.
- Use [◄] and [►] arrows to switch between all 10 themes.
- Adjust UI scale using the slider at the bottom.

-----------------------------------------------------------------------
[ 4. UNINSTALLATION ]
-----------------------------------------------------------------------
Simply delete the following from your Fallout Shelter game folder:
- BepInEx (folder)
- winhttp.dll
- doorstop_config.ini
- .doorstop_version
"@

$readmeTrPathFull = Join-Path $tempFull "BENIOKU_KURULUM.txt"
$readmeEnPathFull = Join-Path $tempFull "README_INSTALL.txt"
Set-Content -Path $readmeTrPathFull -Value $readmeTr -Encoding UTF8
Set-Content -Path $readmeEnPathFull -Value $readmeEn -Encoding UTF8

$readmeTrPathPlug = Join-Path $tempPlugins "BENIOKU_KURULUM.txt"
$readmeEnPathPlug = Join-Path $tempPlugins "README_INSTALL.txt"
Set-Content -Path $readmeTrPathPlug -Value $readmeTr -Encoding UTF8
Set-Content -Path $readmeEnPathPlug -Value $readmeEn -Encoding UTF8

Write-Host ">>> 3. ZIP Arsivleri olusturuluyor..." -ForegroundColor Cyan

$zipFull    = Join-Path $releaseDir "FalloutShelter_PipBoyModMenu_FullPack_v$Version.zip"
$zipPlugins = Join-Path $releaseDir "FalloutShelter_PipBoyModMenu_PluginsOnly_v$Version.zip"

if (Test-Path $zipFull) { Remove-Item $zipFull -Force }
if (Test-Path $zipPlugins) { Remove-Item $zipPlugins -Force }

Compress-Archive -Path (Join-Path $tempFull "*") -DestinationPath $zipFull -CompressionLevel Optimal
Compress-Archive -Path (Join-Path $tempPlugins "*") -DestinationPath $zipPlugins -CompressionLevel Optimal

# Gecici klasorleri temizle
Remove-Item $tempFull -Recurse -Force
Remove-Item $tempPlugins -Recurse -Force

Write-Host ""
Write-Host ">>> BASARILI! Release paketleri olusturuldu:" -ForegroundColor Green
Get-ChildItem $releaseDir -Filter "*.zip" | ForEach-Object {
    Write-Host ("  [+] " + $_.Name + " (" + [math]::Round($_.Length / 1MB, 2) + " MB)") -ForegroundColor Yellow
}
