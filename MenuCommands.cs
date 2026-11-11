using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using CTDynamicModMenu.Commands;
using FSAutoQuest.Events;
using FSAutoQuest.Vats;
using UnityEngine;

namespace FSAutoQuest.Menu
{
    [BepInPlugin("com.halil.fsautoquest.menu", "FSAutoQuest - Turkce Mod Menu & Dugmeler", "0.4.0")]
    [BepInDependency("com.halil.fsautoquest")]
    [BepInDependency("Toemmsen96.CTDynamicModMenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("toemmsen.FSCheats", BepInDependency.DependencyFlags.SoftDependency)]
    public class MenuPlugin : BaseUnityPlugin
    {
        private AutoQuestToggleCommand _toggle;
        private AutoReturnToggleCommand _autoReturn;
        private InstantReturnToggleCommand _instantReturn;
        private MaxLevelDwellersCommand _maxLevel;
        private LanguageToggleCommand _langToggle;
        private BypassQuestRequirementsCommand _bypassQuestReqs;
        private AutoKillEnemiesToggleCommand _autoKill;
        private AutoCollectRewardsToggleCommand _autoCollect;
        private VatsToggleBoostCommand _vatsBoost;
        private VatsToggleWarpCommand _vatsWarp;
        private VatsSetMultiplierCommand _vatsMult;
        private VatsSetAnimSpeedCommand _vatsAnim;
        private VatsAutoPauseIncidentsCommand _vatsPauseIncidents;
        private VatsAutoPauseStrangerCommand _vatsPauseStranger;
        private VatsToggleOverlayCommand _vatsOverlay;

        // Olay Yoneticisi (Event Manager)
        private EventStopAllCommand _eventStopAll;
        private EventTriggerRaidersCommand _eventRaiders;
        private EventTriggerDeathclawsCommand _eventDeathclaws;
        private EventTriggerAliensCommand _eventAliens;
        private EventTriggerGhoulsCommand _eventGhouls;
        private EventTriggerSynthsCommand _eventSynths;
        private EventTriggerEnclaveCommand _eventEnclave;
        private EventTriggerRadscorpionsCommand _eventRadscorpions;
        private EventTriggerMoleratsCommand _eventMolerats;
        private EventTriggerRadroachesCommand _eventRadroaches;
        private EventTriggerFireCommand _eventFire;
        private EventSpawnStrangerCommand _eventStranger;
        private EventSpawnBottleAndCappyCommand _eventBottleAndCappy;
        private EventAutoDefendToggleCommand _eventAutoDefend;
        private EventAutoTapVisitorsToggleCommand _eventAutoTapVisitors;
        private bool _registered;
        private bool _translated;
        private float _nextTry;
        private bool _gaveUpLogged;

        internal static bool IsTurkish = true;
        internal static BepInEx.Configuration.ConfigEntry<bool> CfgLanguageIsTurkish;
        internal static MenuPlugin Instance;
        internal static BepInEx.Logging.ManualLogSource Log;
        private static FieldInfo _fPersist = null;
        internal static void SetPersistentSettings(global::CTDynamicModMenu.CTDynamicModMenu menu, bool val)
        {
            if (menu == null) return;
            try
            {
                if (_fPersist == null)
                    _fPersist = typeof(global::CTDynamicModMenu.CTDynamicModMenu).GetField("persistentSettings", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (_fPersist != null) _fPersist.SetValue(menu, val);
            }
            catch { }
        }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            try
            {
                CfgLanguageIsTurkish = Config.Bind("Genel", "DilTurkce", true, "Mod menusu dili Turkce mi?");
                IsTurkish = CfgLanguageIsTurkish.Value;
            }
            catch { }
            try
            {
                FalloutThemeManager.Initialize(Paths.ConfigPath);
                var harmony = new HarmonyLib.Harmony("com.halil.fsautoquest.theme");
                harmony.PatchAll(typeof(MenuPlugin).Assembly);
                try
                {
                    var patchesType = Type.GetType("FSCheat.Patches, FSCheat");
                    if (patchesType != null)
                    {
                        var targetMethod = patchesType.GetMethod("CyDecryptPostfix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        var prefixMethod = typeof(MenuPlugin).GetMethod("CyDecryptPostfixPrefix", BindingFlags.Static | BindingFlags.NonPublic);
                        if (targetMethod != null && prefixMethod != null)
                        {
                            harmony.Patch(targetMethod, new HarmonyLib.HarmonyMethod(prefixMethod));
                            Logger.LogInfo("FSCheat save log spam engellendi (Performans optimize edildi).");
                        }
                    }
                }
                catch { }
                Logger.LogInfo("Fallout Mod Menu temalari ve Harmony yamasi yuklendi.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Tema/Harmony yukleme hatasi: " + ex.Message);
            }

            _toggle = new AutoQuestToggleCommand();
            _autoReturn = new AutoReturnToggleCommand();
            _instantReturn = new InstantReturnToggleCommand();
            _maxLevel = new MaxLevelDwellersCommand();
            _langToggle = new LanguageToggleCommand();
            _bypassQuestReqs = new BypassQuestRequirementsCommand();
            _autoKill = new AutoKillEnemiesToggleCommand();
            _autoCollect = new AutoCollectRewardsToggleCommand();
            try
            {
                VatsController.Initialize(Config);
                _vatsBoost = new VatsToggleBoostCommand();
                _vatsWarp = new VatsToggleWarpCommand();
                _vatsMult = new VatsSetMultiplierCommand();
                _vatsAnim = new VatsSetAnimSpeedCommand();
                _vatsPauseIncidents = new VatsAutoPauseIncidentsCommand();
                _vatsPauseStranger = new VatsAutoPauseStrangerCommand();
                _vatsOverlay = new VatsToggleOverlayCommand();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("VATS baslatma hatasi: " + ex.Message);
            }

            try
            {
                EventManagerController.Initialize(Config);
                _eventStopAll = new EventStopAllCommand();
                _eventRaiders = new EventTriggerRaidersCommand();
                _eventDeathclaws = new EventTriggerDeathclawsCommand();
                _eventAliens = new EventTriggerAliensCommand();
                _eventGhouls = new EventTriggerGhoulsCommand();
                _eventSynths = new EventTriggerSynthsCommand();
                _eventEnclave = new EventTriggerEnclaveCommand();
                _eventRadscorpions = new EventTriggerRadscorpionsCommand();
                _eventMolerats = new EventTriggerMoleratsCommand();
                _eventRadroaches = new EventTriggerRadroachesCommand();
                _eventFire = new EventTriggerFireCommand();
                _eventStranger = new EventSpawnStrangerCommand();
                _eventBottleAndCappy = new EventSpawnBottleAndCappyCommand();
                _eventAutoDefend = new EventAutoDefendToggleCommand();
                _eventAutoTapVisitors = new EventAutoTapVisitorsToggleCommand();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Event Manager baslatma hatasi: " + ex.Message);
            }
            TryRegister();
        }

        private void Update()
        {
            try { VatsController.TickUpdate(); } catch { }
            try { EventManagerController.TickUpdate(); } catch { }
            if (global::CTDynamicModMenu.CTDynamicModMenu.Instance != null) { SetPersistentSettings(global::CTDynamicModMenu.CTDynamicModMenu.Instance, true); }
            if (_registered && _translated) return;
            if (Time.unscaledTime < _nextTry) return;
            _nextTry = Time.unscaledTime + 1f;
            TryRegister();
        }

        private void TryRegister()
        {
            try
            {
                var menu = global::CTDynamicModMenu.CTDynamicModMenu.Instance;
                if (menu == null)
                {
                    if (!_gaveUpLogged)
                    {
                        _gaveUpLogged = true;
                        Logger.LogInfo("Mod menu henuz hazir degil, bekleniyor.");
                    }
                    return;
                }

                if (!_registered)
                {
                    menu.RegisterCommand(_toggle);
                    menu.RegisterCommand(_autoReturn);
                    menu.RegisterCommand(_instantReturn);
                    menu.RegisterCommand(_maxLevel);
                    menu.RegisterCommand(_langToggle);
                    menu.RegisterCommand(_bypassQuestReqs);
                    menu.RegisterCommand(_autoKill);
                    menu.RegisterCommand(_autoCollect);
                    if (_vatsBoost != null) menu.RegisterCommand(_vatsBoost);
                    if (_vatsWarp != null) menu.RegisterCommand(_vatsWarp);
                    if (_vatsMult != null) menu.RegisterCommand(_vatsMult);
                    if (_vatsAnim != null) menu.RegisterCommand(_vatsAnim);
                    if (_vatsPauseIncidents != null) menu.RegisterCommand(_vatsPauseIncidents);
                    if (_vatsPauseStranger != null) menu.RegisterCommand(_vatsPauseStranger);
                    if (_vatsOverlay != null) menu.RegisterCommand(_vatsOverlay);

                    if (_eventStopAll != null) menu.RegisterCommand(_eventStopAll);
                    if (_eventRaiders != null) menu.RegisterCommand(_eventRaiders);
                    if (_eventDeathclaws != null) menu.RegisterCommand(_eventDeathclaws);
                    if (_eventAliens != null) menu.RegisterCommand(_eventAliens);
                    if (_eventGhouls != null) menu.RegisterCommand(_eventGhouls);
                    if (_eventSynths != null) menu.RegisterCommand(_eventSynths);
                    if (_eventEnclave != null) menu.RegisterCommand(_eventEnclave);
                    if (_eventRadscorpions != null) menu.RegisterCommand(_eventRadscorpions);
                    if (_eventMolerats != null) menu.RegisterCommand(_eventMolerats);
                    if (_eventRadroaches != null) menu.RegisterCommand(_eventRadroaches);
                    if (_eventFire != null) menu.RegisterCommand(_eventFire);
                    if (_eventStranger != null) menu.RegisterCommand(_eventStranger);
                    if (_eventBottleAndCappy != null) menu.RegisterCommand(_eventBottleAndCappy);
                    if (_eventAutoDefend != null) menu.RegisterCommand(_eventAutoDefend);
                    if (_eventAutoTapVisitors != null) menu.RegisterCommand(_eventAutoTapVisitors);
                    _registered = true;
                    Logger.LogInfo("FSAutoQuest ve Event Manager komutlari mod menuye kaydedildi.");
                }

                if (!_translated)
                {
                    ApplyTranslation(menu);
                    _translated = true;
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning("Mod menu kaydi/cevirisi sirasinda hata: " + e.GetType().Name + " " + e.Message);
            }
        }

        internal static void ApplyTranslation(global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            try
            {
                var field = typeof(global::CTDynamicModMenu.CTDynamicModMenu).GetField("registeredCommands",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) return;

                var list = field.GetValue(menu) as List<CustomCommand>;
                if (list == null || list.Count == 0) return;

                if (menu != null) { SetPersistentSettings(menu, true); }
                for (int i = 0; i < list.Count; i++)
                {
                    var cmd = list[i];
                    CustomCommand realCmd = (cmd is LocalizedCommand loc) ? loc.Inner : cmd;

                    // Yapilandirmayi geri yukle (Ingilizce orijinal anahtarla)
                    if (realCmd != null && realCmd.IsToggle && menu != null && menu.Config != null)
                    {
                        try
                        {
                            string key = realCmd.Name + ": IsEnabled";
                            var entry = menu.Config.Bind<bool>("Command Settings", key, realCmd.IsEnabled, "Whether the command is enabled");
                            if (entry != null && entry.Value != realCmd.IsEnabled)
                            {
                                realCmd.IsEnabled = entry.Value;
                            }
                            if (cmd is LocalizedCommand locCmd)
                            {
                                locCmd.IsEnabled = realCmd.IsEnabled;
                            }

                            // Eger hile kayitli olarak aciksa motor icinde calisir duruma getir
                            if (realCmd.IsEnabled)
                            {
                                try { realCmd.Execute(new CommandInput()); } catch { }
                            }
                        }
                        catch { }
                    }

                    if (!(cmd is LocalizedCommand))
                    {
                        list[i] = new LocalizedCommand(cmd);
                    }
                }

                // Baslangic kategorisi Settings ise ve Turkce aktifse Ayarlar yap
                var catField = typeof(global::CTDynamicModMenu.CTDynamicModMenu).GetField("selectedCategory",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (catField != null)
                {
                    string curCat = catField.GetValue(menu) as string;
                    if ((curCat == "Settings" || string.IsNullOrEmpty(curCat)) && IsTurkish)
                    {
                        catField.SetValue(menu, "Ayarlar");
                    }
                }

                if (Instance != null)
                {
                    Instance.Logger.LogInfo("Mod menudeki " + list.Count + " komut basariyla Turkcelestirildi.");
                }
            }
            catch (Exception ex)
            {
                if (Instance != null)
                    Instance.Logger.LogWarning("ApplyTranslation hatasi: " + ex.Message);
            }
        }

        internal static void ToggleLanguage()
        {
            IsTurkish = !IsTurkish;
            if (CfgLanguageIsTurkish != null)
            {
                CfgLanguageIsTurkish.Value = IsTurkish;
                if (Instance != null && Instance.Config != null) { try { Instance.Config.Save(); } catch { } }
            }
            var menu = global::CTDynamicModMenu.CTDynamicModMenu.Instance;
            if (menu == null) return;

            var catField = typeof(global::CTDynamicModMenu.CTDynamicModMenu).GetField("selectedCategory",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (catField != null)
            {
                string curCat = catField.GetValue(menu) as string;
                if (IsTurkish)
                {
                    string tr = Localization.TranslateCategory(curCat);
                    catField.SetValue(menu, tr);
                }
                else
                {
                    string en = Localization.GetEnglishCategory(curCat);
                    catField.SetValue(menu, en);
                }
            }

            if (Instance != null)
            {
                Instance.Logger.LogInfo("Mod menu dili degistirildi: " + (IsTurkish ? "Turkce" : "English"));
            }
        }

        private static bool CyDecryptPostfixPrefix(ref string passphrase)
        {
            try
            {
                var patchesType = Type.GetType("FSCheat.Patches, FSCheat");
                if (patchesType != null)
                {
                    var field = patchesType.GetField("decryptPassphrase", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        field.SetValue(null, passphrase);
                    }
                }
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    /// Herhangi bir CustomCommand'i Turkce dil destegi ile saran proxy sinifi.
    /// </summary>
    internal class LocalizedCommand : CustomCommand
    {
        private readonly CustomCommand _inner;
        private readonly bool _isFsCheatMaxLevel;

        public CustomCommand Inner { get { return _inner; } }

        public LocalizedCommand(CustomCommand inner)
        {
            _inner = inner;
            _isFsCheatMaxLevel = inner != null && inner.GetType().FullName.Contains("MaxLevelDwellers") && !(inner is MaxLevelDwellersCommand);
            if (inner != null)
            {
                base.IsEnabled = inner.IsEnabled;
                base.Keybind = inner.Keybind;
                base.RequireControlKey = inner.RequireControlKey;
                base.RequireAltKey = inner.RequireAltKey;
                base.RequireShiftKey = inner.RequireShiftKey;
            }
        }

        public override string Name
        {
            get
            {
                if (!MenuPlugin.IsTurkish)
                {
                    if (_isFsCheatMaxLevel) return "Max Level Dwellers (Safe)";
                    return _inner.Name;
                }
                if (_isFsCheatMaxLevel) return "Sakinleri Max Seviye Yap (FSCheat - Guvenli)";
                return Localization.TranslateName(_inner.Name);
            }
        }

        public override string Category
        {
            get
            {
                if (!MenuPlugin.IsTurkish) return Localization.GetEnglishCategory(_inner.Category);
                return Localization.TranslateCategory(_inner.Category);
            }
        }

        public override string Description
        {
            get
            {
                if (!MenuPlugin.IsTurkish)
                {
                    if (_isFsCheatMaxLevel) return "Safely sets all dwellers to maximum (50) level without crashing.";
                    return _inner.Description;
                }
                if (_isFsCheatMaxLevel) return "Tum sakinleri guvenli sekilde maksimum (50) seviyeye cikarir (Cokme yapmaz).";
                return Localization.TranslateDescription(_inner.Name, _inner.Description);
            }
        }

        public override string Format { get { return _inner.Format; } }
        public override string AlternativeFormat { get { return _inner.AlternativeFormat; } }

        public override bool IsToggle { get { return _inner.IsToggle; } }
        public override bool HasConfig { get { return _inner.HasConfig; } }
        public override bool PersistConfig { get { return _inner.PersistConfig; } }

        public new void SaveConfig()
        {
            if (_inner != null)
            {
                _inner.SaveConfig();
            }
            else
            {
                base.SaveConfig();
            }
        }

        public override bool IsEnabled
        {
            get { return _inner.IsEnabled; }
            set
            {
                _inner.IsEnabled = value;
                base.IsEnabled = value;
            }
        }

        public override KeyCode? Keybind
        {
            get { return _inner.Keybind; }
            set
            {
                _inner.Keybind = value;
                base.Keybind = value;
            }
        }

        public override bool RequireControlKey
        {
            get { return _inner.RequireControlKey; }
            set
            {
                _inner.RequireControlKey = value;
                base.RequireControlKey = value;
            }
        }

        public override bool RequireAltKey
        {
            get { return _inner.RequireAltKey; }
            set
            {
                _inner.RequireAltKey = value;
                base.RequireAltKey = value;
            }
        }

        public override bool RequireShiftKey
        {
            get { return _inner.RequireShiftKey; }
            set
            {
                _inner.RequireShiftKey = value;
                base.RequireShiftKey = value;
            }
        }

        public override void Execute(CommandInput message)
        {
            // FSCheat'in coken butonu tiklanirsa kendi guvenli metodumuzu calistir
            if (_isFsCheatMaxLevel)
            {
                AutoQuestPlugin.QueueMaxLevelDwellers();
                return;
            }
            _inner.Execute(message);
        }

        public override void LoadCustomConfig() { _inner.LoadCustomConfig(); }
        public override void SaveCustomConfig() { _inner.SaveCustomConfig(); }
    }

    /// <summary>
    /// Dil degistirme butonu.
    /// </summary>
    internal class LanguageToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Dil: Turkce (Degistir - EN)" : "Language: English (Switch - TR)"; }
        }

        public override string Description
        {
            get { return MenuPlugin.IsTurkish ? "Mod menusu dilini Turkce ve Ingilizce arasinda degistirir." : "Switches mod menu language between Turkish and English."; }
        }

        public override string Format { get { return "/dil"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Ayarlar" : "Settings"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }

        public override void Execute(CommandInput message)
        {
            MenuPlugin.ToggleLanguage();
        }
    }

    /// <summary>Otomasyonu ac/kapa. Menude kirmizi = kapali, yesil = acik.</summary>
    internal class AutoQuestToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "Auto Quest"; }
        }
        public override string Description
        {
            get { return MenuPlugin.IsTurkish ? "Gorevleri arka planda otomatik tamamlar, esya toplar ve vaulta doner." : "Automatically completes quests in background."; }
        }
        public override string Format { get { return "/otomatikgorev"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get { return AutoQuestPlugin.IsEnabled; }
            set { AutoQuestPlugin.SetEnabled(value); }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    /// <summary>Gorev tamamlandiginda otomatik kasaya donmeyi kontrol eder.</summary>
    internal class AutoReturnToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Otomatik Kasaya Don" : "Auto Return to Vault"; }
        }
        public override string Description
        {
            get
            {
                return MenuPlugin.IsTurkish
                    ? "Gorev bittiginde onay beklemeden kasaya doner. Kapaliysa 'Return to Vault' butonunu tiklamanizi bekler."
                    : "Automatically returns to vault when quest is done. If off, waits for you to click 'Return to Vault'.";
            }
        }
        public override string Format { get { return "/otomatikkasayadon"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get { return AutoQuestPlugin.IsAutoReturnEnabled; }
            set { AutoQuestPlugin.SetAutoReturnEnabled(value); }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    /// <summary>Gorev bittiginde beklemeden ekibi aninda kapiya ulastirir.</summary>
    internal class InstantReturnToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Aninda Kapiya Ulas" : "Instant Return to Vault"; }
        }
        public override string Description
        {
            get
            {
                return MenuPlugin.IsTurkish
                    ? "Gorev bittiginde saatlerce yurume suresini beklemeden ekibi aninda siginak kapisina ulastirir."
                    : "Instantly brings your quest team to the vault door without waiting for wasteland travel time.";
            }
        }
        public override string Format { get { return "/anindakapiyaulas"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get { return AutoQuestPlugin.IsInstantReturnEnabled; }
            set { AutoQuestPlugin.SetInstantReturnEnabled(value); }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    /// <summary>Gorevlerin seviye, silah, kiyafet ve SPECIAL sartlarini kaldirir.</summary>
        /// <summary>Gorevdeki tum dusmanlari otomatik oldurur (Dost/diyalog NPC'lerine dokunmaz).</summary>
        /// <summary>Gorev ve kesif sonrasi cikan tarif parcasi ve odul pencerelerini otomatik toplar.</summary>
    internal class AutoCollectRewardsToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Tarif / Odul Otomatik Topla" : "Auto-Collect Rewards & Recipes"; }
        }
        public override string Description
        {
            get
            {
                return MenuPlugin.IsTurkish
                    ? "Gorev veya kesif sonrasi cikan tarif parcasi (Recipe Fragment), esya ve kutlama pencerelerini otomatik toplayip kapatir."
                    : "Automatically claims and dismisses post-quest recipe fragment, loot, and reward popup windows.";
            }
        }
        public override string Format { get { return "/autocollect"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get { return AutoQuestPlugin.IsAutoCollectRewardsEnabled; }
            set { AutoQuestPlugin.SetAutoCollectRewardsEnabled(value); }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    internal class AutoKillEnemiesToggleCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Dusmanlari Otomatik Oldur" : "Auto-Kill Quest Enemies"; }
        }
        public override string Description
        {
            get
            {
                return MenuPlugin.IsTurkish
                    ? "Gorevdeki tum dusman NPC'leri (yagmacilar, olum penceleri, akrepler vb.) otomatik olarak aninda oldurur. Dost ve diyalog NPC'lerine asla zarar vermez."
                    : "Instantly defeats all hostile enemy creatures (raiders, deathclaws, scorpions, etc.) in the quest. Never harms friendly or dialogue NPCs.";
            }
        }
        public override string Format { get { return "/autokill"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get { return AutoQuestPlugin.IsAutoKillEnemiesEnabled; }
            set { AutoQuestPlugin.SetAutoKillEnemiesEnabled(value); }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    internal class BypassQuestRequirementsCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Gorev Sartlarini Kaldir" : "Bypass Quest Requirements"; }
        }
        public override string Description
        {
            get
            {
                return MenuPlugin.IsTurkish
                    ? "Seviye, silah, kiyafet ve SPECIAL sartlari olmadan tum gorevleri baslatmanizi saglar."
                    : "Allows starting any quest without meeting level, weapon, outfit, or SPECIAL requirements.";
            }
        }
        public override string Format { get { return "/bypassquestreqs"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Otomatik Gorev" : "FSAutoQuest"; } }

        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }

        public override bool IsEnabled
        {
            get
            {
                return AutoQuestPlugin.CfgBypassQuestReqs != null && AutoQuestPlugin.CfgBypassQuestReqs.Value;
            }
            set
            {
                if (AutoQuestPlugin.CfgBypassQuestReqs != null)
                {
                    AutoQuestPlugin.CfgBypassQuestReqs.Value = value;
                }
            }
        }

        public override void Execute(CommandInput message)
        {
        }
    }

    /// <summary>Sakinleri tek tek max seviyeye cikarir (takilma yapmadan).</summary>
    internal class MaxLevelDwellersCommand : CustomCommand
    {
        public override string Name
        {
            get { return MenuPlugin.IsTurkish ? "Sakinleri Max Seviye Yap" : "Max Level Dwellers"; }
        }
        public override string Description
        {
            get { return MenuPlugin.IsTurkish ? "Tum sakinleri donma veya hata olmadan 50. seviyeye cikarir." : "Safely sets all dwellers to max level without lag."; }
        }
        public override string Format { get { return "/maxseviye"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Sakinler" : "Dwellers"; } }

        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }

        public override void Execute(CommandInput message)
        {
            AutoQuestPlugin.QueueMaxLevelDwellers();
        }
    }

    /// <summary>Zaman hizlandirma (10x boost).</summary>
    internal class VatsToggleBoostCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Zaman Hizlandirma (10x Boost)" : "Accelerate Time (10x Boost)"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Oyun ici zamani (uretim, egitim, hamilelik, crafting ve corak topraklar) 10 kat hizlandirir."
            : "Speeds up game time (production, training, crafting, wasteland) by 10x."; } }
        public override string Format { get { return "/vatsboost"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return VatsController.IsBoostEnabled; }
            set { VatsController.IsBoostEnabled = value; }
        }
        public override void Execute(CommandInput message) { }
    }

    /// <summary>Zaman bukme (50x hiper hiz).</summary>
    internal class VatsToggleWarpCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Zaman Bukme (50x Isik Hizi)" : "Time Warp (50x Hyper Speed)"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Oyun ici zamani 50 kat maksimum hiza bukerek saatlik isleri saniyeler icinde bitirir."
            : "Warps game time at 50x speed, completing multi-hour tasks in seconds."; } }
        public override string Format { get { return "/vatswarp"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return VatsController.IsTimeWarpEnabled; }
            set { VatsController.IsTimeWarpEnabled = value; }
        }
        public override void Execute(CommandInput message) { }
    }

    /// <summary>Hiz carpanini ayarla.</summary>
    internal class VatsSetMultiplierCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Hiz Carpani Belirle" : "Set Speed Multiplier"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Zaman hizlandirma carpanini belirler (2x - 100x)."
            : "Sets custom time acceleration multiplier (2x - 100x)."; } }
        public override string Format { get { return "/vatsmult <multiplier>"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            if (message != null && message.Args != null && message.Args.Count > 0)
            {
                string raw = message.Args[0].Replace("x", "").Replace("X", "").Trim();
                float val;
                if (float.TryParse(raw, out val) && val > 0f)
                {
                    VatsController.SpeedMultiplier = Mathf.Clamp(val, 1f, 999f);
                }
            }
        }
    }

    /// <summary>Animasyon hizini ayarla.</summary>
    internal class VatsSetAnimSpeedCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Animasyon Hizini Belirle" : "Set Animation Speed"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Gorsel hareket ve animasyon akicilik hizini ayarlar (1x - 5x)."
            : "Sets visual movement and animation rendering speed (1x - 5x)."; } }
        public override string Format { get { return "/vatsanim <speed>"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            if (message != null && message.Args != null && message.Args.Count > 0)
            {
                string raw = message.Args[0].Replace("x", "").Replace("X", "").Trim();
                float val;
                if (float.TryParse(raw, out val) && val > 0f)
                {
                    VatsController.AnimationSpeed = Mathf.Clamp(val, 1f, 10f);
                }
            }
        }
    }

    /// <summary>Felaketlerde otomatik duraklat.</summary>
    internal class VatsAutoPauseIncidentsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Felaketlerde Duraklat" : "Auto-Pause on Incidents"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Yangin, akrep, olum pencesi veya akinci baskinlarinda zamani otomatik normale dondurur."
            : "Automatically pauses/slows boost during fires, radscorpions, deathclaws, or raiders."; } }
        public override string Format { get { return "/vatspauseincidents"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return VatsController.AutoPauseIncidents; }
            set { VatsController.AutoPauseIncidents = value; }
        }
        public override void Execute(CommandInput message) { }
    }

    /// <summary>Gizemli yabancida duraklat.</summary>
    internal class VatsAutoPauseStrangerCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Gizemli Yabancida Duraklat" : "Auto-Pause on Stranger"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Gizemli Yabanci belirdiginde zamani aninda duraklatir, kapaklari kacirmanizi onler."
            : "Pauses time acceleration when the Mysterious Stranger appears so you can tap him for caps."; } }
        public override string Format { get { return "/vatspausestranger"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return VatsController.AutoPauseStranger; }
            set { VatsController.AutoPauseStranger = value; }
        }
        public override void Execute(CommandInput message) { }
    }

    /// <summary>Ekran bilgi gostergesi (Overlay).</summary>
    internal class VatsToggleOverlayCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Ekran Bilgi Efekti (Overlay)" : "Screen Overlay (V.A.T.S.)"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Hizlandirma etkinken ekran kenarlarindaki CRT tarama cizgilerini ve gecen sure sayacini gosterir/gizler."
            : "Toggles CRT static bands and elapsed time display while time is accelerated."; } }
        public override string Format { get { return "/vatsoverlay"; } }
        public override string Category { get { return "V.A.T.S."; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return VatsController.ShowOverlay; }
            set { VatsController.ShowOverlay = value; }
        }
        public override void Execute(CommandInput message) { }
    }

    // =========================================================================
    // Olay Yoneticisi (Event Manager) Komutlari
    // =========================================================================

    /// <summary>Tum yangin, istila ve saldiran dusmanlari aninda yok eder.</summary>
    internal class EventStopAllCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Tum Olaylari & Baskinlari Bitir" : "Stop All Incidents & Defeat Invaders"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasada devam eden tum yanginlari sondurur, istilalari temizler ve saldiran tum akinci, uzayli ve olum pencelerini aninda yok eder."
            : "Instantly extinguishes all fires, clears infestations, and eliminates all attacking raiders, deathclaws, or aliens."; } }
        public override string Format { get { return "/eventstopall"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.StopAllIncidents();
        }
    }

    /// <summary>Akinci baskini baslat.</summary>
    internal class EventTriggerRaidersCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Akinci Baskini Baslat" : "Trigger Raiders Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen bir Akinci (Raider) saldirisi baslatir."
            : "Immediately launches a Raider attack breaching the Vault door."; } }
        public override string Format { get { return "/eventraiders"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Raiders);
        }
    }

    /// <summary>Olum pencesi baskini baslat.</summary>
    internal class EventTriggerDeathclawsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Olum Pencesi Baskini Baslat" : "Trigger Deathclaws Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen vahsi bir Olum Pencesi (Deathclaw) saldirisi baslatir."
            : "Immediately launches a ferocious Deathclaw assault on the Vault door."; } }
        public override string Format { get { return "/eventdeathclaws"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Deathclaw);
        }
    }

    /// <summary>Uzayli baskini baslat.</summary>
    internal class EventTriggerAliensCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Uzayli Baskini Baslat" : "Trigger Aliens Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen lazerli bir Uzayli (Alien) saldirisi baslatir."
            : "Immediately launches an Alien invader attack on the Vault door."; } }
        public override string Format { get { return "/eventaliens"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Alien);
        }
    }

    /// <summary>Vahsi Ghoul baskini baslat.</summary>
    internal class EventTriggerGhoulsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Vahsi Ghoul Baskini Baslat" : "Trigger Feral Ghouls Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen hizli bir Vahsi Ghoul (Feral Ghoul) saldirisi baslatir."
            : "Immediately launches a Feral Ghoul raid on the Vault door."; } }
        public override string Format { get { return "/eventghouls"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Ghoul);
        }
    }

    /// <summary>Institute Synth baskini baslat.</summary>
    internal class EventTriggerSynthsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Synth Baskini Baslat" : "Trigger Synths Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen bir Institute Synth saldirisi baslatir."
            : "Immediately launches an Institute Synth infiltration attack."; } }
        public override string Format { get { return "/eventsynths"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Gen1Synth);
        }
    }

    /// <summary>Enclave saldirisi baslat.</summary>
    internal class EventTriggerEnclaveCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Enclave Baskini Baslat" : "Trigger Enclave Attack"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasaya hemen bir Enclave Askeri ve Guc Zirhi saldirisi baslatir."
            : "Immediately launches an Enclave Security and Power Armor attack."; } }
        public override string Format { get { return "/eventenclave"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.EnclavePowerArmor);
        }
    }

    /// <summary>Radyasyon akrebi istilasi baslat.</summary>
    internal class EventTriggerRadscorpionsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Radyasyon Akrebi Istilasi" : "Trigger Radscorpions Infestation"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasa odasinda hemen enerji emen bir Radyasyon Akrebi (Radscorpion) istilasi baslatir."
            : "Immediately spawns a burrowing Radscorpion infestation in a vault room."; } }
        public override string Format { get { return "/eventradscorpions"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Radscorpion);
        }
    }

    /// <summary>Kostebek faresi istilasi baslat.</summary>
    internal class EventTriggerMoleratsCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Kostebek Faresi Istilasi" : "Trigger Molerats Infestation"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasa odasinda hemen bir Kostebek Faresi (Molerat) istilasi baslatir."
            : "Immediately spawns a Molerat infestation in a vault room."; } }
        public override string Format { get { return "/eventmolerats"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Molerat);
        }
    }

    /// <summary>Hamambocegi istilasi baslat.</summary>
    internal class EventTriggerRadroachesCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Hamambocegi Istilasi" : "Trigger Radroaches Infestation"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasa odasinda hemen bir Radyasyon Hamambocegi (Radroach) istilasi baslatir."
            : "Immediately spawns a Radroach infestation in a vault room."; } }
        public override string Format { get { return "/eventradroaches"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Radroach);
        }
    }

    /// <summary>Yangin felaketi baslat.</summary>
    internal class EventTriggerFireCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Yangin Baslat" : "Trigger Vault Fire"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasa odasinda hemen bir yangin felaketi baslatir."
            : "Immediately ignites an emergency fire in a vault room."; } }
        public override string Format { get { return "/eventfire"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.TriggerEmergency(ERoomEmergencyMode.Fire);
        }
    }

    /// <summary>Gizemli yabanciyi cagir.</summary>
    internal class EventSpawnStrangerCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Gizemli Yabanciyi Cagir" : "Spawn Mysterious Stranger"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Gizemli Yabanciyi piyano sesiyle birlikte hemen kasada ortaya cikarir, tiklayarak ucretsiz kapak alabilirsiniz."
            : "Immediately spawns the Mysterious Stranger in the vault with piano chime, ready to be tapped for free caps."; } }
        public override string Format { get { return "/eventstranger"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.SpawnMysteriousStranger();
        }
    }

    /// <summary>Sise ve Kapakcigi cagir.</summary>
    internal class EventSpawnBottleAndCappyCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Sise ve Kapakcigi Cagir" : "Call Bottle & Cappy Visit"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Nuka-World maskotlari Bottle & Cappy'yi kasaya nese ve kapak/quantum getirmeleri icin cagirir."
            : "Immediately summons Nuka-World mascots Bottle & Cappy to dance and give caps/quantum."; } }
        public override string Format { get { return "/eventbottleandcappy"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return false; } }
        public override bool HasConfig { get { return false; } }
        public override void Execute(CommandInput message)
        {
            EventManagerController.SpawnBottleAndCappy();
        }
    }

    /// <summary>Felaket ve baskinlari otomatik savustur.</summary>
    internal class EventAutoDefendToggleCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Otomatik Olay Savusturma" : "Auto-Defend Incidents"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Kasada herhangi bir yangin, istila veya baskin basladiginda onu aninda otomatik bitirir."
            : "Automatically and instantly eliminates any fire, infestation, or invader raid the moment it starts."; } }
        public override string Format { get { return "/eventautodefend"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return EventManagerController.AutoDefendEnabled; }
            set { EventManagerController.AutoDefendEnabled = value; }
        }
        public override void Execute(CommandInput message)
        {
            IsEnabled = !IsEnabled;
            SaveConfig();
        }
    }

    /// <summary>Gizemli Yabanci ve maskotlari otomatik tikla.</summary>
    internal class EventAutoTapVisitorsToggleCommand : CustomCommand
    {
        public override string Name { get { return MenuPlugin.IsTurkish ? "Ziyaretcileri Otomatik Yakala" : "Auto-Tap Mysterious Stranger"; } }
        public override string Description { get { return MenuPlugin.IsTurkish
            ? "Gizemli Yabanci veya Bottle & Cappy belirdiginde onlari aninda tiklayarak ucretsiz kapak ve odulleri toplar."
            : "Automatically taps Mysterious Stranger and Bottle & Cappy the moment they appear for free caps and rewards."; } }
        public override string Format { get { return "/eventautotap"; } }
        public override string Category { get { return MenuPlugin.IsTurkish ? "Olay Yoneticisi" : "Events"; } }
        public override bool IsToggle { get { return true; } }
        public override bool HasConfig { get { return false; } }
        public override bool IsEnabled
        {
            get { return EventManagerController.AutoTapVisitorsEnabled; }
            set { EventManagerController.AutoTapVisitorsEnabled = value; }
        }
        public override void Execute(CommandInput message)
        {
            IsEnabled = !IsEnabled;
            SaveConfig();
        }
    }

    /// <summary>
    /// Turkce ve Ingilizce ceviri sozlukleri.
    /// </summary>
    internal static class Localization
    {
        private static readonly Dictionary<string, string> Categories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Dwellers", "Sakinler" },
            { "Rooms", "Odalar" },
            { "Resources", "Kaynaklar" },
            { "Recipes", "Tarifler" },
            { "Items", "Esyalar" },
            { "Pets", "Evcil Hayvanlar" },
            { "Weapons", "Silahlar" },
            { "Saves", "Kayitlar" },
            { "Premium Pass", "Sezon Bileti" },
            { "Settings", "Ayarlar" },
            { "Help", "Yardim" },
            { "FSAutoQuest", "Otomatik Gorev" },
            { "Themes", "Temalar" },
            { "V.A.T.S.", "V.A.T.S. (Zaman)" },
            { "Events", "Olay Yoneticisi" }
        };

        private static readonly Dictionary<string, string> ReverseCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Rooms
            { "Bypass Crafting Level Requirement", "Uretim Seviye Sartini Kaldir" },
            { "Bypass Room Build Requirement", "Oda Insa Nufus Sartini Kaldir" },
            { "Bypass Upgrade Dweller Requirement", "Oda Yukseltme Nufus Sartini Kaldir" },
            { "Bypass Quest Requirements", "Gorev Sartlarini Kaldir" },
            { "Auto Return to Vault", "Otomatik Kasaya Don" },
            { "Instant Return to Vault", "Aninda Kapiya Ulas" },
            // V.A.T.S. (Zaman)
            { "Accelerate Time (10x Boost)", "Zaman Hizlandirma (10x Boost)" },
            { "Time Warp (50x Hyper Speed)", "Zaman Bukme (50x Isik Hizi)" },
            { "Set Speed Multiplier", "Hiz Carpani Belirle" },
            { "Set Animation Speed", "Animasyon Hizini Belirle" },
            { "Auto-Pause on Incidents", "Felaketlerde Duraklat" },
            { "Auto-Pause on Stranger", "Gizemli Yabancida Duraklat" },
            { "Screen Overlay (V.A.T.S.)", "Ekran Bilgi Efekti (Overlay)" },
            { "No Rush Fail", "Hizlandirma Asla Basarisiz Olmasin" },
            { "Override Incident Chance", "Olay Ihtimalini Sabitle" },
            { "Set Incident Chance", "Olay Ihtimali Oranini Belirle" },

            // Event Manager (Olay Yoneticisi)
            { "Stop All Incidents & Defeat Invaders", "Tum Olaylari & Baskinlari Bitir" },
            { "Trigger Raiders Attack", "Akinci Baskini Baslat" },
            { "Trigger Deathclaws Attack", "Olum Pencesi Baskini Baslat" },
            { "Trigger Aliens Attack", "Uzayli Baskini Baslat" },
            { "Trigger Feral Ghouls Attack", "Vahsi Ghoul Baskini Baslat" },
            { "Trigger Synths Attack", "Synth Baskini Baslat" },
            { "Trigger Enclave Attack", "Enclave Baskini Baslat" },
            { "Trigger Radscorpions Infestation", "Radyasyon Akrebi Istilasi" },
            { "Trigger Molerats Infestation", "Kostebek Faresi Istilasi" },
            { "Trigger Radroaches Infestation", "Hamambocegi Istilasi" },
            { "Trigger Vault Fire", "Yangin Baslat" },
            { "Spawn Mysterious Stranger", "Gizemli Yabanciyi Cagir" },
            { "Call Bottle & Cappy Visit", "Sise ve Kapakcigi Cagir" },
            { "Auto-Defend Incidents", "Otomatik Olay Savusturma" },
            { "Auto-Tap Mysterious Stranger", "Ziyaretcileri Otomatik Yakala" },

            // Dwellers
            { "Create Random Custom Dweller", "Rastgele Ozel Sakin Olustur" },
            { "Create Random Dweller", "Rastgele Sakin Olustur" },
            { "Create Random Legendary Dweller", "Rastgele Efsanevi Sakin Olustur" },
            { "Create Random Unique Dweller", "Rastgele Benzersiz Sakin Olustur" },
            { "Finish all Trainings", "Tum Egitimleri Tamamla" },
            { "Instant Adult Cheat", "Aninda Yetiskin Yap" },
            { "Instant Baby Cheat", "Aninda Dogum Yap" },
            { "Max Happy Dwellers", "Tum Sakinleri %100 Mutlu Yap" },
            { "Max Level Dwellers", "Sakinleri Max Seviye Yap" },
            { "Max Special Levels", "Tum SPECIAL Niteliklerini Fulleyin" },
            { "Override Max Dwellers", "Maksimum Sakin Sinirini Kaldir" },
            { "Reset Dweller Levels", "Sakin Seviyelerini 1'e Sifirla" },

            // Resources
            { "Max Resources", "Maksimum Kaynaklar (Sonsuz)" },
            { "Set Resources", "Kaynak Miktarini Belirle" },
            { "Override Storage Limit Cheat", "Depolama Sinirini Kaldir" },
            { "Set Storage Limit", "Depolama Sinirini Belirle" },
            { "Set Nuka Caps", "Nuka Kapak (Caps) Ekle" },
            { "Set Nuka Quantum", "Nuka Quantum Ekle" },
            { "Set Lunchboxes", "Lunchbox (Kutu) Ekle" },
            { "Set MrHandy Boxes", "Mr. Handy Kutusu Ekle" },
            { "Set PetBoxes", "Evcil Hayvan Kutusu Ekle" },
            { "Set Starter Pack Boxes", "Baslangic Paketi Kutusu Ekle" },
            { "Set Stimpacks and Radaways", "Stimpack ve Radaway Ekle" },
            { "Set Curie Boxes", "Curie Kutusu Ekle" },
            { "Set Victor Boxes", "Victor Kutusu Ekle" },
            { "Set Predefined Pack Boxes", "Ozel Paket Kutulari Ekle" },
            { "Set Spin Resources (Pokerchip etc.)", "Sezon Cark Kaynagi (Poker Fisi) Ekle" },
            { "Set Ultracite", "Ultracite Miktari Belirle" },
            { "Set Nuka Quantum Boxes", "Nuka Quantum Kutusu Ekle" },

            // Recipes
            { "Free Crafting", "Ucretsiz Uretim (Malzemesiz)" },
            { "Infinite Crafting Nuka", "Sonsuz Uretim Nukasi" },
            { "Unlock All Recipes Cheat", "Tum Uretim Tariflerini Ac" },

            // Items
            { "Full Scrap Return", "Tam Hurda Iadesi" },
            { "Override Item Limit Cheat", "Esya Envanter Sinirini Kaldir" },
            { "Set Item Limit", "Esya Envanter Sinirini Belirle" },

            // Pets
            { "Overpowered Pets", "Asiri Guclu Evcil Hayvanlar (x1000)" },
            { "Override Pet Limit Cheat", "Evcil Hayvan Sinirini Kaldir" },

            // Weapons
            { "Override Weapon DMG", "Tum Silah Hasarlarini 100 Yap" },

            // Saves
            { "Decrypt Game Saves", "Kayit Dosyasinin Sifresini Coz (.json)" },
            { "Encrypt Game Saves", "Kayit Dosyasini Geri Sifrele" },

            // Premium Pass
            { "Override Premium Plus Pass", "Premium Plus Sezon Biletini Ac" },

            // Settings
            { "Toggle Log Window", "Log Penceresini Ac/Kapat" },
            { "Toggle Menu Button", "Sol Menu Dugmesini Ac/Kapat" },
            { "Toggle Keybind Text", "Kisayol Tus Metnini Ac/Kapat" },
            { "ToggleButtonKeybindText", "Buton Kisayol Metnini Ac/Kapat" },
            { "BindKeyForCommand", "Komuta Kisayol Tusu Ata" },
            { "ListCommands", "Tum Komutlari Listele" },
            { "SetCommandWindow", "Komut Satirini Ac/Kapat" },
            { "SetPersistance", "Ayarlarin Kaliciligini Ac/Kapat" }
        };

        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bypass Crafting Level Requirement", "Oda seviyesi yetmese bile Rare ve Efsanevi esyalari uretmenizi saglar." },
            { "Bypass Room Build Requirement", "Odalari acmak icin gereken sakin sayisi sartini kaldirir." },
            { "Bypass Upgrade Dweller Requirement", "Oda seviyesini yukseltmek icin gereken sakin sayisi sinirini kaldirir." },
            { "Bypass Quest Requirements", "Seviye, silah, kiyafet ve SPECIAL sartlari olmadan tum gorevleri baslatmanizi saglar." },
            { "Auto Return to Vault", "Gorev bittiginde onay beklemeden kasaya doner. Kapaliysa 'Return to Vault' butonunu tiklamanizi bekler." },
            { "Instant Return to Vault", "Gorev bittiginde saatlerce yurume suresini beklemeden ekibi aninda siginak kapisina ulastirir." },
            // V.A.T.S. (Zaman)
            { "Accelerate Time (10x Boost)", "Oyun ici zamani (uretim, egitim, hamilelik, crafting ve corak topraklar) 10 kat hizlandirir." },
            { "Time Warp (50x Hyper Speed)", "Oyun ici zamani 50 kat maksimum hiza bukerek saatlik isleri saniyeler icinde bitirir." },
            { "Set Speed Multiplier", "Zaman hizlandirma carpanini belirler (2x - 100x)." },
            { "Set Animation Speed", "Gorsel hareket ve animasyon akicilik hizini ayarlar (1x - 5x)." },
            { "Auto-Pause on Incidents", "Yangin, akrep, olum pencesi veya akinci baskinlarinda zamani otomatik normale dondurur." },
            { "Auto-Pause on Stranger", "Gizemli Yabanci belirdiginde zamani aninda duraklatir, kapaklari kacirmanizi onler." },
            { "Screen Overlay (V.A.T.S.)", "Hizlandirma etkinken ekran kenarlarindaki CRT tarama cizgilerini ve gecen sure sayacini gosterir/gizler." },
            { "No Rush Fail", "Oda hizlandirmalarinda (Rush) yangin, akrep veya felaket cikmasini engeller." },
            { "Override Incident Chance", "Rastgele olaylarin (yangin, baskin vb.) cikma ihtimalini sabitler." },
            { "Set Incident Chance", "Olay ihtimali yuzdesini (0-100) ayarlar (/overrideincidentchance ile kullanilir)." },
            { "Create Random Custom Dweller", "Siginak kapisi onunde rastgele ozel bir sakin olusturur." },
            { "Create Random Dweller", "Siginak kapisi onunde rastgele bir sakin olusturur." },
            { "Create Random Legendary Dweller", "Siginak kapisi onunde rastgele Efsanevi (Legendary) bir sakin olusturur." },
            { "Create Random Unique Dweller", "Siginak kapisi onunde rastgele Benzersiz (Unique) bir sakin olusturur." },
            { "Finish all Trainings", "Tum sakinlerin devam eden spor/egitim salonu calismalarini aninda bitirir." },
            { "Instant Adult Cheat", "Cocuk sakinlerin aninda yetiskine donusmesini saglar." },
            { "Instant Baby Cheat", "Hamile sakinlerin aninda bebek dogurmasini saglar." },
            { "Max Happy Dwellers", "Siginaktaki tum sakinlerin mutluluk seviyesini %100 yapar." },
            { "Max Level Dwellers", "Tum sakinleri guvenli sekilde maksimum (50) seviyeye cikarir." },
            { "Max Special Levels", "Tum sakinlerin S.P.E.C.I.A.L. istatistiklerini maksimum 10 yapar." },
            { "Override Max Dwellers", "Siginaktaki 200 sakin sinirini asmayi saglar." },
            { "Reset Dweller Levels", "Tum sakinlerin seviyesini 1 yapar (Maksimum can antrenmani icin)." },
            { "Max Resources", "Elektrik, su ve gida kaynaklarini surekli maksimumda tutar." },
            { "Set Resources", "Elektrik, su ve gida miktarini istediginiz degere ayarlar." },
            { "Override Storage Limit Cheat", "Kaynaklar icin depolama sinirini kaldirir." },
            { "Set Storage Limit", "Depolama kapasite sinirini belirlenen degere ayarlar." },
            { "Set Nuka Caps", "Kapak miktarini belirlenen sayiya ayarlar." },
            { "Set Nuka Quantum", "Nuka-Cola Quantum miktarini belirlenen sayiya ayarlar." },
            { "Set Lunchboxes", "Istediginiz sayida Lunchbox verir." },
            { "Set MrHandy Boxes", "Istediginiz sayida Mr. Handy robot kutusu verir." },
            { "Set PetBoxes", "Istediginiz sayida Evcil Hayvan kutusu verir." },
            { "Set Starter Pack Boxes", "Istediginiz sayida Baslangic Paketi kutusu verir." },
            { "Set Stimpacks and Radaways", "Stimpack ve Radaway miktarini belirlenen sayiya ayarlar." },
            { "Set Curie Boxes", "Istediginiz sayida Curie kutusu verir." },
            { "Set Victor Boxes", "Istediginiz sayida Victor kutusu verir." },
            { "Set Predefined Pack Boxes", "Istediginiz sayida Ozel Paket kutulari verir." },
            { "Set Spin Resources (Pokerchip etc.)", "Sezonluk cark kaynaklarinin miktarini ayarlar." },
            { "Set Ultracite", "Ultracite kaynagini belirlenen sayiya ayarlar." },
            { "Free Crafting", "Tum uretim gereksinimlerini kaldirir (Malzeme ve Nuka harcamaz)." },
            { "Infinite Crafting Nuka", "Uretim yaparken Nuka harcanmaz (yalnizca hurda malzemeler gerekir)." },
            { "Unlock All Recipes Cheat", "Oyundaki tum silah, kiyafet ve tema uretim tariflerini acar." },
            { "Full Scrap Return", "Esya parcaladiginizda rastgele parca yerine tarifin tum hurda bilesenlerini geri verir." },
            { "Override Item Limit Cheat", "Siginak envanterindeki maksimum esya tasima limitini kaldirir." },
            { "Set Item Limit", "Envanter esya limitini belirlenen degere ayarlar." },
            { "Overpowered Pets", "Evcil hayvan bonus etkilerini 1000 katina cikarir (Hasar, can, hiz vb.)." },
            { "Override Pet Limit Cheat", "Siginaktaki evcil hayvan sinirini kaldirir." },
            { "Override Weapon DMG", "Oyundaki tum silahlarin hasarini 100 olarak ayarlar." },
            { "Decrypt Game Saves", "Oyun kayit dosyalarini sifresiz JSON formatina donusturup kaydeder." },
            { "Encrypt Game Saves", "Duzenlenmis sifresiz kayit dosyasini oyunda calisacak sekilde geri sifreler." },
            { "Override Premium Plus Pass", "Sezonluk Premium Plus biletini ucretsiz olarak aktif eder." },
            { "Toggle Log Window", "Oyun ici log penceresini gosterir veya gizler." },
            { "Toggle Menu Button", "Ekranin solundaki F4 menu acma butonunu gosterir veya gizler." },
            { "Toggle Keybind Text", "Butonlarin yanindaki kisayol tuslarini gosterir veya gizler." },
            { "ToggleButtonKeybindText", "Buton metinlerinde kisayol bilgisini gosterir veya gizler." },
            { "BindKeyForCommand", "Herhangi bir hile butonuna klavyeden kisayol tusu atar." },
            { "ListCommands", "Konsolda kullanilabilecek tum komutlari listeler." },
            { "SetCommandWindow", "Metin tabanli hile komut satirini acar." },
            { "SetPersistance", "Ayarlarin Kaliciligini Ac/Kapat" },

            // Event Manager (Olay Yoneticisi)
            { "Stop All Incidents & Defeat Invaders", "Kasada devam eden tum yanginlari sondurur, istilalari temizler ve saldiran tum akinci, uzayli ve olum pencelerini aninda yok eder." },
            { "Trigger Raiders Attack", "Kasaya hemen bir Akinci (Raider) saldirisi baslatir." },
            { "Trigger Deathclaws Attack", "Kasaya hemen vahsi bir Olum Pencesi (Deathclaw) saldirisi baslatir." },
            { "Trigger Aliens Attack", "Kasaya hemen lazerli bir Uzayli (Alien) saldirisi baslatir." },
            { "Trigger Feral Ghouls Attack", "Kasaya hemen hizli bir Vahsi Ghoul (Feral Ghoul) saldirisi baslatir." },
            { "Trigger Synths Attack", "Kasaya hemen bir Institute Synth saldirisi baslatir." },
            { "Trigger Enclave Attack", "Kasaya hemen bir Enclave Askeri ve Guc Zirhi saldirisi baslatir." },
            { "Trigger Radscorpions Infestation", "Kasa odasinda hemen enerji emen bir Radyasyon Akrebi (Radscorpion) istilasi baslatir." },
            { "Trigger Molerats Infestation", "Kasa odasinda hemen bir Kostebek Faresi (Molerat) istilasi baslatir." },
            { "Trigger Radroaches Infestation", "Kasa odasinda hemen bir Radyasyon Hamambocegi (Radroach) istilasi baslatir." },
            { "Trigger Vault Fire", "Kasa odasinda hemen bir yangin felaketi baslatir." },
            { "Spawn Mysterious Stranger", "Gizemli Yabanciyi piyano sesiyle birlikte hemen kasada ortaya cikarir, tiklayarak ucretsiz kapak alabilirsiniz." },
            { "Call Bottle & Cappy Visit", "Nuka-World maskotlari Bottle & Cappy'yi kasaya nese ve kapak/quantum getirmeleri icin cagirir." },
            { "Auto-Defend Incidents", "Kasada herhangi bir yangin, istila veya baskin basladiginda onu aninda otomatik bitirir." },
            { "Auto-Tap Mysterious Stranger", "Gizemli Yabanci veya Bottle & Cappy belirdiginde onlari aninda tiklayarak ucretsiz kapak ve odulleri toplar." }
        };

        static Localization()
        {
            foreach (var kv in Categories)
            {
                ReverseCategories[kv.Value] = kv.Key;
            }
            ReverseCategories["Yardım"] = "Help";
            ReverseCategories["Yardim"] = "Help";
            ReverseCategories["Eşyalar"] = "Items";
            ReverseCategories["Esyalar"] = "Items";
            ReverseCategories["Kayıtlar"] = "Saves";
            ReverseCategories["Kayitlar"] = "Saves";
            ReverseCategories["Otomatik Görev"] = "FSAutoQuest";
            ReverseCategories["Otomatik Gorev"] = "FSAutoQuest";
            ReverseCategories["Temalar"] = "Themes";
            ReverseCategories["Olay Yöneticisi"] = "Events";
            ReverseCategories["Olay Yoneticisi"] = "Events";
        }

        public static string TranslateCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return category;
            string val;
            if (Categories.TryGetValue(category, out val)) return val;
            return category;
        }

        public static string GetEnglishCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return category;
            string val;
            if (ReverseCategories.TryGetValue(category, out val)) return val;
            return category;
        }

        public static string TranslateName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string val;
            if (Names.TryGetValue(name, out val)) return val;
            return name;
        }

        public static string TranslateDescription(string name, string fallback)
        {
            if (string.IsNullOrEmpty(name)) return fallback;
            string val;
            if (Descriptions.TryGetValue(name, out val)) return val;
            return fallback;
        }
    }
}
