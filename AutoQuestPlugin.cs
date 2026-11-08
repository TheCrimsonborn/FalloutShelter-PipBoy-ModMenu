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
        internal static ConfigEntry<bool> CfgRefill;
        internal static ConfigEntry<bool> CfgAutoRevive;
        internal static ConfigEntry<int> CfgStimpak;
        internal static ConfigEntry<int> CfgRadaway;
        internal static ConfigEntry<float> CfgInterval;
        internal static ConfigEntry<bool> CfgVerbose;

        internal static ManualLogSource Log;

        private const string TraceFlagName = "FSAutoQuest-trace.on";

        private float _nextActionTime;
        private float _nextHealTime;
        private float _nextRefillTime;

        private string _questId;
        private float _questSeenAt;
        private readonly Dictionary<int, float> _roomClearedAt = new Dictionary<int, float>();

        private bool _resultShown;
        private float _resultShownAt;
        private bool _exitRequested;
        private float _exitRequestedAt;

        private bool _wasInQuest;
        private string _pendingCompletionQuestId;
        private int _completionAttempts;
        private float _nextCompletionTime;

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
            CfgAutoReturn = Config.Bind("Genel", "KasaDonusu", true,
                "Gorev basarili olunca otomatik olarak kasaya don.");
            CfgAutoRevive = Config.Bind("Genel", "OtomatikCanlandir", false,
                "Gorevde olen sakinleri otomatik canlandir (deneysel).");
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

                if (!CfgEnabled.Value) return;
                if (Time.timeScale <= 0f) return;

                if (Time.frameCount % 30 == 0 && TraceEnabled())
                    Logger.LogInfo(TraceLine());

                if (InQuest())
                {
                    _wasInQuest = true;
                    TickQuest();
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

            var exp = _levelQueue[0];
            _levelQueue.RemoveAt(0);

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

        private static bool InQuest()
        {
            try
            {
                var qs = QuestScene;
                var qdm = QuestDataManager;
                if (qs == null || qdm == null) return false;
                var qid = M(qdm, "CurrentQuestID") as string;
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
        private void TickQuest()
        {
            var qdm = QuestDataManager;
            var qs = QuestScene;
            if (qdm == null || qs == null) return;

            string qid = M(qdm, "CurrentQuestID") as string;
            if (qid != _questId)
            {
                _questId = qid;
                _questSeenAt = Time.time;
                _resultShown = false;
                _exitRequested = false;
                _roomClearedAt.Clear();
                _nextActionTime = 0f;
                Logger.LogInfo("Gorev algilandi: " + qid);
            }

            // gorev sahnesi tam yuklenene kadar bekle
            if (Time.time - _questSeenAt < 1.5f) return;
            if (IsQuickLoading()) return;

            bool succeeded = Truthy(M(qdm, "IsQuestSucceeded"));

            if (succeeded)
            {
                SweepAllQuestRoomsLoot(true);
                HandleSuccess(qdm, qs);
                return;
            }

            var team = M(qdm, "CurrentWastelandTeam");
            if (team != null)
            {
                if (CfgAutoHeal.Value && Time.time >= _nextHealTime)
                {
                    _nextHealTime = Time.time + 0.5f;
                    HealTeam(team);
                }

                if (CfgRefill.Value && Time.time >= _nextRefillTime)
                {
                    _nextRefillTime = Time.time + 2f;
                    TopUpConsumables(team);
                }
            }

            if (CfgAutoQuest.Value)
            {
                var qom = Singleton("QuestObjectiveMgr");
                if (qom != null) ProgressObjectives(qom);
                SweepAllQuestRoomsLoot(false);
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

            // 4. Oda tamamlama ödüllerini topla
            try
            {
                if (Truthy(M(room, "HasCompletionRoomLoot")))
                {
                    var coroutine = Call(room, "CollectCompletionRoomRewards");
                    if (coroutine is IEnumerator)
                    {
                        StartCoroutine((IEnumerator)coroutine);
                    }
                }
            }
            catch (Exception ex)
            {
                LogV("Oda tamamlama odulu hatasi: " + Inner(ex).Message);
            }
        }

        private void SweepAllQuestRoomsLoot(bool forceCompleteAll)
        {
            try
            {
                var roomType = FindType("QuestRoom");
                if (roomType == null) return;

                var allRooms = UnityEngine.Object.FindObjectsOfType(roomType);
                if (allRooms == null || allRooms.Length == 0) return;

                foreach (var obj in allRooms)
                {
                    if (obj == null) continue;

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
                    if (Truthy(M(d, "IsDead")))
                    {
                        if (CfgAutoRevive.Value)
                        {
                            Call(d, "ReviveInQuest");
                            Logger.LogInfo("Sakin canlandirildi: " + RoomName(d));
                        }
                        continue;
                    }

                    var dh = M(d, "Health");
                    if (dh == null) continue;

                    if (Truthy(M(dh, "NeedStimpack")))
                    {
                        Call(dh, "UseStimpack");
                        LogV("Stimpak kullanildi: " + RoomName(d));
                    }
                    if (Truthy(M(dh, "NeedRadaway")))
                    {
                        Call(dh, "UseRadAway");
                        LogV("RadAway kullanildi: " + RoomName(d));
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
                _resultShownAt = Time.time;

                // gorev kaydini kasaya donunce isle (tek sefer)
                _pendingCompletionQuestId = _questId;
                _completionAttempts = 0;
                _nextCompletionTime = 0f;

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

            if (!_exitRequested && Time.time - _resultShownAt > 1.2f)
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
        private static bool TraceEnabled()
        {
            try { return File.Exists(Path.Combine(Paths.BepInExRootPath, TraceFlagName)); }
            catch { return false; }
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
        private const BindingFlags SFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly object[] EmptyArgs = new object[0];
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

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
            var t = FindType(typeName);
            if (t != null && t.IsEnum)
            {
                try { return Enum.Parse(t, member); } catch { }
                try { return Enum.ToObject(t, fallback); } catch { }
            }
            return null;
        }

        private static object M(object o, string name)
        {
            if (o == null) return null;
            var t = o.GetType();
            while (t != null)
            {
                try
                {
                    var p = t.GetProperty(name, Flags);
                    if (p != null && p.GetIndexParameters().Length == 0)
                        return p.GetValue(o, null);
                }
                catch { return null; }

                try
                {
                    var f = t.GetField(name, Flags);
                    if (f != null) return f.GetValue(o);
                }
                catch { return null; }

                t = t.BaseType;
            }
            return null;
        }

        private bool SetMember(object o, string name, object value)
        {
            if (o == null) return false;
            var t = o.GetType();
            while (t != null)
            {
                try
                {
                    var p = t.GetProperty(name, Flags);
                    if (p != null && p.GetIndexParameters().Length == 0 && p.CanWrite)
                    {
                        p.SetValue(o, ConvertArg(p.PropertyType, value), null);
                        return true;
                    }
                }
                catch { }

                try
                {
                    var f = t.GetField(name, Flags);
                    if (f != null && !f.IsInitOnly)
                    {
                        f.SetValue(o, ConvertArg(f.FieldType, value));
                        return true;
                    }
                }
                catch { }

                // auto-property backing field
                try
                {
                    var bf = t.GetField("<" + name + ">k__BackingField", Flags);
                    if (bf != null)
                    {
                        bf.SetValue(o, ConvertArg(bf.FieldType, value));
                        return true;
                    }
                }
                catch { }

                t = t.BaseType;
            }
            return false;
        }

        private object Call(object target, string name, params object[] args)
        {
            if (target == null) return null;
            if (args == null) args = EmptyArgs;
            var t = target.GetType();
            while (t != null)
            {
                MethodInfo best = null;
                try
                {
                    foreach (var m in t.GetMethods(Flags))
                    {
                        if (m.Name != name) continue;
                        var ps = m.GetParameters();
                        if (ps.Length != args.Length) continue;
                        bool ok = true;
                        for (int i = 0; i < ps.Length; i++)
                        {
                            var pt = ps[i].ParameterType;
                            if (args[i] == null)
                            {
                                if (pt.IsValueType && Nullable.GetUnderlyingType(pt) == null) { ok = false; break; }
                                continue;
                            }
                            if (!pt.IsAssignableFrom(args[i].GetType())) { ok = false; break; }
                        }
                        if (ok) { best = m; break; }
                    }
                }
                catch { }

                if (best != null)
                {
                    var ps = best.GetParameters();
                    var converted = new object[args.Length];
                    for (int i = 0; i < args.Length; i++)
                        converted[i] = ConvertArg(ps[i].ParameterType, args[i]);
                    try
                    {
                        return best.Invoke(target, converted);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw e.InnerException ?? e;
                    }
                }
                t = t.BaseType;
            }

            string key = target.GetType().Name + "." + name + "/" + args.Length;
            if (_methodMissLogged.Add(key))
                Logger.LogWarning("Metot bulunamadi: " + key);
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
            var bt = t;
            while (bt != null)
            {
                try
                {
                    var p = bt.GetProperty("Instance", SFlags);
                    if (p != null)
                    {
                        var v = p.GetValue(null, null);
                        if (v != null) return v;
                    }
                }
                catch { }
                foreach (var fn in new[] { "m_instance", "_instance", "instance", "s_instance" })
                {
                    try
                    {
                        var f = bt.GetField(fn, SFlags);
                        if (f != null)
                        {
                            var v = f.GetValue(null);
                            if (v != null) return v;
                        }
                    }
                    catch { }
                }
                bt = bt.BaseType;
            }
            return null;
        }
    }
}
