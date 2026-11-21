using FSAutoQuest.Vats;
using FSAutoQuest.Events;

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using CTDynamicModMenu.Commands;

namespace FSAutoQuest.Menu
{
    public class FalloutTheme
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string HeaderTag { get; set; }
        public string MottoTr { get; set; }
        public string MottoEn { get; set; }
        public string Motto => MenuPlugin.IsTurkish ? MottoTr : MottoEn;
        public Color BgColor { get; set; }
        public Color HeaderBgColor { get; set; }
        public Color BorderColor { get; set; }
        public Color AccentColor { get; set; }
        public Color TextColor { get; set; }
        public Color ButtonBg { get; set; }
        public Color ButtonHover { get; set; }
        public Color ToggleOn { get; set; }
        public Color ToggleOff { get; set; }
        public Color ActiveTab { get; set; }
        public Color InactiveTab { get; set; }

        private string _accentHex;
        private string _toggleOnHex;
        private string _toggleOffHex;
        public string AccentHex => _accentHex ?? (_accentHex = ColorUtility.ToHtmlStringRGB(AccentColor));
        public string ToggleOnHex => _toggleOnHex ?? (_toggleOnHex = ColorUtility.ToHtmlStringRGB(ToggleOn));
        public string ToggleOffHex => _toggleOffHex ?? (_toggleOffHex = ColorUtility.ToHtmlStringRGB(ToggleOff));
    }

    public static class FalloutThemeManager
    {
        public static readonly FalloutTheme[] Themes = new FalloutTheme[]
        {
            // 1. Brotherhood of Steel (BoS)
            new FalloutTheme
            {
                Id = "BoS",
                DisplayName = "Brotherhood of Steel (BoS)",
                HeaderTag = "[ B.O.S. // TACTICAL COMMAND TERMINAL ]",
                MottoTr = "Ad Victoriam // Çelik Kardeşliği",
                MottoEn = "Ad Victoriam // Brotherhood of Steel",
                BgColor = new Color(0.08f, 0.09f, 0.11f, 0.88f),
                HeaderBgColor = new Color(0.14f, 0.11f, 0.08f, 0.95f),
                BorderColor = new Color(0.95f, 0.55f, 0.08f, 0.95f),
                AccentColor = new Color(1.0f, 0.65f, 0.15f, 1.0f),
                TextColor = new Color(0.92f, 0.88f, 0.80f, 1.0f),
                ButtonBg = new Color(0.14f, 0.16f, 0.18f, 0.85f),
                ButtonHover = new Color(0.26f, 0.22f, 0.16f, 0.95f),
                ToggleOn = new Color(1.0f, 0.70f, 0.10f, 1.0f),
                ToggleOff = new Color(0.65f, 0.35f, 0.25f, 0.9f),
                ActiveTab = new Color(0.35f, 0.22f, 0.10f, 0.95f),
                InactiveTab = new Color(0.12f, 0.13f, 0.15f, 0.80f)
            },
            // 2. Institute
            new FalloutTheme
            {
                Id = "Institute",
                DisplayName = "Institute",
                HeaderTag = "[ THE INSTITUTE // SYNTH CONTROL CONSOLE ]",
                MottoTr = "Mankind - Redefined // Enstitü",
                MottoEn = "Mankind - Redefined // The Institute",
                BgColor = new Color(0.04f, 0.07f, 0.10f, 0.88f),
                HeaderBgColor = new Color(0.06f, 0.12f, 0.16f, 0.95f),
                BorderColor = new Color(0.0f, 0.90f, 1.0f, 0.95f),
                AccentColor = new Color(0.2f, 0.95f, 1.0f, 1.0f),
                TextColor = new Color(0.90f, 0.98f, 1.0f, 1.0f),
                ButtonBg = new Color(0.08f, 0.14f, 0.18f, 0.85f),
                ButtonHover = new Color(0.12f, 0.25f, 0.32f, 0.95f),
                ToggleOn = new Color(0.0f, 1.0f, 0.85f, 1.0f),
                ToggleOff = new Color(0.35f, 0.45f, 0.55f, 0.9f),
                ActiveTab = new Color(0.10f, 0.26f, 0.34f, 0.95f),
                InactiveTab = new Color(0.06f, 0.10f, 0.13f, 0.80f)
            },
            // 3. Minutemen
            new FalloutTheme
            {
                Id = "Minutemen",
                DisplayName = "Minutemen",
                HeaderTag = "[ MINUTEMEN // FREEDOM RADIO TERMINAL ]",
                MottoTr = "Ready at a minute's notice // Milis Kuvvetleri",
                MottoEn = "Ready at a minute's notice // Minutemen",
                BgColor = new Color(0.05f, 0.08f, 0.14f, 0.88f),
                HeaderBgColor = new Color(0.08f, 0.14f, 0.22f, 0.95f),
                BorderColor = new Color(0.25f, 0.65f, 0.98f, 0.95f),
                AccentColor = new Color(1.0f, 0.84f, 0.25f, 1.0f),
                TextColor = new Color(0.85f, 0.92f, 0.98f, 1.0f),
                ButtonBg = new Color(0.09f, 0.14f, 0.22f, 0.85f),
                ButtonHover = new Color(0.15f, 0.24f, 0.38f, 0.95f),
                ToggleOn = new Color(0.30f, 0.75f, 1.0f, 1.0f),
                ToggleOff = new Color(0.40f, 0.45f, 0.55f, 0.9f),
                ActiveTab = new Color(0.16f, 0.28f, 0.42f, 0.95f),
                InactiveTab = new Color(0.07f, 0.11f, 0.17f, 0.80f)
            },
            // 4. Railroad
            new FalloutTheme
            {
                Id = "Railroad",
                DisplayName = "Railroad",
                HeaderTag = "[ THE RAILROAD // SHADOW HEADQUARTERS ]",
                MottoTr = "Follow the Freedom Trail // Yeraltı Demiryolu",
                MottoEn = "Follow the Freedom Trail // The Railroad",
                BgColor = new Color(0.07f, 0.07f, 0.08f, 0.88f),
                HeaderBgColor = new Color(0.12f, 0.11f, 0.11f, 0.95f),
                BorderColor = new Color(0.75f, 0.75f, 0.75f, 0.95f),
                AccentColor = new Color(1.0f, 0.65f, 0.20f, 1.0f),
                TextColor = new Color(0.92f, 0.92f, 0.92f, 1.0f),
                ButtonBg = new Color(0.13f, 0.13f, 0.15f, 0.85f),
                ButtonHover = new Color(0.24f, 0.20f, 0.18f, 0.95f),
                ToggleOn = new Color(1.0f, 0.72f, 0.25f, 1.0f),
                ToggleOff = new Color(0.55f, 0.30f, 0.30f, 0.9f),
                ActiveTab = new Color(0.28f, 0.24f, 0.20f, 0.95f),
                InactiveTab = new Color(0.10f, 0.10f, 0.12f, 0.80f)
            },
            // 5. Party
            new FalloutTheme
            {
                Id = "Party",
                DisplayName = "Party",
                HeaderTag = "[ NUKA-WORLD // CELEBRATION PARTY MODE ]",
                MottoTr = "Party in the Wasteland! // Nuka Festivali",
                MottoEn = "Party in the Wasteland! // Nuka-World Party",
                BgColor = new Color(0.11f, 0.04f, 0.16f, 0.88f),
                HeaderBgColor = new Color(0.20f, 0.06f, 0.26f, 0.95f),
                BorderColor = new Color(1.0f, 0.10f, 0.65f, 0.95f),
                AccentColor = new Color(0.0f, 1.0f, 0.95f, 1.0f),
                TextColor = new Color(1.0f, 0.95f, 0.40f, 1.0f),
                ButtonBg = new Color(0.18f, 0.08f, 0.25f, 0.85f),
                ButtonHover = new Color(0.32f, 0.12f, 0.42f, 0.95f),
                ToggleOn = new Color(0.0f, 1.0f, 0.85f, 1.0f),
                ToggleOff = new Color(0.85f, 0.20f, 0.50f, 0.9f),
                ActiveTab = new Color(0.40f, 0.12f, 0.48f, 0.95f),
                InactiveTab = new Color(0.14f, 0.06f, 0.20f, 0.80f)
            },
            // 6. Vault 33
            new FalloutTheme
            {
                Id = "Vault33",
                DisplayName = "Vault 33",
                HeaderTag = "[ VAULT-TEC // VAULT 33 OVERSEER CONSOLE ]",
                MottoTr = "A Brighter Future Underground // Vault 33",
                MottoEn = "A Brighter Future Underground // Vault 33",
                BgColor = new Color(0.04f, 0.09f, 0.18f, 0.88f),
                HeaderBgColor = new Color(0.06f, 0.14f, 0.26f, 0.95f),
                BorderColor = new Color(1.0f, 0.80f, 0.0f, 0.95f),
                AccentColor = new Color(1.0f, 0.85f, 0.0f, 1.0f),
                TextColor = new Color(1.0f, 0.92f, 0.50f, 1.0f),
                ButtonBg = new Color(0.06f, 0.14f, 0.26f, 0.85f),
                ButtonHover = new Color(0.12f, 0.24f, 0.42f, 0.95f),
                ToggleOn = new Color(1.0f, 0.85f, 0.0f, 1.0f),
                ToggleOff = new Color(0.20f, 0.35f, 0.55f, 0.9f),
                ActiveTab = new Color(0.14f, 0.28f, 0.48f, 0.95f),
                InactiveTab = new Color(0.05f, 0.10f, 0.20f, 0.80f)
            },
            // 7. Halloween
            new FalloutTheme
            {
                Id = "Halloween",
                DisplayName = "Halloween",
                HeaderTag = "[ SPOOKY WASTELAND // HALLOWEEN SPECIAL ]",
                MottoTr = "Trick or Treat in the Wasteland // Korkunç Sığınak",
                MottoEn = "Trick or Treat in the Wasteland // Spooky Shelter",
                BgColor = new Color(0.09f, 0.04f, 0.12f, 0.88f),
                HeaderBgColor = new Color(0.18f, 0.07f, 0.14f, 0.95f),
                BorderColor = new Color(1.0f, 0.40f, 0.05f, 0.95f),
                AccentColor = new Color(0.50f, 1.0f, 0.10f, 1.0f),
                TextColor = new Color(1.0f, 0.65f, 0.25f, 1.0f),
                ButtonBg = new Color(0.16f, 0.08f, 0.18f, 0.85f),
                ButtonHover = new Color(0.28f, 0.14f, 0.24f, 0.95f),
                ToggleOn = new Color(0.45f, 1.0f, 0.10f, 1.0f),
                ToggleOff = new Color(0.70f, 0.25f, 0.10f, 0.9f),
                ActiveTab = new Color(0.35f, 0.15f, 0.10f, 0.95f),
                InactiveTab = new Color(0.12f, 0.05f, 0.15f, 0.80f)
            },
            // 8. Christmas
            new FalloutTheme
            {
                Id = "Christmas",
                DisplayName = "Christmas",
                HeaderTag = "[ HOLIDAY VAULT // CHRISTMAS CELEBRATION ]",
                MottoTr = "Merry Wasteland Christmas // Mutlu Noeller",
                MottoEn = "Merry Wasteland Christmas // Holiday Vault",
                BgColor = new Color(0.04f, 0.11f, 0.06f, 0.88f),
                HeaderBgColor = new Color(0.12f, 0.05f, 0.05f, 0.95f),
                BorderColor = new Color(0.90f, 0.15f, 0.15f, 0.95f),
                AccentColor = new Color(0.25f, 0.90f, 0.35f, 1.0f),
                TextColor = new Color(0.95f, 0.98f, 0.95f, 1.0f),
                ButtonBg = new Color(0.07f, 0.18f, 0.09f, 0.85f),
                ButtonHover = new Color(0.18f, 0.28f, 0.16f, 0.95f),
                ToggleOn = new Color(0.25f, 0.95f, 0.35f, 1.0f),
                ToggleOff = new Color(0.85f, 0.20f, 0.20f, 0.9f),
                ActiveTab = new Color(0.35f, 0.10f, 0.10f, 0.95f),
                InactiveTab = new Color(0.06f, 0.14f, 0.08f, 0.80f)
            },
            // 9. Thanksgiving
            new FalloutTheme
            {
                Id = "Thanksgiving",
                DisplayName = "Thanksgiving",
                HeaderTag = "[ HARVEST FEAST // THANKSGIVING GATHERING ]",
                MottoTr = "Give Thanks for Clean Water // Şükran Günü Hasatı",
                MottoEn = "Give Thanks for Clean Water // Harvest Feast",
                BgColor = new Color(0.11f, 0.07f, 0.04f, 0.88f),
                HeaderBgColor = new Color(0.20f, 0.11f, 0.06f, 0.95f),
                BorderColor = new Color(0.95f, 0.58f, 0.10f, 0.95f),
                AccentColor = new Color(1.0f, 0.78f, 0.25f, 1.0f),
                TextColor = new Color(0.98f, 0.88f, 0.72f, 1.0f),
                ButtonBg = new Color(0.18f, 0.11f, 0.07f, 0.85f),
                ButtonHover = new Color(0.30f, 0.18f, 0.10f, 0.95f),
                ToggleOn = new Color(1.0f, 0.70f, 0.10f, 1.0f),
                ToggleOff = new Color(0.55f, 0.30f, 0.15f, 0.9f),
                ActiveTab = new Color(0.35f, 0.20f, 0.08f, 0.95f),
                InactiveTab = new Color(0.14f, 0.08f, 0.05f, 0.80f)
            },
            // 10. New Vegas
            new FalloutTheme
            {
                Id = "NewVegas",
                DisplayName = "New Vegas",
                HeaderTag = "[ MR. HOUSE // THE NEW VEGAS STRIP ]",
                MottoTr = "The House Always Wins // Lucky 38",
                MottoEn = "The House Always Wins // Lucky 38",
                BgColor = new Color(0.11f, 0.06f, 0.03f, 0.88f),
                HeaderBgColor = new Color(0.22f, 0.10f, 0.04f, 0.95f),
                BorderColor = new Color(1.0f, 0.62f, 0.10f, 0.95f),
                AccentColor = new Color(1.0f, 0.75f, 0.15f, 1.0f),
                TextColor = new Color(1.0f, 0.82f, 0.50f, 1.0f),
                ButtonBg = new Color(0.19f, 0.10f, 0.05f, 0.85f),
                ButtonHover = new Color(0.32f, 0.17f, 0.08f, 0.95f),
                ToggleOn = new Color(1.0f, 0.65f, 0.10f, 1.0f),
                ToggleOff = new Color(0.55f, 0.30f, 0.12f, 0.9f),
                ActiveTab = new Color(0.40f, 0.20f, 0.06f, 0.95f),
                InactiveTab = new Color(0.14f, 0.07f, 0.04f, 0.80f)
            }
        };

        public static int CurrentThemeIndex = 5; // Varsayilan: Vault 33
        public static FalloutTheme Current => Themes[CurrentThemeIndex];

        private static Texture2D _pixelTex;
        private static Vector2 _commandsScroll;
        private static CustomCommand _hoveredCmd;
        private static string _cfgPath;

        // Cached Harmony FieldRef delegates & Method delegates (zero-allocation, zero-boxing)
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowMenu = TryFieldRef<bool>("showMenu");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowMenuButton = TryFieldRef<bool>("showMenuButton");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowKeybindText = TryFieldRef<bool>("showKeybindText");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, float> _refUiScaleMultiplier = TryFieldRef<float>("uiScaleMultiplier");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowLogWindow = TryFieldRef<bool>("showLogWindow");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowPopup = TryFieldRef<bool>("showPopup");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowKeybindPicker = TryFieldRef<bool>("showKeybindPicker");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refShowCommandWindow = TryFieldRef<bool>("showCommandWindow");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, Vector2> _refMenuPosition = TryFieldRef<Vector2>("menuPosition");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, bool> _refIsDragging = TryFieldRef<bool>("isDragging");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, Vector2> _refDragOffset = TryFieldRef<Vector2>("dragOffset");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, string> _refSelectedCategory = TryFieldRef<string>("selectedCategory");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, List<CustomCommand>> _refRegisteredCommands = TryFieldRef<List<CustomCommand>>("registeredCommands");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, BepInEx.Configuration.ConfigEntry<float>> _refUiScaleConfig = TryFieldRef<BepInEx.Configuration.ConfigEntry<float>>("uiScaleConfig");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, CustomCommand> _refSelectedCommand = TryFieldRef<CustomCommand>("selectedCommand");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, string> _refUserInput = TryFieldRef<string>("userInput");
        private static readonly AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, string> _refLastDisplayedMessage = TryFieldRef<string>("lastDisplayedMessage");

        private static readonly Action<global::CTDynamicModMenu.CTDynamicModMenu> _fnDrawLogWindow = TryMethodAction("DrawLogWindow");
        private static readonly Action<global::CTDynamicModMenu.CTDynamicModMenu> _fnDrawKeybindPicker = TryMethodAction("DrawKeybindPicker");
        private static readonly Action<global::CTDynamicModMenu.CTDynamicModMenu> _fnDrawCommandWindow = TryMethodAction("DrawCommandWindow");

        // Cached GUIStyles (rebuilt only when GUI.skin changes)
        private static GUISkin _cachedSkin;
        private static GUIStyle _styleLabel;
        private static GUIStyle _styleLabelWrap;
        private static GUIStyle _styleTextField;

        // Static UI scale state (replaces per-OnGUI closure allocations)
        private static float _scale = 1f;
        private static float S(float v) => v * _scale;
        private static int SI(float v) => Mathf.Max(10, (int)(v * _scale));

        // Cached categorized command lists (invalidated on list/count/language/first-item change)
        private static readonly List<string> _cachedCategories = new List<string>();
        private static readonly HashSet<string> _cachedCatSet = new HashSet<string>();
        private static readonly Dictionary<string, List<CustomCommand>> _cachedByCategory = new Dictionary<string, List<CustomCommand>>();
        private static readonly List<CustomCommand> _emptyCommands = new List<CustomCommand>();
        private static List<CustomCommand> _cachedCmdSource;
        private static CustomCommand _cachedFirstCmd;
        private static int _cachedCmdCount = -1;
        private static bool _cachedIsTurkish;

        // Cached theme header label
        private static int _cachedThemeLabelIdx = -1;
        private static bool _cachedThemeLabelTr;
        private static string _cachedThemeLabel;

        // Debounced scale slider persistence
        private static bool _scaleDirty;
        private static float _scaleDirtySince;

        // Static preset arrays (zero per-frame array allocations)
        private static readonly string[] PresetsNuka = { "10,000", "50,000", "250,000", "999,999" };
        private static readonly string[] PresetsQuantum = { "100", "500", "2,000", "9,999" };
        private static readonly string[] PresetsRes = { "1,000", "5,000", "9,999", "99,999" };
        private static readonly string[] PresetsBoxes = { "10", "25", "50", "100" };
        private static readonly string[] PresetsPacks = { "5", "10", "25", "50" };
        private static readonly string[] PresetsStorage = { "500", "1,000", "5,000", "10,000" };
        private static readonly string[] PresetsDweller = { "common", "rare", "legendary" };
        private static readonly string[] PresetsVatsMult = { "2", "5", "10", "25", "50", "100" };
        private static readonly string[] PresetsVatsAnim = { "1", "2", "3", "5" };
        private static readonly string[] PresetsDefault = { "10", "50", "100", "500" };

        private static AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, T> TryFieldRef<T>(string name)
        {
            try
            {
                return AccessTools.FieldRefAccess<global::CTDynamicModMenu.CTDynamicModMenu, T>(name);
            }
            catch
            {
                return null;
            }
        }

        private static Action<global::CTDynamicModMenu.CTDynamicModMenu> TryMethodAction(string name)
        {
            try
            {
                var mi = AccessTools.Method(typeof(global::CTDynamicModMenu.CTDynamicModMenu), name, Type.EmptyTypes);
                if (mi != null)
                {
                    return (Action<global::CTDynamicModMenu.CTDynamicModMenu>)Delegate.CreateDelegate(
                        typeof(Action<global::CTDynamicModMenu.CTDynamicModMenu>), mi);
                }
            }
            catch { }
            return null;
        }

        private static T GetField<T>(AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, T> fieldRef, global::CTDynamicModMenu.CTDynamicModMenu menu, T fallback = default(T))
        {
            return (fieldRef != null && menu != null) ? fieldRef(menu) : fallback;
        }

        private static void SetField<T>(AccessTools.FieldRef<global::CTDynamicModMenu.CTDynamicModMenu, T> fieldRef, global::CTDynamicModMenu.CTDynamicModMenu menu, T value)
        {
            if (fieldRef != null && menu != null)
            {
                fieldRef(menu) = value;
            }
        }

        private static void EnsureStyles()
        {
            var skin = GUI.skin;
            if (_cachedSkin == skin && _styleLabel != null && _styleLabelWrap != null && _styleTextField != null)
                return;

            _cachedSkin = skin;
            _styleLabel = new GUIStyle(skin.label) { wordWrap = false };
            _styleLabelWrap = new GUIStyle(skin.label) { wordWrap = true };
            _styleTextField = new GUIStyle(skin.textField)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static GUIStyle PrepLabel(int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color, bool wordWrap = false)
        {
            var style = wordWrap ? _styleLabelWrap : _styleLabel;
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.alignment = alignment;
            style.normal.textColor = color;
            return style;
        }

        private static void EnsureCommandCache(List<CustomCommand> registeredCommands, string themesTabName)
        {
            int count = registeredCommands != null ? registeredCommands.Count : 0;
            CustomCommand firstCmd = count > 0 ? registeredCommands[0] : null;
            bool isTr = MenuPlugin.IsTurkish;

            if (_cachedCmdSource == registeredCommands &&
                _cachedCmdCount == count &&
                _cachedIsTurkish == isTr &&
                ReferenceEquals(_cachedFirstCmd, firstCmd))
            {
                return;
            }

            _cachedCmdSource = registeredCommands;
            _cachedCmdCount = count;
            _cachedIsTurkish = isTr;
            _cachedFirstCmd = firstCmd;

            _cachedCategories.Clear();
            _cachedCatSet.Clear();
            foreach (var kv in _cachedByCategory)
            {
                kv.Value.Clear();
            }

            if (registeredCommands != null)
            {
                for (int i = 0; i < registeredCommands.Count; i++)
                {
                    var cmd = registeredCommands[i];
                    if (cmd == null) continue;
                    string cat = cmd.Category;
                    if (_cachedCatSet.Add(cat))
                    {
                        _cachedCategories.Add(cat);
                    }
                    List<CustomCommand> bucket;
                    if (!_cachedByCategory.TryGetValue(cat, out bucket))
                    {
                        bucket = new List<CustomCommand>();
                        _cachedByCategory[cat] = bucket;
                    }
                    bucket.Add(cmd);
                }
            }

            if (!_cachedCatSet.Contains(themesTabName))
            {
                _cachedCategories.Add(themesTabName);
            }
        }

        private static void FlushPendingScaleSave(global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            if (!_scaleDirty || menu == null) return;
            _scaleDirty = false;
            try
            {
                var cfgEntry = GetField(_refUiScaleConfig, menu, null);
                if (cfgEntry != null)
                {
                    float curScale = GetField(_refUiScaleMultiplier, menu, 1f);
                    cfgEntry.Value = curScale;
                    if (menu.Config != null && !menu.Config.SaveOnConfigSet)
                    {
                        menu.Config.Save();
                    }
                }
            }
            catch { }
        }

        private static bool ContainsCI(string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void Initialize(string configDir)
        {
            _cfgPath = Path.Combine(configDir, "FSAutoQuest.theme");
            LoadSavedTheme();
        }

        public static void SetTheme(int index)
        {
            if (index < 0) index = Themes.Length - 1;
            if (index >= Themes.Length) index = 0;
            CurrentThemeIndex = index;
            SaveTheme();
        }

        public static void NextTheme()
        {
            SetTheme(CurrentThemeIndex + 1);
        }

        public static void PrevTheme()
        {
            SetTheme(CurrentThemeIndex - 1);
        }

        private static void SaveTheme()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cfgPath))
                {
                    File.WriteAllText(_cfgPath, Current.Id);
                }
            }
            catch { }
        }

        private static void LoadSavedTheme()
        {
            try
            {
                if (File.Exists(_cfgPath))
                {
                    string id = File.ReadAllText(_cfgPath).Trim();
                    for (int i = 0; i < Themes.Length; i++)
                    {
                        if (Themes[i].Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                        {
                            CurrentThemeIndex = i;
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        public static void DrawRect(Rect rect, Color color)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            if (_pixelTex == null)
            {
                _pixelTex = new Texture2D(1, 1);
                _pixelTex.hideFlags = HideFlags.HideAndDontSave;
                _pixelTex.SetPixel(0, 0, Color.white);
                _pixelTex.Apply();
            }
            var old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _pixelTex);
            GUI.color = old;
        }

        public static void DrawBoxWithBorder(Rect rect, Color bg, Color border, float borderWidth = 2f)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            DrawRect(rect, bg);
            DrawRect(new Rect(rect.x, rect.y, rect.width, borderWidth), border);
            DrawRect(new Rect(rect.x, rect.y + rect.height - borderWidth, rect.width, borderWidth), border);
            DrawRect(new Rect(rect.x, rect.y, borderWidth, rect.height), border);
            DrawRect(new Rect(rect.x + rect.width - borderWidth, rect.y, borderWidth, rect.height), border);
        }

        /// <summary>
        /// Orijinal OnGUI metodunu tamamen temamiza uygun hale getiren yama.
        /// Sol ustteki cirkin kirmizi yazi ve gri butonu yok eder, yerine Pip-Boy temali buton koyar.
        /// </summary>
        public static void DrawThemedOnGUI(global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            if (menu == null) return;
            EnsureStyles();

            bool showMenu = GetField(_refShowMenu, menu, false);
            bool showMenuButton = GetField(_refShowMenuButton, menu, true);
            bool showKeybindText = GetField(_refShowKeybindText, menu, true);
            var theme = Current;

            float autoScale = Screen.height / 1080f;
            float uiScale = GetField(_refUiScaleMultiplier, menu, 1f);
            _scale = autoScale * uiScale;

            // Flush any debounced scale save if menu was closed via hotkey (e.g. F4)
            if (!showMenu && _scaleDirty)
            {
                FlushPendingScaleSave(menu);
            }

            // 1. Menu KAPALIYKEN: Dweller dislisini (134) kapatmayan, temaya uygun Pip-Boy butonu
            if (!showMenu)
            {
                // Ekranin sol ustunde disli sayaci ~70px kaplar; butonu tam onun altina (Y=78) yerlestiriyoruz
                float btnX = S(12f);
                float btnY = S(78f);
                float btnW = S(190f);
                float btnH = S(32f);

                if (showKeybindText)
                {
                    var keyStyle = PrepLabel(SI(11f), FontStyle.Bold, TextAnchor.MiddleLeft, theme.AccentColor);
                    string keyText = MenuPlugin.IsTurkish ? "▶ F4: Mod Menüsü" : "▶ F4: Mod Menu";
                    GUI.Label(new Rect(btnX, btnY - S(17f), S(220f), S(17f)), keyText, keyStyle);
                }

                if (showMenuButton)
                {
                    Rect openBtnRect = new Rect(btnX, btnY, btnW, btnH);
                    DrawBoxWithBorder(openBtnRect, theme.BgColor, theme.BorderColor, S(2f));

                    var openStyle = PrepLabel(SI(12f), FontStyle.Bold, TextAnchor.MiddleCenter, theme.AccentColor);
                    string btnLabel = MenuPlugin.IsTurkish ? "⚙ MOD MENÜSÜ (F4)" : "⚙ MOD MENU (F4)";

                    if (GUI.Button(openBtnRect, btnLabel, openStyle))
                    {
                        SetField(_refShowMenu, menu, true);
                        menu.EnableCursor();
                    }
                }
            }

            // 2. Log penceresi
            bool showLog = GetField(_refShowLogWindow, menu, false);
            if (showLog && _fnDrawLogWindow != null)
            {
                try { _fnDrawLogWindow(menu); } catch { }
            }

            // 3. Menu ACIKKEN: Fallout Temali Mod Menusu
            if (showMenu)
            {
                if (Event.current == null || Event.current.type == EventType.Repaint)
                {
                    menu.EnableCursor();
                }
                DrawThemedMenu(menu);
            }

            // 4. Diger popup pencereler
            bool showPopup = GetField(_refShowPopup, menu, false);
            if (showPopup)
            {
                try { DrawThemedPopup(menu, theme); } catch { }
            }

            bool showKeybindPicker = GetField(_refShowKeybindPicker, menu, false);
            if (showKeybindPicker && _fnDrawKeybindPicker != null)
            {
                try { _fnDrawKeybindPicker(menu); } catch { }
            }

            bool showCommand = GetField(_refShowCommandWindow, menu, false);
            if (showCommand && _fnDrawCommandWindow != null)
            {
                try { _fnDrawCommandWindow(menu); } catch { }
            }
        }

        /// <summary>
        /// Fallout temali ana menuyu cizer. Sekmelerin disari tasmamasi icin dinamik kaydirma ve satir sarma uygular.
        /// </summary>
        public static void DrawThemedMenu(global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            if (menu == null) return;
            EnsureStyles();

            var pos = GetField(_refMenuPosition, menu, Vector2.zero);
            var isDragging = GetField(_refIsDragging, menu, false);
            var dragOffset = GetField(_refDragOffset, menu, Vector2.zero);
            var selectedCategory = GetField(_refSelectedCategory, menu, "");
            var registeredCommands = GetField(_refRegisteredCommands, menu, null);
            var uiScale = GetField(_refUiScaleMultiplier, menu, 1f);

            float autoScale = Screen.height / 1080f;
            _scale = autoScale * uiScale;

            var theme = Current;

            // Temalar sekmesini ozel olarak kategorilere ekleyelim (cached)
            string themesTabName = MenuPlugin.IsTurkish ? "Temalar" : "Themes";
            EnsureCommandCache(registeredCommands, themesTabName);

            if (string.IsNullOrEmpty(selectedCategory) || (!_cachedCatSet.Contains(selectedCategory) && selectedCategory != themesTabName))
            {
                selectedCategory = _cachedCategories.Count > 0 ? _cachedCategories[0] : "";
                SetField(_refSelectedCategory, menu, selectedCategory);
            }

            // Pencere genisligi: Butun sekmelerin ve iceriklerin ferahca sigmasi icin genis tutuyoruz
            float windowWidth = S(840f);

            // Ekran sinirlari icinde tut
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width - windowWidth);

            // 1. Baslik Bari (Header)
            float headerHeight = S(46f);
            Rect headerRect = new Rect(pos.x, pos.y, windowWidth, headerHeight);

            // 2. Hizli Tema Gecis Bari (Header alti)
            float themeBarHeight = S(32f);
            float curY = pos.y + headerHeight + S(6f);
            Rect themeBarRect = new Rect(pos.x + S(10f), curY, windowWidth - S(20f), themeBarHeight);

            curY += themeBarHeight + S(8f);

            // 3. Kategori Sekmeleri Hesaplamasi (Satir Sarma - Wrapping)
            float tabStartX = pos.x + S(10f);
            float maxTabRight = pos.x + windowWidth - S(10f);
            float tabH = S(28f);
            float tabGapX = S(5f);
            float tabGapY = S(5f);

            // Kac satir olacagini onceden hesaplayalim ki pencere yuksekligini tam belirleyelim
            float testX = tabStartX;
            float testY = curY;
            for (int i = 0; i < _cachedCategories.Count; i++)
            {
                var cat = _cachedCategories[i];
                float tabW = S(cat.Length * 8f + 16f);
                if (testX + tabW > maxTabRight && testX > tabStartX)
                {
                    testX = tabStartX;
                    testY += tabH + tabGapY;
                }
                testX += tabW + tabGapX;
            }
            float tabsTotalHeight = (testY - curY) + tabH;

            float contentHeight = S(400f);
            float infoHeight = S(48f);
            float bottomHeight = S(36f);

            // Pencerenin net yuksekligi
            float windowHeight = headerHeight + S(6f) + themeBarHeight + S(8f) + tabsTotalHeight + S(10f) + contentHeight + S(8f) + infoHeight + S(10f) + bottomHeight + S(12f);
            pos.y = Mathf.Clamp(pos.y, 0, Screen.height - windowHeight);

            Rect windowRect = new Rect(pos.x, pos.y, windowWidth, windowHeight);

            // Ana Yari Saydam Kutu & Fallout CRT Cercevesi
            DrawBoxWithBorder(windowRect, theme.BgColor, theme.BorderColor, S(2.5f));

            // Header Cizimi
            headerRect.y = pos.y;
            DrawRect(headerRect, theme.HeaderBgColor);
            DrawRect(new Rect(pos.x, pos.y + headerHeight - S(2f), windowWidth, S(2f)), theme.BorderColor);

            var titleStyle = PrepLabel(SI(15f), FontStyle.Bold, TextAnchor.MiddleLeft, theme.AccentColor);
            GUI.Label(new Rect(pos.x + S(12f), pos.y + S(4f), windowWidth - S(120f), S(22f)), theme.HeaderTag, titleStyle);

            var mottoStyle = PrepLabel(SI(11f), FontStyle.Italic, TextAnchor.MiddleLeft, theme.TextColor);
            GUI.Label(new Rect(pos.x + S(14f), pos.y + S(24f), windowWidth - S(120f), S(18f)), theme.Motto, mottoStyle);

            // Hizli Dil Degistirme Butonu (Sag Ust)
            float langBtnWidth = S(85f);
            Rect langBtnRect = new Rect(pos.x + windowWidth - langBtnWidth - S(10f), pos.y + S(8f), langBtnWidth, S(28f));
            bool isLangHover = langBtnRect.Contains(Event.current.mousePosition);

            DrawBoxWithBorder(langBtnRect, isLangHover ? theme.ButtonHover : theme.ButtonBg, isLangHover ? theme.AccentColor : theme.BorderColor * 0.7f, isLangHover ? S(2f) : S(1.2f));

            var langStyle = PrepLabel(SI(11f), FontStyle.Bold, TextAnchor.MiddleCenter, isLangHover ? theme.AccentColor : theme.TextColor);
            if (GUI.Button(langBtnRect, MenuPlugin.IsTurkish ? "DİL: TR" : "LANG: EN", langStyle))
            {
                MenuPlugin.ToggleLanguage();
            }

            // Tema Gecis Bari
            themeBarRect.y = pos.y + headerHeight + S(6f);
            DrawBoxWithBorder(themeBarRect, theme.ButtonBg, theme.BorderColor * 0.7f, S(1f));

            Rect prevBtnRect = new Rect(themeBarRect.x + S(4f), themeBarRect.y + S(3f), S(35f), themeBarHeight - S(6f));
            bool isPrevHover = prevBtnRect.Contains(Event.current.mousePosition);
            DrawBoxWithBorder(prevBtnRect, isPrevHover ? theme.ButtonHover : theme.ButtonBg, isPrevHover ? theme.AccentColor : theme.BorderColor * 0.5f, S(1f));

            var arrowStyle = PrepLabel(SI(12f), FontStyle.Bold, TextAnchor.MiddleCenter, isPrevHover ? theme.AccentColor : theme.TextColor);
            if (GUI.Button(prevBtnRect, "◄", arrowStyle))
            {
                PrevTheme();
            }

            var themeNameStyle = PrepLabel(SI(13f), FontStyle.Bold, TextAnchor.MiddleCenter, theme.AccentColor);
            if (_cachedThemeLabelIdx != CurrentThemeIndex || _cachedThemeLabelTr != MenuPlugin.IsTurkish || _cachedThemeLabel == null)
            {
                _cachedThemeLabelIdx = CurrentThemeIndex;
                _cachedThemeLabelTr = MenuPlugin.IsTurkish;
                _cachedThemeLabel = (MenuPlugin.IsTurkish ? "TEMA: " : "THEME: ") + theme.DisplayName + " (" + (CurrentThemeIndex + 1) + "/10)";
            }
            GUI.Label(new Rect(themeBarRect.x + S(45f), themeBarRect.y, themeBarRect.width - S(90f), themeBarHeight), _cachedThemeLabel, themeNameStyle);

            Rect nextBtnRect = new Rect(themeBarRect.x + themeBarRect.width - S(39f), themeBarRect.y + S(3f), S(35f), themeBarHeight - S(6f));
            bool isNextHover = nextBtnRect.Contains(Event.current.mousePosition);
            DrawBoxWithBorder(nextBtnRect, isNextHover ? theme.ButtonHover : theme.ButtonBg, isNextHover ? theme.AccentColor : theme.BorderColor * 0.5f, S(1f));

            var nextArrowStyle = PrepLabel(SI(12f), FontStyle.Bold, TextAnchor.MiddleCenter, isNextHover ? theme.AccentColor : theme.TextColor);
            if (GUI.Button(nextBtnRect, "►", nextArrowStyle))
            {
                NextTheme();
            }

            // Kategori Sekmelerini Ciz (Satir Sarmali - Asla Disari Tasmaz!)
            float renderTabX = tabStartX;
            float renderTabY = themeBarRect.y + themeBarHeight + S(8f);
            int tabFontSize = SI(11.5f);

            for (int i = 0; i < _cachedCategories.Count; i++)
            {
                var cat = _cachedCategories[i];
                float tabW = S(cat.Length * 8f + 16f);
                if (renderTabX + tabW > maxTabRight && renderTabX > tabStartX)
                {
                    renderTabX = tabStartX;
                    renderTabY += tabH + tabGapY;
                }

                Rect tabRect = new Rect(renderTabX, renderTabY, tabW, tabH);
                bool isActive = cat == selectedCategory;

                DrawBoxWithBorder(tabRect, isActive ? theme.ActiveTab : theme.InactiveTab, isActive ? theme.AccentColor : theme.BorderColor * 0.45f, isActive ? S(2f) : S(1f));

                var tabStyle = PrepLabel(tabFontSize, isActive ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter, isActive ? theme.AccentColor : theme.TextColor);
                if (GUI.Button(tabRect, cat, tabStyle))
                {
                    selectedCategory = cat;
                    SetField(_refSelectedCategory, menu, selectedCategory);
                }

                renderTabX += tabW + tabGapX;
            }

            curY = renderTabY + tabH + S(10f);

            // Icerik Alani
            Rect contentBoxRect = new Rect(pos.x + S(10f), curY, windowWidth - S(20f), contentHeight);
            DrawBoxWithBorder(contentBoxRect, theme.BgColor * 0.7f, theme.BorderColor * 0.4f, S(1f));

            _hoveredCmd = null;

            if (selectedCategory == themesTabName)
            {
                DrawThemesGrid(contentBoxRect, theme);
            }
            else
            {
                DrawCommandsGrid(contentBoxRect, selectedCategory, theme, menu);
            }

            curY += contentHeight + S(8f);

            // Bilgi / Aciklama (Tooltip) Paneli
            Rect infoRect = new Rect(pos.x + S(10f), curY, windowWidth - S(20f), infoHeight);
            DrawBoxWithBorder(infoRect, theme.HeaderBgColor * 0.9f, theme.BorderColor * 0.6f, S(1f));

            var infoStyle = PrepLabel(SI(11.5f), FontStyle.Normal, TextAnchor.MiddleLeft, theme.TextColor, wordWrap: true);

            string infoText;
            if (_hoveredCmd != null)
            {
                infoText = "<color=#" + theme.AccentHex + "><b>" + _hoveredCmd.Name + ":</b></color> " + _hoveredCmd.Description;
            }
            else
            {
                infoText = "<color=#" + theme.AccentHex + "><b>ROBCO PIP-BOY 3000 //</b></color> " +
                    (MenuPlugin.IsTurkish ? "Detay ve aciklama icin bir dugmenin uzerine gelin veya tiklayin." : "Hover over a button to view details and descriptions.");
            }
            GUI.Label(new Rect(infoRect.x + S(10f), infoRect.y + S(4f), infoRect.width - S(20f), infoHeight - S(8f)), infoText, infoStyle);

            curY += infoHeight + S(10f);

            // Alt Bar: Olcek Cubugu & Kapat Butonu
            var sliderLabelStyle = PrepLabel(SI(11f), FontStyle.Normal, TextAnchor.MiddleLeft, theme.TextColor);
            GUI.Label(new Rect(pos.x + S(14f), curY, S(90f), bottomHeight), MenuPlugin.IsTurkish ? "Olcek:" : "Scale:", sliderLabelStyle);

            float sliderWidth = S(180f);
            float newScale = GUI.HorizontalSlider(new Rect(pos.x + S(100f), curY + S(10f), sliderWidth, S(16f)), uiScale, 0.5f, 2.0f);
            if (Mathf.Abs(newScale - uiScale) > 0.01f)
            {
                SetField(_refUiScaleMultiplier, menu, newScale);
                _scaleDirty = true;
                _scaleDirtySince = Time.unscaledTime;
            }
            if (_scaleDirty && (Event.current.rawType == EventType.MouseUp || (Time.unscaledTime - _scaleDirtySince) >= 0.5f))
            {
                FlushPendingScaleSave(menu);
            }

            // Kapat Butonu
            float closeBtnWidth = S(180f);
            Rect closeBtnRect = new Rect(pos.x + windowWidth - closeBtnWidth - S(10f), curY, closeBtnWidth, bottomHeight);
            bool isCloseHover = closeBtnRect.Contains(Event.current.mousePosition);

            Color closeBg = isCloseHover ? new Color(theme.ToggleOff.r, theme.ToggleOff.g, theme.ToggleOff.b, 0.35f) : theme.ButtonBg;
            Color closeBorder = isCloseHover ? theme.ToggleOff : theme.BorderColor * 0.7f;
            DrawBoxWithBorder(closeBtnRect, closeBg, closeBorder, isCloseHover ? S(2f) : S(1.2f));

            // Terminal ust cizgi vurgusu
            DrawRect(new Rect(closeBtnRect.x + S(2f), closeBtnRect.y + S(1f), closeBtnRect.width - S(4f), S(1f)), isCloseHover ? theme.ToggleOff * 0.8f : theme.BorderColor * 0.3f);

            var closeStyle = PrepLabel(SI(13f), FontStyle.Bold, TextAnchor.MiddleCenter, isCloseHover ? Color.white : theme.ToggleOff);
            string closeText = MenuPlugin.IsTurkish ? "<b>✕ MENÜYÜ KAPAT</b>" : "<b>✕ CLOSE MENU</b>";

            if (GUI.Button(closeBtnRect, closeText, closeStyle))
            {
                FlushPendingScaleSave(menu);
                SetField(_refShowMenu, menu, false);
                menu.RecoverCursorState();
            }

            // Pencere Surukleme Islemleri
            var ev = Event.current;
            if (ev.type == EventType.MouseDown && headerRect.Contains(ev.mousePosition))
            {
                isDragging = true;
                dragOffset = ev.mousePosition - pos;
                SetField(_refIsDragging, menu, true);
                SetField(_refDragOffset, menu, dragOffset);
                ev.Use();
            }
            else if (ev.type == EventType.MouseDrag && isDragging)
            {
                pos = ev.mousePosition - dragOffset;
                SetField(_refMenuPosition, menu, pos);
                ev.Use();
            }
            else if (ev.type == EventType.MouseUp && isDragging)
            {
                isDragging = false;
                SetField(_refIsDragging, menu, false);
            }
        }

        private static void DrawThemesGrid(Rect area, FalloutTheme curTheme)
        {
            float padding = S(12f);
            float colWidth = (area.width - padding * 3f) / 2f;
            float btnHeight = S(46f);
            int fontSize = SI(12.5f);
            string activeText = MenuPlugin.IsTurkish ? " (AKTİF)" : " (ACTIVE)";

            for (int i = 0; i < Themes.Length; i++)
            {
                int row = i / 2;
                int col = i % 2;
                float x = area.x + padding + col * (colWidth + padding);
                float y = area.y + padding + row * (btnHeight + S(8f));

                Rect btnRect = new Rect(x, y, colWidth, btnHeight);
                var t = Themes[i];
                bool isSelected = i == CurrentThemeIndex;
                bool isThemeHover = btnRect.Contains(Event.current.mousePosition);

                Color tBg = isSelected ? t.ActiveTab : (isThemeHover ? t.ButtonHover : t.ButtonBg);
                Color tBorder = isSelected ? t.AccentColor : (isThemeHover ? t.BorderColor : t.BorderColor * 0.5f);

                DrawBoxWithBorder(btnRect, tBg, tBorder, isSelected ? S(2.5f) : (isThemeHover ? S(2f) : S(1.2f)));

                // Pip-Boy terminal ust parlama cizgisi
                DrawRect(new Rect(btnRect.x + S(2f), btnRect.y + S(1f), btnRect.width - S(4f), S(1f)), isSelected ? t.AccentColor * 0.9f : (isThemeHover ? t.BorderColor * 0.7f : t.BorderColor * 0.25f));

                var btnStyle = PrepLabel(fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, isSelected || isThemeHover ? t.AccentColor : t.TextColor, wordWrap: true);
                string label = isSelected ? ("★ " + t.DisplayName + activeText) : t.DisplayName;

                if (GUI.Button(btnRect, label, btnStyle))
                {
                    SetTheme(i);
                }
            }
        }

        private static void DrawCommandsGrid(Rect area, string category, FalloutTheme theme, global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            List<CustomCommand> cmds;
            if (string.IsNullOrEmpty(category) || !_cachedByCategory.TryGetValue(category, out cmds))
            {
                cmds = _emptyCommands;
            }

            float padding = S(10f);
            float btnWidth = (area.width - padding * 4f) / 3f; // 3 sutunlu ferah duzen
            float btnHeight = S(38f);
            int cols = 3;
            int rows = Mathf.CeilToInt((float)cmds.Count / cols);
            float totalHeight = rows * (btnHeight + padding) + padding;

            Rect viewRect = new Rect(0, 0, area.width - S(20f), totalHeight);
            _commandsScroll = GUI.BeginScrollView(new Rect(area.x + S(4f), area.y + S(4f), area.width - S(8f), area.height - S(8f)), _commandsScroll, viewRect);

            int cmdFontSize = SI(11.5f);
            string onSuffix = " <color=#" + theme.ToggleOnHex + "><b>" + (MenuPlugin.IsTurkish ? "[AÇIK]" : "[ON]") + "</b></color>";
            string offSuffix = " <color=#" + theme.ToggleOffHex + ">" + (MenuPlugin.IsTurkish ? "[KAPALI]" : "[OFF]") + "</color>";

            Vector2 mouseInScroll = Event.current.mousePosition;

            for (int i = 0; i < cmds.Count; i++)
            {
                var cmd = cmds[i];
                int r = i / cols;
                int c = i % cols;
                float x = padding + c * (btnWidth + padding);
                float y = padding + r * (btnHeight + padding);
                Rect bRect = new Rect(x, y, btnWidth, btnHeight);

                // Tooltip & Hover kontrolu
                bool isHover = bRect.Contains(mouseInScroll);
                if (isHover)
                {
                    _hoveredCmd = cmd;
                }

                // Buton rengi ve durumu
                Color btnBg = isHover ? theme.ButtonHover : theme.ButtonBg;
                Color borderColor = isHover ? theme.AccentColor : theme.BorderColor * 0.55f;
                Color textColor = isHover ? theme.AccentColor : theme.TextColor;
                string fullLabel;

                bool isToggle = cmd.IsToggle;
                bool isEnabled = isToggle && cmd.IsEnabled;

                if (isToggle)
                {
                    if (isEnabled)
                    {
                        btnBg = isHover ? Color.Lerp(theme.ButtonHover, theme.ToggleOn, 0.45f) : Color.Lerp(theme.ButtonBg, theme.ToggleOn, 0.22f);
                        borderColor = theme.ToggleOn;
                        textColor = isHover ? Color.white : theme.ToggleOn;
                        fullLabel = cmd.Name + onSuffix;
                    }
                    else
                    {
                        btnBg = isHover ? theme.ButtonHover : theme.ButtonBg;
                        textColor = isHover ? theme.TextColor : theme.TextColor * 0.85f;
                        fullLabel = cmd.Name + offSuffix;
                    }
                }
                else
                {
                    fullLabel = cmd.Name;
                }

                // Temaya ozel Fallout Pip-Boy terminal kutusu ve parlayan kenarlik
                float bWidth = isHover || isEnabled ? S(2f) : S(1.2f);
                DrawBoxWithBorder(bRect, btnBg, borderColor, bWidth);

                // Pip-Boy terminal cam yansimasi / ust vurgusu
                Color topGlow = isHover ? theme.AccentColor * 0.85f : (isEnabled ? theme.ToggleOn * 0.7f : theme.BorderColor * 0.25f);
                DrawRect(new Rect(bRect.x + S(2f), bRect.y + S(1f), bRect.width - S(4f), S(1f)), topGlow);

                var cmdStyle = PrepLabel(cmdFontSize, FontStyle.Bold, TextAnchor.MiddleCenter, textColor, wordWrap: true);

                if (GUI.Button(bRect, fullLabel, cmdStyle))
                {
                    try
                    {
                        string format = cmd.Format ?? "";
                        string[] formatParts = format.Split(' ');

                        if (formatParts.Length > 1)
                        {
                            // Komut parametre bekliyor (Örn: /nuka <amount>, /setres <amount>, /setboxes <amount>)
                            SetField(_refShowMenu, menu, false);
                            SetField(_refShowPopup, menu, true);
                            SetField(_refSelectedCommand, menu, cmd);
                            SetField(_refUserInput, menu, GetSmartDefault(cmd));
                        }
                        else if (isToggle)
                        {
                            cmd.IsEnabled = !cmd.IsEnabled;
                            SaveCommandState(menu, cmd);
                            cmd.Execute(new CommandInput());
                        }
                        else
                        {
                            cmd.Execute(new CommandInput());
                        }
                    }
                    catch (Exception ex)
                    {
                        if (MenuPlugin.Log != null)
                            MenuPlugin.Log.LogWarning("Komut calistirma hatasi (" + cmd.Name + "): " + ex.Message);
                    }
                }
            }

            GUI.EndScrollView();
        }

        private static void SaveConfigIfManual(BepInEx.Configuration.ConfigFile cfg)
        {
            if (cfg != null && !cfg.SaveOnConfigSet)
            {
                cfg.Save();
            }
        }

        public static void SaveCommandState(global::CTDynamicModMenu.CTDynamicModMenu menu, CustomCommand cmd)
        {
            if (cmd == null) return;

            if (menu != null)
            {
                MenuPlugin.SetPersistentSettings(menu, true);
            }

            CustomCommand target = cmd;
            if (cmd is LocalizedCommand loc && loc.Inner != null)
            {
                target = loc.Inner;
                target.IsEnabled = cmd.IsEnabled;
            }

            try
            {
                target.SaveConfig();
            }
            catch { }

            if (menu != null && menu.Config != null && !string.IsNullOrEmpty(target.Name))
            {
                try
                {
                    string key = target.Name + ": IsEnabled";
                    var entry = menu.Config.Bind<bool>("Command Settings", key, target.IsEnabled, "Whether the command is enabled");
                    if (entry != null)
                    {
                        entry.Value = target.IsEnabled;
                    }
                    SaveConfigIfManual(menu.Config);
                }
                catch { }
            }

            try
            {
                CustomCommand c = (target != null) ? target : cmd;
                if (c is AutoQuestToggleCommand)
                {
                    AutoQuestPlugin.SetEnabled(c.IsEnabled);
                    if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                }
                else if (c is AutoReturnToggleCommand)
                {
                    AutoQuestPlugin.SetAutoReturnEnabled(c.IsEnabled);
                    if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                }
                else if (c is InstantReturnToggleCommand)
                {
                    AutoQuestPlugin.SetInstantReturnEnabled(c.IsEnabled);
                    if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                }
                else if (c is AutoCollectRewardsToggleCommand)
                {
                    AutoQuestPlugin.SetAutoCollectRewardsEnabled(c.IsEnabled);
                    if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                }
                else if (c is AutoKillEnemiesToggleCommand)
                {
                    AutoQuestPlugin.SetAutoKillEnemiesEnabled(c.IsEnabled);
                    if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                }
                else if (c is BypassQuestRequirementsCommand)
                {
                    if (AutoQuestPlugin.CfgBypassQuestReqs != null)
                    {
                        AutoQuestPlugin.CfgBypassQuestReqs.Value = c.IsEnabled;
                        if (AutoQuestPlugin.Instance != null) SaveConfigIfManual(AutoQuestPlugin.Instance.Config);
                    }
                }
                else if (c is VatsToggleBoostCommand)
                {
                    VatsController.IsBoostEnabled = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is VatsToggleWarpCommand)
                {
                    VatsController.IsTimeWarpEnabled = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is VatsAutoPauseIncidentsCommand)
                {
                    VatsController.AutoPauseIncidents = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is VatsAutoPauseStrangerCommand)
                {
                    VatsController.AutoPauseStranger = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is VatsToggleOverlayCommand)
                {
                    VatsController.ShowOverlay = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is EventAutoDefendToggleCommand)
                {
                    EventManagerController.AutoDefendEnabled = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
                else if (c is EventAutoTapVisitorsToggleCommand)
                {
                    EventManagerController.AutoTapVisitorsEnabled = c.IsEnabled;
                    if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                }
            }
            catch { }
        }

        public static string GetSmartDefault(CustomCommand cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Format)) return "100";
            string fmt = cmd.Format;
            if (ContainsCI(fmt, "/nuka")) return "999999";
            if (ContainsCI(fmt, "/quantum")) return "9999";
            if (ContainsCI(fmt, "/setres")) return "9999";
            if (ContainsCI(fmt, "/setboxes")) return "50";
            if (ContainsCI(fmt, "/setmrhandy")) return "10";
            if (ContainsCI(fmt, "/setpetboxes")) return "25";
            if (ContainsCI(fmt, "/setstarterpack")) return "10";
            if (ContainsCI(fmt, "/setcurie") || ContainsCI(fmt, "/setvictor") || ContainsCI(fmt, "/setpredefinedpack")) return "10";
            if (ContainsCI(fmt, "/spinres")) return "500";
            if (ContainsCI(fmt, "/ultracite")) return "9999";
            if (ContainsCI(fmt, "/setstimrad")) return "9999";
            if (ContainsCI(fmt, "/setstoragelimit") || ContainsCI(fmt, "/setitemlimit")) return "5000";
            if (ContainsCI(fmt, "/setincidentchance")) return "0";
            if (ContainsCI(fmt, "/gendweller") || ContainsCI(fmt, "/gencustomdweller") || ContainsCI(fmt, "/genuniquedweller")) return "legendary";
            if (ContainsCI(fmt, "/vatsmult")) return "10";
            if (ContainsCI(fmt, "/vatsanim")) return "3";
            return "100";
        }

        private static string[] GetPresetsForCommand(CustomCommand cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Format)) return null;
            string fmt = cmd.Format;
            if (ContainsCI(fmt, "/nuka")) return PresetsNuka;
            if (ContainsCI(fmt, "/quantum")) return PresetsQuantum;
            if (ContainsCI(fmt, "/setres")) return PresetsRes;
            if (ContainsCI(fmt, "/setboxes") || ContainsCI(fmt, "/setpetboxes")) return PresetsBoxes;
            if (ContainsCI(fmt, "/setmrhandy") || ContainsCI(fmt, "/setstarterpack") || ContainsCI(fmt, "/setcurie") || ContainsCI(fmt, "/setvictor") || ContainsCI(fmt, "/setpredefinedpack") || ContainsCI(fmt, "/setquantumbox")) return PresetsPacks;
            if (ContainsCI(fmt, "/setstimrad")) return PresetsQuantum;
            if (ContainsCI(fmt, "/setstoragelimit") || ContainsCI(fmt, "/setitemlimit")) return PresetsStorage;
            if (ContainsCI(fmt, "/gendweller") || ContainsCI(fmt, "/gencustomdweller") || ContainsCI(fmt, "/genuniquedweller")) return PresetsDweller;
            if (ContainsCI(fmt, "/vatsmult")) return PresetsVatsMult;
            if (ContainsCI(fmt, "/vatsanim")) return PresetsVatsAnim;
            return PresetsDefault;
        }

        public static void DrawThemedPopup(global::CTDynamicModMenu.CTDynamicModMenu menu, FalloutTheme theme)
        {
            if (menu == null) return;
            EnsureStyles();

            if (Event.current == null || Event.current.type == EventType.Repaint)
            {
                menu.EnableCursor();
            }

            var selectedCmd = GetField(_refSelectedCommand, menu, null);
            if (selectedCmd == null)
            {
                SetField(_refShowPopup, menu, false);
                SetField(_refShowMenu, menu, true);
                return;
            }

            float popupWidth = S(480f);
            float popupHeight = S(270f);
            float popupX = (Screen.width - popupWidth) / 2f;
            float popupY = (Screen.height - popupHeight) / 2f;
            Rect popupRect = new Rect(popupX, popupY, popupWidth, popupHeight);

            // Ana CRT arka plan ve parlak cerceve
            DrawBoxWithBorder(popupRect, theme.HeaderBgColor, theme.BorderColor, S(2.5f));

            // Baslik Barı
            float headerH = S(32f);
            Rect headerRect = new Rect(popupX, popupY, popupWidth, headerH);
            DrawRect(headerRect, theme.BgColor);
            DrawRect(new Rect(popupX, popupY + headerH - S(1.5f), popupWidth, S(1.5f)), theme.BorderColor);

            var titleStyle = PrepLabel(SI(13f), FontStyle.Bold, TextAnchor.MiddleLeft, theme.AccentColor);
            string titleText = MenuPlugin.IsTurkish ? "ROBCO PIP-BOY 3000 // DEĞER BELİRLE" : "ROBCO PIP-BOY 3000 // SET VALUE";
            GUI.Label(new Rect(popupX + S(12f), popupY + S(4f), popupWidth - S(24f), S(24f)), titleText, titleStyle);

            // Komut Adi ve Bilgilendirme
            var nameStyle = PrepLabel(SI(13f), FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            GUI.Label(new Rect(popupX + S(10f), popupY + S(42f), popupWidth - S(20f), S(24f)), selectedCmd.Name, nameStyle);

            var hintStyle = PrepLabel(SI(11f), FontStyle.Italic, TextAnchor.MiddleCenter, theme.TextColor * 0.9f);
            string hintText = MenuPlugin.IsTurkish
                ? "Miktar girin veya aşağıdaki hızlı butonlardan birini seçin:"
                : "Enter amount or select one of the quick presets below:";
            GUI.Label(new Rect(popupX + S(10f), popupY + S(66f), popupWidth - S(20f), S(20f)), hintText, hintStyle);

            // Metin Kutusu (Input Field)
            string curInput = GetField(_refUserInput, menu, "") ?? "";
            Rect inputRect = new Rect(popupX + S(50f), popupY + S(92f), popupWidth - S(100f), S(34f));
            DrawBoxWithBorder(inputRect, theme.ButtonBg, theme.AccentColor, S(1.5f));

            _styleTextField.fontSize = SI(14f);
            _styleTextField.normal.textColor = theme.AccentColor;
            string newInput = GUI.TextField(new Rect(inputRect.x + S(4f), inputRect.y + S(2f), inputRect.width - S(8f), inputRect.height - S(4f)), curInput, _styleTextField);
            if (newInput != curInput)
            {
                curInput = newInput;
                SetField(_refUserInput, menu, curInput);
            }

            // Hizli Secenek Butonlari (Presets)
            string[] presets = GetPresetsForCommand(selectedCmd);
            if (presets != null && presets.Length > 0)
            {
                float presetY = popupY + S(136f);
                float pPad = S(6f);
                float pWidth = (popupWidth - S(60f) - (presets.Length - 1) * pPad) / presets.Length;
                float pHeight = S(28f);
                int presetFontSize = SI(11f);

                for (int i = 0; i < presets.Length; i++)
                {
                    float pX = popupX + S(30f) + i * (pWidth + pPad);
                    Rect pRect = new Rect(pX, presetY, pWidth, pHeight);
                    bool isPHover = pRect.Contains(Event.current.mousePosition);

                    DrawBoxWithBorder(pRect, isPHover ? theme.ButtonHover : theme.ButtonBg, isPHover ? theme.AccentColor : theme.BorderColor * 0.5f, S(1f));

                    var pStyle = PrepLabel(presetFontSize, FontStyle.Bold, TextAnchor.MiddleCenter, isPHover ? theme.AccentColor : theme.TextColor);

                    if (GUI.Button(pRect, presets[i], pStyle))
                    {
                        string raw = presets[i].Replace(",", "").Replace(".", "").Trim();
                        SetField(_refUserInput, menu, raw);
                    }
                }
            }

            // Alt Butonlar: Iptal ve Onayla
            float btnY = popupY + popupHeight - S(52f);
            float btnW = (popupWidth - S(70f)) / 2f;
            float btnH = S(38f);
            int actionFontSize = SI(12.5f);

            // IPTAL BUTONU
            Rect cancelRect = new Rect(popupX + S(25f), btnY, btnW, btnH);
            bool isCancelHover = cancelRect.Contains(Event.current.mousePosition);
            DrawBoxWithBorder(cancelRect, isCancelHover ? new Color(theme.ToggleOff.r, theme.ToggleOff.g, theme.ToggleOff.b, 0.35f) : theme.ButtonBg,
                isCancelHover ? theme.ToggleOff : theme.BorderColor * 0.7f, isCancelHover ? S(2f) : S(1.2f));

            var cancelStyle = PrepLabel(actionFontSize, FontStyle.Bold, TextAnchor.MiddleCenter, isCancelHover ? Color.white : theme.ToggleOff);
            string cancelLabel = MenuPlugin.IsTurkish ? "<b>✕ İPTAL</b>" : "<b>✕ CANCEL</b>";

            if (GUI.Button(cancelRect, cancelLabel, cancelStyle))
            {
                SetField(_refShowPopup, menu, false);
                SetField(_refShowMenu, menu, true);
                SetField(_refSelectedCommand, menu, null);
                SetField(_refUserInput, menu, "");
            }

            // ONAYLA BUTONU
            Rect confirmRect = new Rect(popupX + S(45f) + btnW, btnY, btnW, btnH);
            bool isConfirmHover = confirmRect.Contains(Event.current.mousePosition);
            DrawBoxWithBorder(confirmRect, isConfirmHover ? Color.Lerp(theme.ButtonHover, theme.ToggleOn, 0.45f) : Color.Lerp(theme.ButtonBg, theme.ToggleOn, 0.22f),
                isConfirmHover ? Color.white : theme.ToggleOn, isConfirmHover ? S(2f) : S(1.5f));

            var confirmStyle = PrepLabel(actionFontSize, FontStyle.Bold, TextAnchor.MiddleCenter, isConfirmHover ? Color.white : theme.ToggleOn);
            string confirmLabel = MenuPlugin.IsTurkish ? "<b>✔ ONAYLA</b>" : "<b>✔ CONFIRM</b>";

            if (GUI.Button(confirmRect, confirmLabel, confirmStyle))
            {
                try
                {
                    string val = (curInput ?? "").Replace(",", "").Replace(".", "").Trim();
                    if (string.IsNullOrEmpty(val))
                    {
                        val = GetSmartDefault(selectedCmd);
                    }
                    string cmdPrefix = selectedCmd.Format.Split(' ')[0];
                    string full = cmdPrefix + " " + val;
                    selectedCmd.Execute(CommandInput.Parse(full));
                    SetField(_refLastDisplayedMessage, menu, selectedCmd.Name + " (" + val + ") uygulandi.");
                    try
                    {
                        if (menu != null) SaveConfigIfManual(menu.Config);
                        if (MenuPlugin.Instance != null) SaveConfigIfManual(MenuPlugin.Instance.Config);
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    if (MenuPlugin.Log != null)
                        MenuPlugin.Log.LogWarning("Parametre komut hatasi: " + ex.Message);
                }

                SetField(_refShowPopup, menu, false);
                SetField(_refShowMenu, menu, true);
                SetField(_refSelectedCommand, menu, null);
                SetField(_refUserInput, menu, "");
            }
        }
    }

    /// <summary>
    /// Orijinal OnGUI metodunu Harmony ile yakalayip temali OnGUI ile degistiren yama.
    /// </summary>
    [HarmonyPatch(typeof(global::CTDynamicModMenu.CTDynamicModMenu), "OnGUI")]
    internal static class ThemedOnGUIPatch
    {
        private static bool Prefix(global::CTDynamicModMenu.CTDynamicModMenu __instance)
        {
            FalloutThemeManager.DrawThemedOnGUI(__instance);
            return false;
        }
    }
}
