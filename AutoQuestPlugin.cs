using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FSAutoQuest
{
    /// <summary>
    /// Otomatik gorev modulu.
    /// - Gorev ekibini maksimum Stimpak / RadAway ile yola cikarir (kasada stok doldurur,
    ///   gorev baslayinca gorev envanterini tamamlar).
    /// - Gorev sirasinda sakinleri otomatik iyilestirir (Stimpak / RadAway).
    /// - Aktif gorevin hedef odalarini sirayla tamamlar, gorevi basariyla bitirir
    ///   ve kasaya doner.
    /// F6 = otomasyonu ac/kapa
    /// </summary>
    [BepInPlugin("com.halil.fsautoquest", "FSAutoQuest - Otomatik Gorev", "0.2.0")]
    public class AutoQuestPlugin : BaseUnityPlugin
    {
        // ------------------------------------------------------------ config
        public static ConfigEntry<bool> CfgEnabled;
        public static ConfigEntry<bool> CfgBypassQuestReqs;
        internal static ConfigEntry<bool> CfgAutoQuest;
        internal static ConfigEntry<bool> CfgAutoHeal;
        internal static ConfigEntry<bool> CfgAutoReturn;
        public static ConfigEntry<bool> CfgInstantReturn;
        internal static ConfigEntry<bool> CfgRefill;
        internal static ConfigEntry<bool> CfgAutoRevive;
        internal static ConfigEntry<int> CfgStimpak;
        internal static ConfigEntry<int> CfgRadaway;
        internal static ConfigEntry<float> CfgInterval;
        internal static ConfigEntry<bool> CfgVerbose;
        internal static ConfigEntry<bool> CfgAutoKillEnemies;
        internal static ConfigEntry<bool> CfgAutoCollectRewards;
        public static ConfigEntry<bool> CfgFreeShop;

        internal static ManualLogSource Log;

        private const string TraceFlagName = "FSAutoQuest-trace.on";

        private float _nextActionTime;
        private float _nextHealTime;
        private float _nextRefillTime;
        private float _nextSweepTime;
        private float _nextRoomCacheRefreshTime;
        private float _nextVaultTeamScanTime;

        private string _questId;
        private float _questSeenAt;
        private readonly Dictionary<int, float> _roomClearedAt = new Dictionary<int, float>();
        private UnityEngine.Object[] _cachedQuestRooms;
        private string _cachedQuestRoomsId;

        private bool _resultShown;
        private float _resultShownAt;
        private bool _exitRequested;
        private float _exitRequestedAt;

        private bool _wasInQuest;
        private string _pendingCompletionQuestId;
        private int _completionAttempts;
        private float _nextCompletionTime;
        private int _pendingQuestLunchboxes;
        private int _pendingQuestPetCarriers;
        private int _pendingQuestMrHandy;

        private string _traceFlagPath;
        private bool _traceEnabledCached;
        private float _nextTraceCheckTime;

        private UnityEngine.Object _cachedThemeWin;
        private UnityEngine.Object _cachedRecipeWin;
        private UnityEngine.Object _cachedArrivalWin;

        private bool _inputErrorLogged;
        private readonly HashSet<string> _methodMissLogged = new HashSet<string>();

        // ------------------------------------------------------------ max level queue
        private readonly List<object> _levelQueue = new List<object>();
        private float _nextLevelTime;
        private int _levelTotal;
        private int _levelDone;

        // ------------------------------------------------------------ mod menu API
        /// <summary>Calisan eklenti ornegi (mod menu adaptoru kullanir).</summary>
        public static AutoQuestPlugin Instance { get; private set; }

        /// <summary>Otomasyon acik mi? (mod menu toggle'i dogrudan bunu kullanir)</summary>
        public static bool IsEnabled
        {
            get { return CfgEnabled != null && CfgEnabled.Value; }
            set { SetEnabled(value); }
        }

        /// <summary>Otomasyonu ac/kapa. F6 ve mod menu ayni yolu kullanir.</summary>
        public static void SetEnabled(bool value)
        {
            if (CfgEnabled == null) return;
            bool changed = CfgEnabled.Value != value;
            CfgEnabled.Value = value;

            var inst = Instance;
            if (!value && inst != null)
            {
                inst._resultShown = false;
                inst._exitRequested = false;
            }

            if (changed && Log != null)
                Log.LogInfo("Otomasyon: " + (value ? "ACIK" : "KAPALI"));
        }

        public static void ToggleEnabled()
        {
            SetEnabled(!IsEnabled);
        }

        public static bool IsAutoReturnEnabled
        {
            get { return CfgAutoReturn != null && CfgAutoReturn.Value; }
            set { SetAutoReturnEnabled(value); }
        }

        public static void SetAutoReturnEnabled(bool value)
        {
            if (CfgAutoReturn == null) return;
            bool changed = CfgAutoReturn.Value != value;
            CfgAutoReturn.Value = value;
            if (changed && Log != null)
                Log.LogInfo("Otomatik Kasaya Donus: " + (value ? "ACIK" : "KAPALI"));
        }

        public static bool IsInstantReturnEnabled
        {
            get { return CfgInstantReturn != null && CfgInstantReturn.Value; }
            set { SetInstantReturnEnabled(value); }
        }

        public static void SetInstantReturnEnabled(bool value)
        {
            if (CfgInstantReturn == null) return;
            bool changed = CfgInstantReturn.Value != value;
            CfgInstantReturn.Value = value;
            if (changed && Log != null)
                Log.LogInfo("Aninda Kapiya Ulas: " + (value ? "ACIK" : "KAPALI"));
        }

        public static bool IsAutoKillEnemiesEnabled
        {
            get { return CfgAutoKillEnemies == null || CfgAutoKillEnemies.Value; }
            set { SetAutoKillEnemiesEnabled(value); }
        }

        public static void SetAutoKillEnemiesEnabled(bool value)
        {
            if (CfgAutoKillEnemies == null) return;
            bool changed = CfgAutoKillEnemies.Value != value;
            CfgAutoKillEnemies.Value = value;
            if (changed && Log != null)
                Log.LogInfo("Dusmanlari Otomatik Oldur: " + (value ? "ACIK" : "KAPALI"));
        }

        public static bool IsAutoCollectRewardsEnabled
        {
            get { return CfgAutoCollectRewards == null || CfgAutoCollectRewards.Value; }
            set { SetAutoCollectRewardsEnabled(value); }
        }

        public static void SetAutoCollectRewardsEnabled(bool value)
        {
            if (CfgAutoCollectRewards == null) return;
            bool changed = CfgAutoCollectRewards.Value != value;
            CfgAutoCollectRewards.Value = value;
            if (changed && Log != null)
                Log.LogInfo("Tarif/Odul Otomatik Topla: " + (value ? "ACIK" : "KAPALI"));
        }

        public static bool IsFreeShopEnabled
        {
            get { return CfgFreeShop != null && CfgFreeShop.Value; }
            set { SetFreeShopEnabled(value); }
        }

        public static void SetFreeShopEnabled(bool value)
        {
            if (CfgFreeShop == null) return;
            bool changed = CfgFreeShop.Value != value;
            CfgFreeShop.Value = value;
            if (changed && Log != null)
                Log.LogInfo("Ucretsiz Magaza (Pack'ler): " + (value ? "ACIK" : "KAPALI"));
        }

        /// <summary>Mod menuden "Sakinleri Max Seviye Yap" icin.</summary>
        public static void QueueMaxLevelDwellers()
        {
            var inst = Instance;
            if (inst == null)
            {
                if (Log != null) Log.LogWarning("Max seviye: eklenti ornegi yok.");
                return;
            }
            inst.QueueMaxLevelInternal();
        }

        // ------------------------------------------------------------ lifecycle
        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Application.runInBackground = true;

            CfgEnabled = Config.Bind("Genel", "Etkin", true,
                "Otomatik gorev modulu acik mi? (F6 ile de degistirilebilir)");
            CfgBypassQuestReqs = Config.Bind("Genel", "GorevSartlariniKaldir", false,
                "Gorevler icin seviye, silah, kiyafet ve SPECIAL gereksinimlerini kaldirir.");
            CfgAutoQuest = Config.Bind("Genel", "GoreviOtomatikBitir", true,
                "Gorev hedeflerini otomatik tamamla.");
            CfgAutoHeal = Config.Bind("Genel", "OtomatikIyilestir", true,
                "Gorev sirasinda sakinleri Stimpak/RadAway ile otomatik iyilestir.");
            CfgAutoReturn = Config.Bind("Genel", "KasaDonusu", false,
                "Gorev basarili olunca otomatik olarak kasaya don. (Kapaliysa kullanicinin Return to Vault butonuna basmasini bekler)");
            CfgInstantReturn = Config.Bind("Genel", "AnindaKapiyaUlas", true,
                "Gorev bittiginde saatlerce yurume suresini beklemeden ekibi aninda siginak kapisina ulastirir.");
            CfgAutoKillEnemies = Config.Bind("Genel", "DusmanlariOldur", true,
                "Gorevdeki tum dusman NPC'leri otomatik oldur. (Dost ve diyalog NPC'lerine asla dokunmaz)");
            CfgAutoCollectRewards = Config.Bind("Genel", "OtomatikOdulTopla", true,
                "Gorev ve kesif sonrasi cikan tarif parcasi (Recipe Fragment), esya ve odul pencerelerini otomatik toplayip kapatir.");
            CfgFreeShop = Config.Bind("Magaza", "UcretsizPackler", true,
                "Magazadaki pack'ler (Lunchbox, Pet Carrier, Mr. Handy, Starter Pack, Nuka) satin alinirken odeme yapilmaz; icerik dogrudan hesaba eklenir.");
            CfgAutoRevive = Config.Bind("Genel", "OtomatikCanlandir", true,
                "Gorevde olen sakinleri otomatik canlandir.");
            CfgRefill = Config.Bind("Sarf", "SarfMalzemeTamamla", true,
                "Gorev sirasinda Stimpak/RadAway biterse tekrar doldur.");
            CfgStimpak = Config.Bind("Sarf", "StimpakAdedi", 9999,
                "Gorev ekibinin sahip olacagi Stimpak adedi.");
            CfgRadaway = Config.Bind("Sarf", "RadawayAdedi", 9999,
                "Gorev ekibinin sahip olacagi RadAway adedi.");
            CfgInterval = Config.Bind("Zamanlama", "IslemAraligi", 0.4f,
                "Iki gorev islemi arasindaki minimum saniye.");
            CfgVerbose = Config.Bind("Zamanlama", "AyrintiliLog", false,
                "Ayrintili (her islem) log kaydi.");

            try { _traceFlagPath = Path.Combine(Paths.BepInExRootPath, TraceFlagName); }
            catch { _traceFlagPath = null; }
            _nextTraceCheckTime = 0f;

            Logger.LogInfo("FSAutoQuest 0.2.0 yuklendi. F6 = otomasyon ac/kapa, F4 = mod menu.");
            Logger.LogInfo("Ayarlar: stimpak=" + CfgStimpak.Value + " radaway=" + CfgRadaway.Value +
                           " autoQuest=" + CfgAutoQuest.Value + " autoHeal=" + CfgAutoHeal.Value);
        }

        private void Update()
        {
            try
            {
                HandleHotkeys();
                ProcessLevelQueue();
                ProcessRewardWindows();

                if (!CfgEnabled.Value) return;
                if (Time.timeScale <= 0f) return;

                if (Time.frameCount % 30 == 0 && TraceEnabled())
                    Logger.LogInfo(TraceLine());

                object qs, qdm;
                string qid;
                if (TryGetActiveQuest(out qs, out qdm, out qid))
                {
                    _wasInQuest = true;
                    TickQuest(qs, qdm, qid);
                }
                else
                {
                    if (_wasInQuest)
                    {
                        // gorevden ciktik: bir sonraki gorev icin durumu sifirla
                        _wasInQuest = false;
                        _questId = null;
                        _resultShown = false;
                        _exitRequested = false;
                        _roomClearedAt.Clear();
                        _cachedQuestRooms = null;
                        _cachedQuestRoomsId = null;
                        _nextSweepTime = 0f;
                        _nextRoomCacheRefreshTime = 0f;
                    }
                    TickVault();
                }
            }
            catch (Exception e)
            {
                if (!_inputErrorLogged)
                {
                    _inputErrorLogged = true;
                    Logger.LogError("Update hatasi (bir kez loglanir): " + e);
                }
            }
        }

        // ------------------------------------------------------------ hotkeys
        private void HandleHotkeys()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f6Key.wasPressedThisFrame)
            {
                ToggleEnabled();
                if (!CfgEnabled.Value)
                {
                    _resultShown = false;
                    _exitRequested = false;
                }
            }
        }

        // ------------------------------------------------------------ max level queue impl
        private void QueueMaxLevelInternal()
        {
            _levelQueue.Clear();
            _levelTotal = 0;
            _levelDone = 0;

            var dm = Singleton("DwellerManager");
            if (dm == null)
            {
                Logger.LogWarning("Max seviye: DwellerManager bulunamadi.");
                return;
            }

            var dwellers = M(dm, "Dwellers") as IList;
            if (dwellers == null || dwellers.Count == 0)
            {
                Logger.LogWarning("Max seviye: sakin listesi bos.");
                return;
            }

            foreach (var d in dwellers)
            {
                if (d == null) continue;
                object exp = null;
                try { exp = M(d, "Experience"); } catch { }
                if (exp == null) continue;
                _levelQueue.Add(exp);
            }

            _levelTotal = _levelQueue.Count;
            _nextLevelTime = 0f;

            if (_levelTotal == 0)
            {
                Logger.LogWarning("Max seviye: hicbir sakinin deneyim nesnesi bulunamadi.");
                return;
            }

            Logger.LogInfo("Max seviye: " + _levelTotal + " sakin siraya alindi.");
        }

        /// <summary>
        /// Siradaki sakini kare basina en fazla bir kez olmak uzere max seviyeye cikarir.
        /// Oyun duraklatilmis olsa da calisir (unscaledTime).
        /// </summary>
        private void ProcessLevelQueue()
        {
            if (_levelQueue.Count == 0) return;
            if (Time.unscaledTime < _nextLevelTime) return;
            _nextLevelTime = Time.unscaledTime + 0.05f;

            int last = _levelQueue.Count - 1;
            var exp = _levelQueue[last];
            _levelQueue.RemoveAt(last);

            try
            {
                if (exp != null)
                {
                    int max = 50;
                    try
                    {
                        var raw = M(exp, "m_maxLevel");
                        if (raw != null) max = Convert.ToInt32(raw);
                    }
                    catch { }
                    if (max <= 0) max = 50;

                    Call(exp, "SetLevelAndMinExp", max);
                    _levelDone++;
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning("Max seviye hatasi: " + Inner(e).Message);
            }

            if (_levelQueue.Count == 0)
            {
                Logger.LogInfo("Max seviye tamamlandi: " + _levelDone + "/" + _levelTotal + " sakin islendi.");
                _levelTotal = 0;
                _levelDone = 0;
            }
        }

        // ------------------------------------------------------------ quest detect
        private static object QuestScene { get { return Singleton("QuestScene"); } }
        private static object QuestDataManager { get { return Singleton("QuestDataManager"); } }

        private static bool TryGetActiveQuest(out object qs, out object qdm, out string qid)
        {
            qs = null;
            qdm = null;
            qid = null;
            try
            {
                qs = QuestScene;
                if (qs == null) return false;
                qdm = QuestDataManager;
                if (qdm == null) return false;
                qid = M(qdm, "CurrentQuestID") as string;
                if (string.IsNullOrEmpty(qid)) return false;
                if (Truthy(M(qdm, "IsQuestDone"))) return false;
                return true;
            }
            catch { return false; }
        }

        // ------------------------------------------------------------ vault tick
        private void TickVault()
        {
            TryRegisterCompletion();

            bool needBoxCheck = _pendingQuestLunchboxes > 0 || _pendingQuestPetCarriers > 0 || _pendingQuestMrHandy > 0;
            bool needInstantReturn = CfgInstantReturn != null && CfgInstantReturn.Value;

            if ((needBoxCheck || needInstantReturn) && Time.unscaledTime >= _nextVaultTeamScanTime)
            {
                _nextVaultTeamScanTime = Time.unscaledTime + 0.6f;
                try
                {
                    var wasteland = Singleton("Wasteland");
                    var teams = wasteland != null ? M(wasteland, "Teams") as IList : null;
                    if (teams != null)
                    {
                        foreach (var t in teams)
                        {
                            if (t == null) continue;
                            int status = Convert.ToInt32(M(t, "m_eStatus"));

                            if (needBoxCheck && (status == 2 || status == 3) && Truthy(Call(t, "IsQuest")))
                            {
                                var eq = M(t, "TeamEquipment");
                                var br = eq != null ? M(eq, "BasicResources") : null;
                                var res = br != null ? M(br, "Resources") : null;
                                if (res != null)
                                {
                                    EnsureBoxReward(res, "Lunchbox", _pendingQuestLunchboxes);
                                    EnsureBoxReward(res, "PetCarrier", _pendingQuestPetCarriers);
                                    EnsureBoxReward(res, "MrHandy", _pendingQuestMrHandy);
                                    _pendingQuestLunchboxes = 0;
                                    _pendingQuestPetCarriers = 0;
                                    _pendingQuestMrHandy = 0;
                                    needBoxCheck = false;
                                }
                            }

                            if (needInstantReturn && status == 2) // ReturningToVault
                            {
                                bool isQuest = Truthy(Call(t, "IsQuest"));
                                if (isQuest)
                                {
                                    Call(t, "SkipReturningTime");
                                    Logger.LogInfo("Gorev donus yolculugu tamamlandi, sakinler siginak kapisina ulasti.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogV("Vault team tarama hatasi: " + Inner(ex).Message);
                }
            }

            if (!CfgRefill.Value) return;
            if (Time.time < _nextRefillTime) return;
            _nextRefillTime = Time.time + 2f;

            try
            {
                var vault = Singleton("Vault");
                if (vault == null) return;
                var storage = M(vault, "Storage");
                if (storage == null) return;
                var res = M(storage, "Resources");
                if (res == null) return;

                SetResourceIfLow(res, "StimPack", CfgStimpak.Value);
                SetResourceIfLow(res, "RadAway", CfgRadaway.Value);
            }
            catch (Exception e)
            {
                LogV("Kasa stok hatasi: " + e.GetType().Name + " " + e.Message);
            }
        }

        // ------------------------------------------------------------ completed quest registration
        /// <summary>
        /// Gorev basariyla bitince oyunun normalde bir sonraki yuklemede yaptigi
        /// "tamamlanan gorevler" kaydini hemen isler. Boylece gunluk/haftalik
        /// gorev secici ilerler ve ayni gorev tekrar sunulmaz.
        /// </summary>
        private void TryRegisterCompletion()
        {
            if (_pendingCompletionQuestId == null) return;
            if (Time.time < _nextCompletionTime) return;
            _nextCompletionTime = Time.time + 2f;

            string id = _pendingCompletionQuestId;
            try
            {
                // gorev sahnesi hala kapanmadiysa bekle
                if (QuestScene != null) return;
                if (Singleton("Vault") == null) return;

                var cqdm = Singleton("CompletedQuestDataManager");
                if (cqdm == null) return;

                if (Truthy(Call(cqdm, "IsQuestCompleted", id, false)))
                {
                    Logger.LogInfo("Gorev kaydi zaten mevcut: " + id);
                    _pendingCompletionQuestId = null;
                    return;
                }

                var gp = Singleton("GameParameters");
                if (gp == null) return;
                var qd = M(gp, "QuestsData");
                if (qd == null) return;

                var quest = Call(qd, "GetQuest", id, false);
                if (quest == null)
                {
                    Logger.LogWarning("Gorev kaydi icin tanim bulunamadi: " + id);
                    _pendingCompletionQuestId = null;
                    return;
                }

                Call(cqdm, "SetQuestAsCompleted", quest);
                _nextCompletionTime = Time.time + 0.5f;

                bool ok = Truthy(Call(cqdm, "IsQuestCompleted", id, false));
                Logger.LogInfo("Gorev kaydi islendi: " + id + " (dogrulama=" + (ok ? "basarili" : "basarisiz") + ")");
                _pendingCompletionQuestId = null;
            }
            catch (Exception e)
            {
                _completionAttempts++;
                Logger.LogWarning("Gorev kaydi hatasi (" + _completionAttempts + "): " + Inner(e).Message);
                if (_completionAttempts >= 3)
                {
                    Logger.LogWarning("Gorev kaydi 3 denemede basarisiz, birakildi: " + id);
                    _pendingCompletionQuestId = null;
                }
            }
        }

        private void EnsureBoxReward(object gameResources, string prop, int expected)
        {
            if (expected <= 0) return;
            try
            {
                float cur = Convert.ToSingle(M(gameResources, prop));
                if (cur < expected)
                {
                    if (SetMember(gameResources, prop, (float)expected))
                    {
                        Logger.LogInfo("Gorev kutu odulu kaybi onlendi (" + prop + "): " + cur.ToString("0") + " -> " + expected);
                    }
                }
            }
            catch (Exception e)
            {
                LogV("Kutu odulu geri yuklenemedi (" + prop + "): " + e.Message);
            }
        }

        private void SetResourceIfLow(object gameResources, string prop, int wanted)
        {
            try
            {
                float cur = Convert.ToSingle(M(gameResources, prop));
                if (cur >= wanted - 0.5f) return;
                if (SetMember(gameResources, prop, (float)wanted))
                {
                    Logger.LogInfo("Kasa " + prop + ": " + cur.ToString("0") + " -> " + wanted +
                                   " (gorev ekibi icin hazir)");
                }
            }
            catch (Exception e)
            {
                LogV(prop + " ayarlanamadi: " + e.Message);
            }
        }

        // ------------------------------------------------------------ quest tick
        private void TickQuest(object qs, object qdm, string qid)
        {
            if (qdm == null || qs == null) return;

            if (qid != _questId)
            {
                _questId = qid;
                _questSeenAt = Time.unscaledTime;
                _resultShown = false;
                _exitRequested = false;
                _roomClearedAt.Clear();
                _cachedQuestRooms = null;
                _cachedQuestRoomsId = null;
                _nextActionTime = 0f;
                _nextSweepTime = 0f;
                _nextRoomCacheRefreshTime = 0f;
                Logger.LogInfo("Gorev algilandi: " + qid);
            }

            // gorev sahnesi tam yuklenene ve giris akisi tamamlanana kadar bekle (VATS hizindan etkilenmesin diye unscaledTime)
            float elapsedReal = Time.unscaledTime - _questSeenAt;
            if (elapsedReal < 1.5f) return;
            if (IsQuickLoading()) return;

            bool entranceDone = Truthy(M(qdm, "EntranceFlowCompleted"));
            if (!entranceDone)
            {
                if (elapsedReal < 6.0f) return;
                // Giris animasyonu 6 saniyede bitmezse guvenli sekilde tamamlandi isaretle
                try { SetMember(qdm, "EntranceFlowCompleted", true); } catch { }
            }

            bool succeeded = Truthy(M(qdm, "IsQuestSucceeded"));

            if (succeeded)
            {
                if (!_resultShown)
                {
                    SweepAllQuestRoomsLoot(true);
                }
                HandleSuccess(qdm, qs);
                return;
            }

            bool needHeal = CfgAutoHeal.Value && Time.time >= _nextHealTime;
            bool needRefill = CfgRefill.Value && Time.time >= _nextRefillTime;
            if (needHeal || needRefill)
            {
                var team = M(qdm, "CurrentWastelandTeam");
                if (team != null)
                {
                    if (needHeal)
                    {
                        _nextHealTime = Time.time + 0.5f;
                        HealTeam(team);
                    }

                    if (needRefill)
                    {
                        _nextRefillTime = Time.time + 2f;
                        TopUpConsumables(team);
                    }
                }
            }

            if (CfgAutoQuest.Value)
            {
                if (Time.time >= _nextActionTime)
                {
                    var qom = Singleton("QuestObjectiveMgr");
                    if (qom != null) ProgressObjectives(qom);
                }
                if (Time.unscaledTime >= _nextSweepTime)
                {
                    _nextSweepTime = Time.unscaledTime + 0.6f;
                    SweepAllQuestRoomsLoot(false);
                }
            }
        }

        private bool IsQuickLoading()
        {
            var qlw = Singleton("QuickLoadingWindow");
            if (qlw == null) return false;
            try { return Truthy(Call(qlw, "IsPlaying")); }
            catch { return false; }
        }

        // ------------------------------------------------------------ objectives
        private void ProgressObjectives(object qom)
        {
            if (Time.time < _nextActionTime) return;

            var rooms = M(qom, "ObjectiveRooms") as IList;
            if (rooms == null || rooms.Count == 0) return;

            foreach (var room in rooms)
            {
                if (room == null) continue;

                int type;
                try { type = Convert.ToInt32(Call(room, "GetObjectiveType")); }
                catch { continue; }

                if (type != 1 && type != 2) continue; // 1 = Loot, 2 = ClearOut

                bool cleared;
                try { cleared = Truthy(M(room, "IsRoomClearOutSucceeded")); }
                catch { continue; }

                if (!cleared)
                {
                    if (CfgAutoKillEnemies != null && CfgAutoKillEnemies.Value) { KillRoomEnemies(room); }
                    try
                    {
                        Call(room, "SetAndNotifyRoomClearedOut", EnumVal("EQuestRoomResult", "Succeeded", 1));
                        _roomClearedAt[room.GetHashCode()] = Time.time;
                        _nextActionTime = Time.time + CfgInterval.Value;
                        Logger.LogInfo("Hedef oda temizlendi: " + RoomName(room) + " (tip=" + TypeName(type) + ")");
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning("Oda temizleme hatasi: " + Inner(e).Message);
                        _nextActionTime = Time.time + 1.5f;
                    }
                    return;
                }

                if (type == 1) // Loot / PickUp
                {
                    // loot nesnelerinin olusmasi icin kisa bekleme
                    float clearedAt;
                    if (_roomClearedAt.TryGetValue(room.GetHashCode(), out clearedAt) &&
                        Time.time - clearedAt < 1.2f)
                    {
                        _nextActionTime = Time.time + CfgInterval.Value;
                        return;
                    }

                    bool hasLoot;
                    try { hasLoot = Truthy(Call(room, "HasLoot")); }
                    catch { hasLoot = false; }

                    if (hasLoot)
                    {
                        try
                        {
                            CollectAllRoomLoot(room);
                            _nextActionTime = Time.time + CfgInterval.Value;
                            LogV("Loot toplandi: " + RoomName(room));
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning("Loot toplama hatasi: " + Inner(e).Message);
                            _nextActionTime = Time.time + 1.5f;
                        }
                        return;
                    }

                    if (!Truthy(M(room, "IsRoomPickUpSucceeded")))
                    {
                        try
                        {
                            Call(room, "SetAndNotifyRoomPickedUp", EnumVal("EQuestRoomResult", "Succeeded", 1));
                            if (!Truthy(M(room, "IsRoomPickUpSucceeded")))
                            {
                                SetMember(room, "RoomPickUpResult", EnumVal("EQuestRoomResult", "Succeeded", 1));
                                Call(room, "NotifyRoomPickedUp");
                            }
                            _nextActionTime = Time.time + CfgInterval.Value;
                            Logger.LogInfo("Hedef oda toplandi: " + RoomName(room));
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning("Oda toplama hatasi: " + Inner(e).Message);
                            _nextActionTime = Time.time + 1.5f;
                        }
                        return;
                    }
                }
            }
        }

        private void CollectAllRoomLoot(object room)
        {
            if (room == null) return;

            // 1. Düşman cesetlerinin loot ikonlarını aç
            try { Call(room, "ShowCorpsesLoot", true); } catch { }

            // 2. Odadaki TÜM konteynerleri topla (kasa, masa, dolap vb.)
            try
            {
                var lootList = M(room, "m_lootObjects") as IList;
                if (lootList != null)
                {
                    while (lootList.Count > 0)
                    {
                        var lootObj = lootList[0];
                        if (lootObj == null) { lootList.RemoveAt(0); continue; }
                        var containedLoot = M(lootObj, "ContainedLoot");
                        Call(room, "OnLootCollectedFromContainer", containedLoot, lootObj);
                        if (lootList.Count > 0 && lootList[0] == lootObj)
                        {
                            lootList.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogV("Konteyner toplama hatasi: " + Inner(ex).Message);
            }

            // 3. Odadaki TÜM ölü düşman cesetlerini topla
            try
            {
                var fighters = M(room, "m_fighterInstances") as IList;
                if (fighters != null)
                {
                    foreach (var f in fighters)
                    {
                        if (f == null) continue;
                        bool alive = false;
                        try { alive = Truthy(Call(f, "IsAlive")); } catch { }
                        if (!alive)
                        {
                            try
                            {
                                SetMember(f, "m_canCollectLoot", true);
                                Call(f, "OnCollect");
                            }
                            catch
                            {
                                try
                                {
                                    Call(f, "CollectLoot");
                                    Call(f, "ReleaseInstance");
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogV("Ceset loot toplama hatasi: " + Inner(ex).Message);
            }

            // 4. Oda tamamlama ödüllerini doğrudan ve eksiksiz topla (coroutine gecikmesi olmadan)
            try
            {
                var qdm = Singleton("QuestDataManager");
                var uo = room as UnityEngine.Component;
                var tr = uo != null ? uo.transform : null;

                var mainLoot = M(room, "m_roomCompletionLoot");
                if (mainLoot != null && qdm != null)
                {
                    Call(qdm, "CollectLoot", mainLoot, tr);
                    SetMember(room, "m_roomCompletionLoot", null);
                }

                var extraLoot = M(room, "m_extraRoomCompletionLoot") as IList;
                if (extraLoot != null && qdm != null)
                {
                    foreach (var l in extraLoot)
                    {
                        if (l != null)
                        {
                            Call(qdm, "CollectLoot", l, tr);
                        }
                    }
                    SetMember(room, "m_extraRoomCompletionLoot", null);
                }
            }
            catch (Exception ex)
            {
                LogV("Oda tamamlama odulu hatasi: " + Inner(ex).Message);
            }
        }

        private float _nextWindowCollectTime;

        private void ProcessRewardWindows()
        {
            if (!IsEnabled || CfgAutoCollectRewards == null || !CfgAutoCollectRewards.Value) return;
            if (Time.unscaledTime < _nextWindowCollectTime) return;
            _nextWindowCollectTime = Time.unscaledTime + 0.35f;

            // Gorev sahnesindeyken VaultGUI odul pencerelerini arama (FindObjectOfType spam onlenir)
            if (QuestScene != null) return;

            try
            {
                var vgm = Singleton("VaultGUIManager");
                if (vgm == null && Singleton("Vault") == null) return;

                // 1. Theme Recipe Loot Window (Tarif Parcalari, ornegin Minutemen Living Quarters)
                object themeWin = vgm != null ? M(vgm, "m_themeRecipeLootWindow") : null;
                if (themeWin == null)
                {
                    if (_cachedThemeWin == null)
                    {
                        var tType = FindType("ThemeRecipeLootWindow");
                        if (tType != null) _cachedThemeWin = UnityEngine.Object.FindObjectOfType(tType);
                    }
                    themeWin = _cachedThemeWin;
                }

                if (themeWin != null)
                {
                    bool isThemeActive = false;
                    var winObj = M(themeWin, "m_window");
                    if (winObj != null)
                    {
                        try { isThemeActive = Truthy(Call(winObj, "IsActive")); } catch { }
                    }
                    if (!isThemeActive)
                    {
                        var comp = themeWin as UnityEngine.Component;
                        if (comp != null && comp.gameObject.activeInHierarchy) isThemeActive = true;
                    }

                    if (isThemeActive)
                    {
                        bool allRevealed = false;
                        try { allRevealed = Truthy(Call(themeWin, "HasAllPartsRevealed")); } catch { }

                        if (!allRevealed)
                        {
                            try { Call(themeWin, "Collect"); } catch { }
                        }
                        else
                        {
                            try { Call(themeWin, "Close"); } catch { }
                        }
                    }
                }

                // 2. Normal Recipe Loot Window (Silah / Kiyafet Tarif Parcalari)
                object recipeWin = vgm != null ? M(vgm, "m_recipeLootWindow") : null;
                if (recipeWin == null)
                {
                    if (_cachedRecipeWin == null)
                    {
                        var rType = FindType("RecipeLootWindow");
                        if (rType != null) _cachedRecipeWin = UnityEngine.Object.FindObjectOfType(rType);
                    }
                    recipeWin = _cachedRecipeWin;
                }

                if (recipeWin != null)
                {
                    bool isRecipeActive = false;
                    var rWinObj = M(recipeWin, "m_window");
                    if (rWinObj != null)
                    {
                        try { isRecipeActive = Truthy(Call(rWinObj, "IsActive")); } catch { }
                    }
                    if (!isRecipeActive)
                    {
                        var comp = recipeWin as UnityEngine.Component;
                        if (comp != null && comp.gameObject.activeInHierarchy) isRecipeActive = true;
                    }

                    if (isRecipeActive)
                    {
                        try { Call(recipeWin, "InputClose"); } catch { }
                        try { Call(recipeWin, "AttemptClose"); } catch { }
                    }
                }

                // 3. Arrival Window (Gorevden / Kesiften donen ekibin karsilama ekrani)
                // Not: LunchBoxCollectedWindow (Collection / Shop / Redeem ekrani) kesinlikle otomatik kapatilmaz,
                // aksi halde oyuncu lunchbox/paket acma ekranini kullanamaz.
                object arrWin = vgm != null ? M(vgm, "m_arrivalWindow") : null;
                if (arrWin == null)
                {
                    if (_cachedArrivalWin == null)
                    {
                        var aType = FindType("ArrivalWindow");
                        if (aType != null) _cachedArrivalWin = UnityEngine.Object.FindObjectOfType(aType);
                    }
                    arrWin = _cachedArrivalWin;
                }

                if (arrWin != null)
                {
                    bool isArrOpen = false;
                    try { isArrOpen = Truthy(M(arrWin, "IsOpen")); } catch { }
                    if (isArrOpen)
                    {
                        try
                        {
                            Call(arrWin, "OnClaimPressed");
                            Call(arrWin, "Close", true);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogV("ProcessRewardWindows hatasi: " + Inner(ex).Message);
            }
        }

        private void KillRoomEnemies(object room)
        {
            if (room == null) return;
            try
            {
                var convList = M(room, "m_conversationNPCs") as IList;
                var fighters = M(room, "m_fighterInstances") as IList;
                if (fighters == null || fighters.Count == 0) return;

                for (int i = 0; i < fighters.Count; i++)
                {
                    var f = fighters[i];
                    if (f == null) continue;

                    // 1. ASLA bir Dweller'a (oyuncu sakinine veya kurtarilacak sakine) dokunma
                    // check type below
                    string typeFullName = f.GetType().FullName;
                    if (typeFullName.Contains("Dweller")) continue;

                    // 2. Diyalog ve dost NPC listesindeyse (m_conversationNPCs) ASLA dokunma
                    if (convList != null && convList.Contains(f)) continue;

                    // 3. Canli dusman kontrolu (IsAlive == true)
                    bool alive = false;
                    try { alive = Truthy(Call(f, "IsAlive")); } catch { }
                    if (!alive) continue;

                    // 4. Dusman turu kontrolu (EEnemyType != None veya Fighter turevi)
                    int enemyType = 0;
                    try { enemyType = Convert.ToInt32(Call(f, "get_EnemyType")); } catch { }
                    string typeName = f.GetType().Name;

                    if (enemyType > 0 || typeName.EndsWith("Fighter") || typeName.Contains("Creature"))
                    {
                        try
                        {
                            Call(f, "Kill");
                        }
                        catch
                        {
                            try
                            {
                                var hp = M(f, "CurrentHealth");
                                if (hp != null) SetMember(hp, "m_healthValue", 0f);
                                Call(f, "Kill");
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogV("Dusman temizleme hatasi: " + Inner(ex).Message);
            }
        }

        private UnityEngine.Object[] GetQuestRooms(bool forceRefresh)
        {
            if (!forceRefresh &&
                _cachedQuestRooms != null &&
                _cachedQuestRooms.Length > 0 &&
                _cachedQuestRoomsId == _questId &&
                Time.unscaledTime < _nextRoomCacheRefreshTime)
            {
                return _cachedQuestRooms;
            }

            var roomType = FindType("QuestRoom");
            if (roomType == null) return null;

            _cachedQuestRooms = UnityEngine.Object.FindObjectsOfType(roomType);
            _cachedQuestRoomsId = _questId;
            _nextRoomCacheRefreshTime = Time.unscaledTime + 5.0f;
            return _cachedQuestRooms;
        }

        private void SweepAllQuestRoomsLoot(bool forceCompleteAll)
        {
            try
            {
                var allRooms = GetQuestRooms(forceCompleteAll);
                if (allRooms == null || allRooms.Length == 0) return;

                foreach (var obj in allRooms)
                {
                    if (obj == null) continue;
                    if (CfgAutoKillEnemies != null && CfgAutoKillEnemies.Value) { KillRoomEnemies(obj); }

                    // Odayı temizle (düşmanlar öldürüldü sayılır)
                    if (forceCompleteAll && !Truthy(M(obj, "IsRoomClearOutSucceeded")))
                    {
                        try { Call(obj, "SetAndNotifyRoomClearedOut", EnumVal("EQuestRoomResult", "Succeeded", 1)); }
                        catch { }
                    }

                    // Odadaki tüm lootları (konteynerler + cesetler + tamamlama) topla
                    CollectAllRoomLoot(obj);

                    // Odayı toplandı olarak işaretle
                    if (forceCompleteAll && !Truthy(M(obj, "IsRoomPickUpSucceeded")))
                    {
                        try { Call(obj, "SetAndNotifyRoomPickedUp", EnumVal("EQuestRoomResult", "Succeeded", 1)); }
                        catch { }
                    }
                }
            }
            catch (Exception e)
            {
                LogV("Genel oda supurme hatasi: " + Inner(e).Message);
            }
        }

        private static string TypeName(int t)
        {
            if (t == 1) return "Loot";
            if (t == 2) return "ClearOut";
            return t.ToString();
        }

        private static string RoomName(object room)
        {
            try
            {
                var uo = room as UnityEngine.Object;
                return uo != null ? uo.name : "QuestRoom";
            }
            catch { return "QuestRoom"; }
        }

        // ------------------------------------------------------------ heal / refill
        private void HealTeam(object team)
        {
            IList dwellers;
            try { dwellers = M(team, "Dwellers") as IList; }
            catch { return; }
            if (dwellers == null) return;

            foreach (var d in dwellers)
            {
                if (d == null) continue;

                try
                {
                    // 1. Ölen sakin varsa anında canlandır
                    if (Truthy(M(d, "IsDead")))
                    {
                        try
                        {
                            Call(d, "ReviveInQuest");
                            Logger.LogInfo("Sakin aninda canlandirildi: " + RoomName(d));
                        }
                        catch { }
                    }

                    // 2. Canı ve radyasyonu maksimuma sabitle (Ölümsüzlük)
                    var dh = M(d, "Health");
                    if (dh != null)
                    {
                        try
                        {
                            float maxHp = Convert.ToSingle(M(dh, "HealthMax"));
                            if (maxHp > 0f)
                            {
                                SetMember(dh, "HealthValue", maxHp);
                            }
                            SetMember(dh, "RadiationValue", 0f);
                        }
                        catch { }

                        if (Truthy(M(dh, "NeedStimpack")))
                        {
                            try { Call(dh, "UseStimpack"); } catch { }
                        }
                        if (Truthy(M(dh, "NeedRadaway")))
                        {
                            try { Call(dh, "UseRadAway"); } catch { }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogV("Iyilestirme hatasi: " + Inner(e).Message);
                }
            }
        }

        private void TopUpConsumables(object team)
        {
            try
            {
                var inv = M(team, "LootInventory");
                if (inv == null) return;

                TopUpOne(inv, "Stimpak", CfgStimpak.Value);
                TopUpOne(inv, "Radaway", CfgRadaway.Value);
            }
            catch (Exception e)
            {
                LogV("Sarf tamamlama hatasi: " + e.GetType().Name + " " + e.Message);
            }
        }

        private void TopUpOne(object inventory, string lootTypeName, int wanted)
        {
            var lootType = EnumVal("EQuestLootType", lootTypeName, -1);
            if (lootType == null) return;

            int cur;
            try { cur = Convert.ToInt32(Call(inventory, "GetLootCount", lootType)); }
            catch { return; }

            int need = wanted - cur;
            if (need <= 0) return;

            try
            {
                var loot = Call(inventory, "CreateLoot", lootType, (float)need, 0f);
                if (loot == null) return;
                Call(inventory, "AddLoot", loot);
                Logger.LogInfo("Gorev sarfi " + lootTypeName + ": " + cur + " -> " + wanted);
            }
            catch (Exception e)
            {
                LogV("Sarf eklenemedi (" + lootTypeName + "): " + Inner(e).Message);
            }
        }

        // ------------------------------------------------------------ success / exit
        private void HandleSuccess(object qdm, object qs)
        {
            if (_resultShown)
            {
                if (!CfgAutoReturn.Value) return;
                if (!_exitRequested && Time.unscaledTime - _resultShownAt <= 1.2f) return;
                if (_exitRequested && Time.time - _exitRequestedAt <= 12f) return;
            }

            object rw = null;
            try
            {
                var qgm = Singleton("QuestGUIManager");
                if (qgm != null) rw = M(qgm, "m_questResultWindow");
            }
            catch { }

            if (!_resultShown)
            {
                _resultShown = true;
                _resultShownAt = Time.unscaledTime;

                // Kasaya donerken RETRY QUEST / 0 odul hatasi olmamasi icin giris ve basari bayraklarini garantiye al
                try
                {
                    if (qdm != null)
                    {
                        if (!Truthy(M(qdm, "EntranceFlowCompleted")))
                            SetMember(qdm, "EntranceFlowCompleted", true);
                        if (!Truthy(M(qdm, "IsQuestSucceeded")))
                            Call(qdm, "SetQuestSucceeded");
                    }
                }
                catch { }

                // gorev kaydini kasaya donunce isle (tek sefer)
                _pendingCompletionQuestId = _questId;
                _completionAttempts = 0;
                _nextCompletionTime = 0f;

                // Gorevde kazanilan Lunchbox / PetCarrier / MrHandy kutularini guvenceye al
                _pendingQuestLunchboxes = 0;
                _pendingQuestPetCarriers = 0;
                _pendingQuestMrHandy = 0;
                try
                {
                    var qTeam = qdm != null ? M(qdm, "CurrentWastelandTeam") : null;
                    var qInv = qTeam != null ? M(qTeam, "LootInventory") : null;
                    if (qInv != null)
                    {
                        var tLb = EnumVal("EQuestLootType", "Lunchbox", 12);
                        var tPc = EnumVal("EQuestLootType", "PetCarrier", 13);
                        var tMh = EnumVal("EQuestLootType", "MrHandyBox", 14);
                        if (tLb != null) _pendingQuestLunchboxes = Convert.ToInt32(Call(qInv, "GetLootCount", tLb));
                        if (tPc != null) _pendingQuestPetCarriers = Convert.ToInt32(Call(qInv, "GetLootCount", tPc));
                        if (tMh != null) _pendingQuestMrHandy = Convert.ToInt32(Call(qInv, "GetLootCount", tMh));
                        if (_pendingQuestLunchboxes > 0 || _pendingQuestPetCarriers > 0 || _pendingQuestMrHandy > 0)
                        {
                            Logger.LogInfo("Gorev kutu odulleri kaydedildi: Lunchbox=" + _pendingQuestLunchboxes +
                                           ", PetCarrier=" + _pendingQuestPetCarriers +
                                           ", MrHandy=" + _pendingQuestMrHandy);
                        }
                    }
                }
                catch { }

                bool showing = false;
                try { showing = Truthy(M(rw, "m_isShowing")); } catch { }

                if (rw != null && !showing)
                {
                    try
                    {
                        // ShowNow -> ShowQuestSucceeded -> SetInventoryAsQuestSucceeded (oduller)
                        Call(rw, "ShowNow", true);
                        Logger.LogInfo("Gorev BASARILI. Sonuc penceresi acildi, oduller uygulandi.");
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning("Sonuc penceresi acilamadi: " + Inner(e).Message);
                    }
                }
                else
                {
                    Logger.LogInfo("Gorev BASARILI. Sonuc penceresi zaten acik.");
                }
                return;
            }

            if (!CfgAutoReturn.Value) return;

            if (!_exitRequested && Time.unscaledTime - _resultShownAt > 1.2f)
            {
                _exitRequested = true;
                _exitRequestedAt = Time.time;
                try
                {
                    if (rw != null)
                    {
                        Call(rw, "ReturnToVault");
                        Logger.LogInfo("Kasaya donus istendi (sonuc penceresi).");
                    }
                    else if (qs != null)
                    {
                        Call(qs, "ReturnToVault", false);
                        Logger.LogInfo("Kasaya donus istendi (gorev sahnesi).");
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning("Kasaya donus hatasi: " + Inner(e).Message);
                }
                return;
            }

            // emniyet: 12 saniye icinde donulmediyse zorla bitir
            if (_exitRequested && Time.time - _exitRequestedAt > 12f && QuestScene != null)
            {
                _exitRequestedAt = Time.time;
                try
                {
                    Call(qdm, "CompleteOrForfeitQuest", false, false);
                    Logger.LogWarning("Kasa donusu gecikti, gorev zorla kapatildi.");
                }
                catch (Exception e)
                {
                    LogV("Zorla kapatma hatasi: " + Inner(e).Message);
                }
            }
        }

        // ------------------------------------------------------------ logging
        private bool TraceEnabled()
        {
            float now = Time.unscaledTime;
            if (now >= _nextTraceCheckTime)
            {
                _nextTraceCheckTime = now + 10f;
                try { _traceEnabledCached = !string.IsNullOrEmpty(_traceFlagPath) && File.Exists(_traceFlagPath); }
                catch { _traceEnabledCached = false; }
            }
            return _traceEnabledCached;
        }

        private string TraceLine()
        {
            var sb = new StringBuilder();
            sb.Append("[AutoQuest] trace");
            try
            {
                var qdm = QuestDataManager;
                var qsc = QuestScene;
                sb.Append(" questScene=").Append(qsc == null ? "YOK" : "VAR");
                if (qdm != null)
                {
                    sb.Append(" qid=").Append(M(qdm, "CurrentQuestID"));
                    sb.Append(" done=").Append(M(qdm, "IsQuestDone"));
                    sb.Append(" ok=").Append(M(qdm, "IsQuestSucceeded"));
                }
                var qom = Singleton("QuestObjectiveMgr");
                if (qom != null)
                {
                    var rooms = M(qom, "ObjectiveRooms") as IList;
                    int total = rooms == null ? 0 : rooms.Count;
                    int done = 0;
                    if (rooms != null)
                    {
                        foreach (var r in rooms)
                        {
                            if (r == null) continue;
                            if (Truthy(M(r, "IsRoomClearOutSucceeded")) &&
                                (Convert.ToInt32(Call(r, "GetObjectiveType")) != 1 ||
                                 Truthy(M(r, "IsRoomPickUpSucceeded")))) done++;
                        }
                    }
                    sb.Append(" obj=").Append(done).Append("/").Append(total);
                    try
                    {
                        sb.Append(" prim=").Append(Call(qom, "IsPrimaryObjectiveCompleted"));
                        sb.Append(" sec=").Append(Call(qom, "IsSecondaryObjectiveCompleted"));
                    }
                    catch { }
                }
            }
            catch (Exception e) { sb.Append(" <hata:").Append(e.GetType().Name).Append(">"); }
            return sb.ToString();
        }

        internal static void LogV(string msg)
        {
            try { if (CfgVerbose != null && CfgVerbose.Value) Log.LogInfo(msg); } catch { }
        }

        // ------------------------------------------------------------ reflection helpers
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags SFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        private static readonly object[] EmptyArgs = new object[0];
        private static readonly string[] SingletonFieldNames = new[] { "m_Instance", "m_instance", "_instance", "instance", "s_instance" };
        private static readonly CachedMethod[] EmptyCachedMethods = new CachedMethod[0];
        private static readonly FieldInfo[] EmptyFieldInfos = new FieldInfo[0];

        private struct MemberKey : IEquatable<MemberKey>
        {
            public readonly Type Type;
            public readonly string Name;

            public MemberKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public bool Equals(MemberKey other)
            {
                return Type == other.Type && string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is MemberKey && Equals((MemberKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Name != null ? StringComparer.Ordinal.GetHashCode(Name) : 0);
                }
            }
        }

        private struct MethodKey : IEquatable<MethodKey>
        {
            public readonly Type Type;
            public readonly string Name;
            public readonly int ArgCount;

            public MethodKey(Type type, string name, int argCount)
            {
                Type = type;
                Name = name;
                ArgCount = argCount;
            }

            public bool Equals(MethodKey other)
            {
                return ArgCount == other.ArgCount && Type == other.Type && string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is MethodKey && Equals((MethodKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = Type != null ? Type.GetHashCode() : 0;
                    h = (h * 397) ^ (Name != null ? StringComparer.Ordinal.GetHashCode(Name) : 0);
                    h = (h * 397) ^ ArgCount;
                    return h;
                }
            }
        }

        private struct StringPairKey : IEquatable<StringPairKey>
        {
            public readonly string A;
            public readonly string B;

            public StringPairKey(string a, string b)
            {
                A = a;
                B = b;
            }

            public bool Equals(StringPairKey other)
            {
                return string.Equals(A, other.A, StringComparison.Ordinal) &&
                       string.Equals(B, other.B, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is StringPairKey && Equals((StringPairKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = A != null ? StringComparer.Ordinal.GetHashCode(A) : 0;
                    return (h * 397) ^ (B != null ? StringComparer.Ordinal.GetHashCode(B) : 0);
                }
            }
        }

        private struct ReadMember
        {
            public PropertyInfo Prop;
            public FieldInfo Field;
        }

        private struct WriteMember
        {
            public PropertyInfo Prop;
            public FieldInfo Field;
            public Type ValueType;
        }

        private struct CachedMethod
        {
            public MethodInfo Method;
            public ParameterInfo[] Params;
        }

        private sealed class SingletonAccessor
        {
            public PropertyInfo ValidProp;
            public FieldInfo[] InstanceFields;
            public PropertyInfo InstanceProp;
        }

        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<StringPairKey, object> EnumCache = new Dictionary<StringPairKey, object>();
        private static readonly Dictionary<MemberKey, ReadMember> ReadMemberCache = new Dictionary<MemberKey, ReadMember>();
        private static readonly Dictionary<MemberKey, WriteMember> WriteMemberCache = new Dictionary<MemberKey, WriteMember>();
        private static readonly Dictionary<MethodKey, CachedMethod[]> MethodCache = new Dictionary<MethodKey, CachedMethod[]>();
        private static readonly Dictionary<Type, SingletonAccessor> SingletonCache = new Dictionary<Type, SingletonAccessor>();

        private static Exception Inner(Exception e)
        {
            if (e is TargetInvocationException && e.InnerException != null) return e.InnerException;
            return e;
        }

        private static Type FindType(string name)
        {
            lock (TypeCache)
            {
                Type cached;
                if (TypeCache.TryGetValue(name, out cached)) return cached;
            }

            Type found = null;
            Assembly[] asms;
            try { asms = AppDomain.CurrentDomain.GetAssemblies(); }
            catch { asms = new Assembly[0]; }

            // Once Assembly-CSharp, sonra digerleri
            found = FindInAssemblies(asms, name, true);
            if (found == null) found = FindInAssemblies(asms, name, false);

            lock (TypeCache) { TypeCache[name] = found; }
            return found;
        }

        private static Type FindInAssemblies(Assembly[] asms, string name, bool onlyAssemblyCSharp)
        {
            foreach (var asm in asms)
            {
                if (asm == null) continue;
                string an;
                try { an = asm.GetName().Name; } catch { continue; }
                bool isAcs = an != null && an.StartsWith("Assembly-CSharp");
                if (onlyAssemblyCSharp != isAcs) continue;

                Type[] ts;
                try { ts = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { ts = e.Types; }
                catch { continue; }

                foreach (var t in ts)
                {
                    if (t == null) continue;
                    if (t.Name == name || t.FullName == name) return t;
                }
            }
            return null;
        }

        private static object EnumVal(string typeName, string member, int fallback)
        {
            var key = new StringPairKey(typeName, member);
            object cached;
            if (EnumCache.TryGetValue(key, out cached)) return cached;

            object result = null;
            var t = FindType(typeName);
            if (t != null && t.IsEnum)
            {
                try { result = Enum.Parse(t, member); } catch { }
                if (result == null)
                {
                    try { result = Enum.ToObject(t, fallback); } catch { }
                }
            }

            EnumCache[key] = result;
            return result;
        }

        private static object M(object o, string name)
        {
            if (o == null) return null;
            var type = o.GetType();
            var key = new MemberKey(type, name);
            ReadMember rm;
            if (!ReadMemberCache.TryGetValue(key, out rm))
            {
                rm = default(ReadMember);
                var t = type;
                while (t != null)
                {
                    try
                    {
                        var p = t.GetProperty(name, Flags);
                        if (p != null && p.GetIndexParameters().Length == 0)
                        {
                            rm.Prop = p;
                            break;
                        }
                    }
                    catch { }

                    try
                    {
                        var f = t.GetField(name, Flags);
                        if (f != null)
                        {
                            rm.Field = f;
                            break;
                        }
                    }
                    catch { }

                    t = t.BaseType;
                }
                ReadMemberCache[key] = rm;
            }

            if (rm.Prop != null)
            {
                try { return rm.Prop.GetValue(o, null); }
                catch { return null; }
            }
            if (rm.Field != null)
            {
                try { return rm.Field.GetValue(o); }
                catch { return null; }
            }
            return null;
        }

        private bool SetMember(object o, string name, object value)
        {
            if (o == null) return false;
            var type = o.GetType();
            var key = new MemberKey(type, name);
            WriteMember wm;
            if (!WriteMemberCache.TryGetValue(key, out wm))
            {
                wm = default(WriteMember);
                var t = type;
                while (t != null)
                {
                    try
                    {
                        var p = t.GetProperty(name, Flags);
                        if (p != null && p.GetIndexParameters().Length == 0 && p.CanWrite)
                        {
                            wm.Prop = p;
                            wm.ValueType = p.PropertyType;
                            break;
                        }
                    }
                    catch { }

                    try
                    {
                        var f = t.GetField(name, Flags);
                        if (f != null && !f.IsInitOnly)
                        {
                            wm.Field = f;
                            wm.ValueType = f.FieldType;
                            break;
                        }
                    }
                    catch { }

                    // auto-property backing field
                    try
                    {
                        var bf = t.GetField("<" + name + ">k__BackingField", Flags);
                        if (bf != null)
                        {
                            wm.Field = bf;
                            wm.ValueType = bf.FieldType;
                            break;
                        }
                    }
                    catch { }

                    t = t.BaseType;
                }
                WriteMemberCache[key] = wm;
            }

            if (wm.Prop != null)
            {
                try
                {
                    wm.Prop.SetValue(o, ConvertArg(wm.ValueType, value), null);
                    return true;
                }
                catch { return false; }
            }
            if (wm.Field != null)
            {
                try
                {
                    wm.Field.SetValue(o, ConvertArg(wm.ValueType, value));
                    return true;
                }
                catch { return false; }
            }
            return false;
        }

        private object Call(object target, string name)
        {
            return CallInternal(target, name, EmptyArgs);
        }

        private object Call(object target, string name, params object[] args)
        {
            return CallInternal(target, name, args ?? EmptyArgs);
        }

        private object CallInternal(object target, string name, object[] args)
        {
            if (target == null) return null;
            var type = target.GetType();
            var key = new MethodKey(type, name, args.Length);
            CachedMethod[] candidates;
            if (!MethodCache.TryGetValue(key, out candidates))
            {
                List<CachedMethod> list = null;
                var t = type;
                while (t != null)
                {
                    try
                    {
                        var methods = t.GetMethods(Flags);
                        for (int mi = 0; mi < methods.Length; mi++)
                        {
                            var m = methods[mi];
                            if (m.Name != name) continue;
                            var ps = m.GetParameters();
                            if (ps.Length != args.Length) continue;
                            if (list == null) list = new List<CachedMethod>(2);
                            list.Add(new CachedMethod { Method = m, Params = ps });
                        }
                    }
                    catch { }
                    t = t.BaseType;
                }
                candidates = list != null ? list.ToArray() : EmptyCachedMethods;
                MethodCache[key] = candidates;
            }

            for (int ci = 0; ci < candidates.Length; ci++)
            {
                var cand = candidates[ci];
                var ps = cand.Params;
                bool ok = true;
                bool needsConvert = false;
                for (int i = 0; i < ps.Length; i++)
                {
                    var pt = ps[i].ParameterType;
                    var a = args[i];
                    if (a == null)
                    {
                        if (pt.IsValueType && Nullable.GetUnderlyingType(pt) == null) { ok = false; break; }
                        continue;
                    }
                    if (!pt.IsAssignableFrom(a.GetType())) { ok = false; break; }
                }
                if (!ok) continue;

                object[] invokeArgs;
                if (args.Length == 0)
                {
                    invokeArgs = EmptyArgs;
                }
                else
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        var a = args[i];
                        if (a != null && !ps[i].ParameterType.IsAssignableFrom(a.GetType()))
                        {
                            needsConvert = true;
                            break;
                        }
                    }
                    if (needsConvert)
                    {
                        invokeArgs = new object[args.Length];
                        for (int i = 0; i < args.Length; i++)
                            invokeArgs[i] = ConvertArg(ps[i].ParameterType, args[i]);
                    }
                    else
                    {
                        invokeArgs = args;
                    }
                }

                try
                {
                    return cand.Method.Invoke(target, invokeArgs);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
            }

            string missKey = type.Name + "." + name + "/" + args.Length;
            if (_methodMissLogged.Add(missKey))
                Logger.LogWarning("Metot bulunamadi: " + missKey);
            return null;
        }

        private static object ConvertArg(Type pt, object a)
        {
            if (a == null) return null;
            var at = a.GetType();
            if (pt.IsAssignableFrom(at)) return a;
            if (pt.IsEnum)
            {
                try { return Enum.ToObject(pt, Convert.ToInt32(a)); } catch { return a; }
            }
            try
            {
                if (pt == typeof(float)) return Convert.ToSingle(a);
                if (pt == typeof(double)) return Convert.ToDouble(a);
                if (pt == typeof(int)) return Convert.ToInt32(a);
                if (pt == typeof(long)) return Convert.ToInt64(a);
                if (pt == typeof(bool)) return Convert.ToBoolean(a);
            }
            catch { }
            return a;
        }

        private static bool Truthy(object v)
        {
            if (v == null) return false;
            if (v is bool) return (bool)v;
            try { return Convert.ToBoolean(v); } catch { return false; }
        }

        private static object Singleton(string typeName)
        {
            var t = FindType(typeName);
            if (t == null) return null;

            SingletonAccessor acc;
            if (!SingletonCache.TryGetValue(t, out acc))
            {
                acc = new SingletonAccessor();
                List<FieldInfo> fields = null;
                var bt = t;
                while (bt != null)
                {
                    if (acc.ValidProp == null)
                    {
                        try { acc.ValidProp = bt.GetProperty("IsInstanceValid", SFlags); } catch { }
                    }

                    for (int i = 0; i < SingletonFieldNames.Length; i++)
                    {
                        try
                        {
                            var f = bt.GetField(SingletonFieldNames[i], SFlags);
                            if (f != null)
                            {
                                if (fields == null) fields = new List<FieldInfo>(2);
                                if (!fields.Contains(f)) fields.Add(f);
                            }
                        }
                        catch { }
                    }

                    if (acc.InstanceProp == null)
                    {
                        try { acc.InstanceProp = bt.GetProperty("Instance", SFlags); } catch { }
                    }

                    bt = bt.BaseType;
                }
                acc.InstanceFields = fields != null ? fields.ToArray() : EmptyFieldInfos;
                SingletonCache[t] = acc;
            }

            // 1. Önce IsInstanceValid kontrolü yap (MonoSingleton<T> için).
            // Eğer false ise sahnede obje yoktur; Instance çağırmak Unity'nin
            // "cant find a gameobject of instance..." uyarısını loglamasına yol açar.
            if (acc.ValidProp != null)
            {
                try
                {
                    var val = acc.ValidProp.GetValue(null, null);
                    if (val is bool && !(bool)val)
                    {
                        return null;
                    }
                }
                catch { }
            }

            // 2. Statik alanları doğrudan oku (m_Instance vs.)
            var instFields = acc.InstanceFields;
            for (int i = 0; i < instFields.Length; i++)
            {
                try
                {
                    var v = instFields[i].GetValue(null);
                    if (v != null) return v;
                }
                catch { }
            }

            // 3. IsInstanceValid true ise veya bulunamadıysa Instance property'sini çağır
            if (acc.InstanceProp != null)
            {
                try
                {
                    var v = acc.InstanceProp.GetValue(null, null);
                    if (v != null) return v;
                }
                catch { }
            }

            return null;
        }
    }
}
