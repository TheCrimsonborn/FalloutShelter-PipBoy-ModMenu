using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using CTDynamicModMenu.Commands;
using UnityEngine;

namespace FSAutoQuest.Menu
{
    [BepInPlugin("com.halil.fsautoquest.menu", "FSAutoQuest - Turkce Mod Menu & Dugmeler", "0.3.0")]
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
        private bool _registered;
        private bool _translated;
        private float _nextTry;
        private bool _gaveUpLogged;

        internal static bool IsTurkish = true;
        internal static MenuPlugin Instance;
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
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
            TryRegister();
        }

        private void Update()
        {
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
                    _registered = true;
                    Logger.LogInfo("FSAutoQuest komutlari mod menuye kaydedildi.");
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

                for (int i = 0; i < list.Count; i++)
                {
                    var cmd = list[i];
                    if (cmd is LocalizedCommand) continue;

                    list[i] = new LocalizedCommand(cmd);
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
            { "Themes", "Temalar" }
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
            { "No Rush Fail", "Hizlandirma Asla Basarisiz Olmasin" },
            { "Override Incident Chance", "Olay Ihtimalini Sabitle" },
            { "Set Incident Chance", "Olay Ihtimali Oranini Belirle" },

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
            { "SetPersistance", "Ayarlarin Kaliciligini Ac/Kapat" }
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
