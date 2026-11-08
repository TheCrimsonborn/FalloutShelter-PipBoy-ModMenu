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
            if (_pixelTex == null)
            {
                _pixelTex = new Texture2D(1, 1);
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
            var trav = Traverse.Create(menu);
            bool showMenu = trav.Field<bool>("showMenu").Value;
            bool showMenuButton = trav.Field<bool>("showMenuButton").Value;
            bool showKeybindText = trav.Field<bool>("showKeybindText").Value;
            var theme = Current;

            float autoScale = Screen.height / 1080f;
            float uiScale = trav.Field<float>("uiScaleMultiplier").Value;
            float scale = autoScale * uiScale;
            Func<float, float> S = v => v * scale;
            Func<float, int> SI = v => Mathf.Max(10, (int)(v * scale));

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
                    var keyStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = SI(11f),
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                    keyStyle.normal.textColor = theme.AccentColor;
                    string keyText = MenuPlugin.IsTurkish ? "▶ F4: Mod Menüsü" : "▶ F4: Mod Menu";
                    GUI.Label(new Rect(btnX, btnY - S(17f), S(220f), S(17f)), keyText, keyStyle);
                }

                if (showMenuButton)
                {
                    Rect openBtnRect = new Rect(btnX, btnY, btnW, btnH);
                    DrawBoxWithBorder(openBtnRect, theme.BgColor, theme.BorderColor, S(2f));

                    var openStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = SI(12f),
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    openStyle.normal.textColor = theme.AccentColor;
                    string btnLabel = MenuPlugin.IsTurkish ? "⚙ MOD MENÜSÜ (F4)" : "⚙ MOD MENU (F4)";

                    if (GUI.Button(openBtnRect, btnLabel, openStyle))
                    {
                        trav.Field("showMenu").SetValue(true);
                        menu.EnableCursor();
                    }
                }
            }

            // 2. Log penceresi
            bool showLog = trav.Field<bool>("showLogWindow").Value;
            if (showLog)
            {
                try { trav.Method("DrawLogWindow").GetValue(); } catch { }
            }

            // 3. Menu ACIKKEN: Fallout Temali Mod Menusu
            if (showMenu)
            {
                menu.EnableCursor();
                DrawThemedMenu(menu);
            }

            // 4. Diger popup pencereler
            bool showPopup = trav.Field<bool>("showPopup").Value;
            if (showPopup)
            {
                try { trav.Method("ShowPopupForUserInput").GetValue(); } catch { }
            }

            bool showKeybindPicker = trav.Field<bool>("showKeybindPicker").Value;
            if (showKeybindPicker)
            {
                try { trav.Method("DrawKeybindPicker").GetValue(); } catch { }
            }

            bool showCommand = trav.Field<bool>("showCommandWindow").Value;
            if (showCommand)
            {
                try { trav.Method("DrawCommandWindow").GetValue(); } catch { }
            }
        }

        /// <summary>
        /// Fallout temali ana menuyu cizer. Sekmelerin disari tasmamasi icin dinamik kaydirma ve satir sarma uygular.
        /// </summary>
        public static void DrawThemedMenu(global::CTDynamicModMenu.CTDynamicModMenu menu)
        {
            var trav = Traverse.Create(menu);
            var pos = trav.Field<Vector2>("menuPosition").Value;
            var isDragging = trav.Field<bool>("isDragging").Value;
            var dragOffset = trav.Field<Vector2>("dragOffset").Value;
            var selectedCategory = trav.Field<string>("selectedCategory").Value;
            var registeredCommands = trav.Field<List<CustomCommand>>("registeredCommands").Value;
            var uiScale = trav.Field<float>("uiScaleMultiplier").Value;

            float autoScale = Screen.height / 1080f;
            float scale = autoScale * uiScale;
            Func<float, float> S = v => v * scale;
            Func<float, int> SI = v => Mathf.Max(10, (int)(v * scale));

            var theme = Current;

            // Kategorileri topla
            var categories = new List<string>();
            var catSet = new HashSet<string>();
            if (registeredCommands != null)
            {
                foreach (var cmd in registeredCommands)
                {
                    if (cmd != null && catSet.Add(cmd.Category))
                    {
                        categories.Add(cmd.Category);
                    }
                }
            }

            // Temalar sekmesini ozel olarak kategorilere ekleyelim
            string themesTabName = MenuPlugin.IsTurkish ? "Temalar" : "Themes";
            if (!categories.Contains(themesTabName))
            {
                categories.Add(themesTabName);
            }

            if (string.IsNullOrEmpty(selectedCategory) || (!catSet.Contains(selectedCategory) && selectedCategory != themesTabName))
            {
                selectedCategory = categories.Count > 0 ? categories[0] : "";
                trav.Field("selectedCategory").SetValue(selectedCategory);
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
            foreach (var cat in categories)
            {
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

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(15f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = theme.AccentColor;
            GUI.Label(new Rect(pos.x + S(12f), pos.y + S(4f), windowWidth - S(120f), S(22f)), theme.HeaderTag, titleStyle);

            var mottoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(11f),
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleLeft
            };
            mottoStyle.normal.textColor = theme.TextColor;
            GUI.Label(new Rect(pos.x + S(14f), pos.y + S(24f), windowWidth - S(120f), S(18f)), theme.Motto, mottoStyle);

            // Hizli Dil Degistirme Butonu (Sag Ust)
            float langBtnWidth = S(85f);
            Rect langBtnRect = new Rect(pos.x + windowWidth - langBtnWidth - S(10f), pos.y + S(8f), langBtnWidth, S(28f));
            bool isLangHover = langBtnRect.Contains(Event.current.mousePosition);

            DrawBoxWithBorder(langBtnRect, isLangHover ? theme.ButtonHover : theme.ButtonBg, isLangHover ? theme.AccentColor : theme.BorderColor * 0.7f, isLangHover ? S(2f) : S(1.2f));

            var langStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(11f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            langStyle.normal.textColor = isLangHover ? theme.AccentColor : theme.TextColor;
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

            var arrowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(12f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            arrowStyle.normal.textColor = isPrevHover ? theme.AccentColor : theme.TextColor;

            if (GUI.Button(prevBtnRect, "◄", arrowStyle))
            {
                PrevTheme();
            }

            var themeNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(13f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            themeNameStyle.normal.textColor = theme.AccentColor;
            string themeLabel = (MenuPlugin.IsTurkish ? "TEMA: " : "THEME: ") + theme.DisplayName + " (" + (CurrentThemeIndex + 1) + "/10)";
            GUI.Label(new Rect(themeBarRect.x + S(45f), themeBarRect.y, themeBarRect.width - S(90f), themeBarHeight), themeLabel, themeNameStyle);

            Rect nextBtnRect = new Rect(themeBarRect.x + themeBarRect.width - S(39f), themeBarRect.y + S(3f), S(35f), themeBarHeight - S(6f));
            bool isNextHover = nextBtnRect.Contains(Event.current.mousePosition);
            DrawBoxWithBorder(nextBtnRect, isNextHover ? theme.ButtonHover : theme.ButtonBg, isNextHover ? theme.AccentColor : theme.BorderColor * 0.5f, S(1f));

            var nextArrowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(12f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            nextArrowStyle.normal.textColor = isNextHover ? theme.AccentColor : theme.TextColor;

            if (GUI.Button(nextBtnRect, "►", nextArrowStyle))
            {
                NextTheme();
            }

            // Kategori Sekmelerini Ciz (Satir Sarmali - Asla Disari Tasmaz!)
            float renderTabX = tabStartX;
            float renderTabY = themeBarRect.y + themeBarHeight + S(8f);

            foreach (var cat in categories)
            {
                float tabW = S(cat.Length * 8f + 16f);
                if (renderTabX + tabW > maxTabRight && renderTabX > tabStartX)
                {
                    renderTabX = tabStartX;
                    renderTabY += tabH + tabGapY;
                }

                Rect tabRect = new Rect(renderTabX, renderTabY, tabW, tabH);
                bool isActive = cat == selectedCategory;

                DrawBoxWithBorder(tabRect, isActive ? theme.ActiveTab : theme.InactiveTab, isActive ? theme.AccentColor : theme.BorderColor * 0.45f, isActive ? S(2f) : S(1f));

                var tabStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = SI(11.5f),
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal,
                    alignment = TextAnchor.MiddleCenter
                };
                tabStyle.normal.textColor = isActive ? theme.AccentColor : theme.TextColor;

                if (GUI.Button(tabRect, cat, tabStyle))
                {
                    selectedCategory = cat;
                    trav.Field("selectedCategory").SetValue(selectedCategory);
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
                DrawThemesGrid(contentBoxRect, S, SI, theme);
            }
            else
            {
                DrawCommandsGrid(contentBoxRect, selectedCategory, registeredCommands, S, SI, theme);
            }

            curY += contentHeight + S(8f);

            // Bilgi / Aciklama (Tooltip) Paneli
            Rect infoRect = new Rect(pos.x + S(10f), curY, windowWidth - S(20f), infoHeight);
            DrawBoxWithBorder(infoRect, theme.HeaderBgColor * 0.9f, theme.BorderColor * 0.6f, S(1f));

            var infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(11.5f),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            infoStyle.normal.textColor = theme.TextColor;

            string infoText;
            if (_hoveredCmd != null)
            {
                infoText = "<color=#" + ColorUtility.ToHtmlStringRGB(theme.AccentColor) + "><b>" + _hoveredCmd.Name + ":</b></color> " + _hoveredCmd.Description;
            }
            else
            {
                infoText = "<color=#" + ColorUtility.ToHtmlStringRGB(theme.AccentColor) + "><b>ROBCO PIP-BOY 3000 //</b></color> " +
                    (MenuPlugin.IsTurkish ? "Detay ve aciklama icin bir dugmenin uzerine gelin veya tiklayin." : "Hover over a button to view details and descriptions.");
            }
            GUI.Label(new Rect(infoRect.x + S(10f), infoRect.y + S(4f), infoRect.width - S(20f), infoHeight - S(8f)), infoText, infoStyle);

            curY += infoHeight + S(10f);

            // Alt Bar: Olcek Cubugu & Kapat Butonu
            var sliderLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(11f),
                alignment = TextAnchor.MiddleLeft
            };
            sliderLabelStyle.normal.textColor = theme.TextColor;
            GUI.Label(new Rect(pos.x + S(14f), curY, S(90f), bottomHeight), MenuPlugin.IsTurkish ? "Olcek:" : "Scale:", sliderLabelStyle);

            float sliderWidth = S(180f);
            float newScale = GUI.HorizontalSlider(new Rect(pos.x + S(100f), curY + S(10f), sliderWidth, S(16f)), uiScale, 0.5f, 2.0f);
            if (Mathf.Abs(newScale - uiScale) > 0.01f)
            {
                trav.Field("uiScaleMultiplier").SetValue(newScale);
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

            var closeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(13f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            closeStyle.normal.textColor = isCloseHover ? Color.white : theme.ToggleOff;
            string closeText = MenuPlugin.IsTurkish ? "<b>✕ MENÜYÜ KAPAT</b>" : "<b>✕ CLOSE MENU</b>";

            if (GUI.Button(closeBtnRect, closeText, closeStyle))
            {
                trav.Field("showMenu").SetValue(false);
                menu.RecoverCursorState();
            }

            // Pencere Surukleme Islemleri
            var ev = Event.current;
            if (ev.type == EventType.MouseDown && headerRect.Contains(ev.mousePosition))
            {
                isDragging = true;
                dragOffset = ev.mousePosition - pos;
                trav.Field("isDragging").SetValue(true);
                trav.Field("dragOffset").SetValue(dragOffset);
                ev.Use();
            }
            else if (ev.type == EventType.MouseDrag && isDragging)
            {
                pos = ev.mousePosition - dragOffset;
                trav.Field("menuPosition").SetValue(pos);
                ev.Use();
            }
            else if (ev.type == EventType.MouseUp && isDragging)
            {
                isDragging = false;
                trav.Field("isDragging").SetValue(false);
            }
        }

        private static void DrawThemesGrid(Rect area, Func<float, float> S, Func<float, int> SI, FalloutTheme curTheme)
        {
            float padding = S(12f);
            float colWidth = (area.width - padding * 3f) / 2f;
            float btnHeight = S(46f);

            var btnStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(12.5f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

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

                btnStyle.normal.textColor = isSelected || isThemeHover ? t.AccentColor : t.TextColor;
                string activeText = MenuPlugin.IsTurkish ? " (AKTİF)" : " (ACTIVE)";
                string label = (isSelected ? "★ " : "") + t.DisplayName + (isSelected ? activeText : "");

                if (GUI.Button(btnRect, label, btnStyle))
                {
                    SetTheme(i);
                }
            }
        }

        private static void DrawCommandsGrid(Rect area, string category, List<CustomCommand> allCommands, Func<float, float> S, Func<float, int> SI, FalloutTheme theme)
        {
            var cmds = new List<CustomCommand>();
            if (allCommands != null)
            {
                foreach (var c in allCommands)
                {
                    if (c != null && c.Category == category)
                    {
                        cmds.Add(c);
                    }
                }
            }

            float padding = S(10f);
            float btnWidth = (area.width - padding * 4f) / 3f; // 3 sutunlu ferah duzen
            float btnHeight = S(38f);
            int cols = 3;
            int rows = Mathf.CeilToInt((float)cmds.Count / cols);
            float totalHeight = rows * (btnHeight + padding) + padding;

            Rect viewRect = new Rect(0, 0, area.width - S(20f), totalHeight);
            _commandsScroll = GUI.BeginScrollView(new Rect(area.x + S(4f), area.y + S(4f), area.width - S(8f), area.height - S(8f)), _commandsScroll, viewRect);

            var cmdStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SI(11.5f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

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
                string statusText = "";

                if (cmd.IsToggle)
                {
                    string onTag = MenuPlugin.IsTurkish ? "[AÇIK]" : "[ON]";
                    string offTag = MenuPlugin.IsTurkish ? "[KAPALI]" : "[OFF]";

                    if (cmd.IsEnabled)
                    {
                        btnBg = isHover ? Color.Lerp(theme.ButtonHover, theme.ToggleOn, 0.45f) : Color.Lerp(theme.ButtonBg, theme.ToggleOn, 0.22f);
                        borderColor = theme.ToggleOn;
                        textColor = isHover ? Color.white : theme.ToggleOn;
                        statusText = " <color=#" + ColorUtility.ToHtmlStringRGB(theme.ToggleOn) + "><b>" + onTag + "</b></color>";
                    }
                    else
                    {
                        btnBg = isHover ? theme.ButtonHover : theme.ButtonBg;
                        textColor = isHover ? theme.TextColor : theme.TextColor * 0.85f;
                        statusText = " <color=#" + ColorUtility.ToHtmlStringRGB(theme.ToggleOff) + ">" + offTag + "</color>";
                    }
                }

                // Temaya ozel Fallout Pip-Boy terminal kutusu ve parlayan kenarlik
                float bWidth = isHover || (cmd.IsToggle && cmd.IsEnabled) ? S(2f) : S(1.2f);
                DrawBoxWithBorder(bRect, btnBg, borderColor, bWidth);

                // Pip-Boy terminal cam yansimasi / ust vurgusu
                Color topGlow = isHover ? theme.AccentColor * 0.85f : (cmd.IsToggle && cmd.IsEnabled ? theme.ToggleOn * 0.7f : theme.BorderColor * 0.25f);
                DrawRect(new Rect(bRect.x + S(2f), bRect.y + S(1f), bRect.width - S(4f), S(1f)), topGlow);

                cmdStyle.normal.textColor = textColor;
                string fullLabel = cmd.Name + statusText;

                if (GUI.Button(bRect, fullLabel, cmdStyle))
                {
                    if (cmd.IsToggle)
                    {
                        cmd.IsEnabled = !cmd.IsEnabled;
                    }
                    else
                    {
                        cmd.Execute(new CommandInput());
                    }
                }
            }

            GUI.EndScrollView();
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
