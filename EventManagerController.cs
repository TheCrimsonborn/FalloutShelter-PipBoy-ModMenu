using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using FSAutoQuest.Menu;
using UnityEngine;

namespace FSAutoQuest.Events
{
    public static class EventManagerController
    {
        private static ConfigEntry<bool> _cfgAutoDefend;
        private static ConfigEntry<bool> _cfgAutoTapVisitors;
        private static float _suppressAutoDefendUntil = 0f;
        private static float _lastDefendCheck = 0f;
        private static float _lastTapCheck = 0f;

        private sealed class InvaderMgrReflection
        {
            public readonly PropertyInfo HasInvaders;
            public readonly MethodInfo IsAlive;
            public readonly MethodInfo ForceStop;
            public readonly MethodInfo CleanMgr;

            public InvaderMgrReflection(Type t)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                HasInvaders = t.GetProperty("HasInvaders", flags);
                IsAlive = t.GetMethod("IsAlive", flags);
                ForceStop = t.GetMethod("ForceStop", flags);
                CleanMgr = t.GetMethod("CleanMgr", flags);
            }
        }

        private static readonly Dictionary<Type, InvaderMgrReflection> _invaderMgrCache = new Dictionary<Type, InvaderMgrReflection>();
        private static readonly List<Room> _emergencyRoomsScratch = new List<Room>();

        private static readonly FieldInfo _fStrangerCanAppear = typeof(MysteriousStrangerMgr).GetField("m_canAppear", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _fStrangerAppear = typeof(MysteriousStrangerMgr).GetField("m_appear", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _mShowStranger = typeof(MysteriousStrangerMgr).GetMethod("ShowMysteriousStranger", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly FieldInfo _fBottleLocked = typeof(BottleAndCappyMgr).GetField("m_locked", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _fBottleCurCoroutine = typeof(BottleAndCappyMgr).GetField("m_currentCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _mBottleStartFlow = typeof(BottleAndCappyMgr).GetMethod("StartApperanceFlow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static InvaderMgrReflection GetInvaderMgrReflection(object mgr)
        {
            Type t = mgr.GetType();
            InvaderMgrReflection info;
            if (!_invaderMgrCache.TryGetValue(t, out info))
            {
                info = new InvaderMgrReflection(t);
                _invaderMgrCache[t] = info;
            }
            return info;
        }

        private static bool IsInvaderMgrActive(object mgr)
        {
            if (mgr == null) return false;
            var info = GetInvaderMgrReflection(mgr);
            if (info.HasInvaders != null && (bool)info.HasInvaders.GetValue(mgr, null))
                return true;
            if (info.IsAlive != null && (bool)info.IsAlive.Invoke(mgr, null))
                return true;
            return false;
        }

        private static void StopInvaderMgr(object mgr)
        {
            if (mgr == null) return;
            try
            {
                var info = GetInvaderMgrReflection(mgr);
                if (info.ForceStop != null) info.ForceStop.Invoke(mgr, null);
                if (info.CleanMgr != null) info.CleanMgr.Invoke(mgr, null);
            }
            catch { }
        }

        public static bool AutoDefendEnabled
        {
            get { return _cfgAutoDefend != null && _cfgAutoDefend.Value; }
            set
            {
                if (_cfgAutoDefend != null)
                {
                    _cfgAutoDefend.Value = value;
                    if (MenuPlugin.Instance != null && MenuPlugin.Instance.Config != null)
                    {
                        try { MenuPlugin.Instance.Config.Save(); } catch { }
                    }
                }
            }
        }

        public static bool AutoTapVisitorsEnabled
        {
            get { return _cfgAutoTapVisitors != null && _cfgAutoTapVisitors.Value; }
            set
            {
                if (_cfgAutoTapVisitors != null)
                {
                    _cfgAutoTapVisitors.Value = value;
                    if (MenuPlugin.Instance != null && MenuPlugin.Instance.Config != null)
                    {
                        try { MenuPlugin.Instance.Config.Save(); } catch { }
                    }
                }
            }
        }

        public static void Initialize(ConfigFile config)
        {
            _cfgAutoDefend = config.Bind("EventManager", "AutoDefendIncidents", false,
                "Kasada herhangi bir yangin, istila veya baskin basladiginda aninda otomatik bitir.");
            _cfgAutoTapVisitors = config.Bind("EventManager", "AutoTapVisitors", false,
                "Gizemli Yabanci veya Bottle & Cappy belirdiginde onlari aninda otomatik tiklayip odulleri topla.");
        }

        public static void TickUpdate()
        {
            float now = Time.unscaledTime;

            // 1. Otomatik Olay Savuşturma (Auto-Defend)
            if (AutoDefendEnabled && now >= _suppressAutoDefendUntil && now - _lastDefendCheck >= 0.25f)
            {
                _lastDefendCheck = now;
                if (MonoSingleton<Vault>.IsInstanceValid)
                {
                    var vault = MonoSingleton<Vault>.Instance;
                    if (vault != null && vault.EmergencyState != null)
                    {
                        var state = vault.EmergencyState;
                        if (HasActiveIncident(state))
                        {
                            StopAllIncidents();
                        }
                    }
                }
            }

            // 2. Ziyaretçileri Otomatik Yakalama (Mysterious Stranger & Bottle/Cappy)
            if (AutoTapVisitorsEnabled && now - _lastTapCheck >= 0.25f)
            {
                _lastTapCheck = now;

                // Gizemli Yabancı
                if (MonoSingleton<MysteriousStrangerMgr>.IsInstanceValid)
                {
                    var sm = MonoSingleton<MysteriousStrangerMgr>.Instance;
                    if (sm != null && sm.CurrentMysteriousStranger != null)
                    {
                        var stranger = sm.CurrentMysteriousStranger;
                        if (!stranger.AlreadyTapped)
                        {
                            try
                            {
                                stranger.CanClick = true;
                                stranger.OnHandleTapDragable();
                                if (!stranger.AlreadyTapped)
                                {
                                    sm.NotifyMysteriousStrangerTapped();
                                    stranger.AlreadyTapped = true;
                                }
                            }
                            catch { }
                        }
                    }
                }

                // Bottle & Cappy
                if (MonoSingleton<BottleAndCappyMgr>.IsInstanceValid)
                {
                    var bm = MonoSingleton<BottleAndCappyMgr>.Instance;
                    if (bm != null)
                    {
                        try
                        {
                            if (bm.Bottle != null) bm.Bottle.OnHandleTapDragable();
                            if (bm.Cappy != null) bm.Cappy.OnHandleTapDragable();
                        }
                        catch { }
                    }
                }
            }
        }

        public static bool HasActiveIncident(VaultEmergencyState state)
        {
            if (state == null) return false;
            if (state.InEmergency || state.InInvasion || state.IsInvaderEmergency || state.EmergencyTriggered)
                return true;
            if (state.EmergencyRooms != null && state.EmergencyRooms.Count > 0)
                return true;

            try
            {
                if (IsInvaderMgrActive(state.RaiderMgr)
                    || IsInvaderMgrActive(state.DeathclawMgr)
                    || IsInvaderMgrActive(state.AlienMgr)
                    || IsInvaderMgrActive(state.GhoulMgr)
                    || IsInvaderMgrActive(state.ProtectronMgr)
                    || IsInvaderMgrActive(state.ScorchedMgr)
                    || IsInvaderMgrActive(state.SecuritronMgr)
                    || IsInvaderMgrActive(state.EnclaveSecurityMgr)
                    || IsInvaderMgrActive(state.EnclavePowerArmorMgr)
                    || IsInvaderMgrActive(state.Gen1SynthMgr)
                    || IsInvaderMgrActive(state.Gen2SynthMgr))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static void TriggerEmergency(ERoomEmergencyMode mode)
        {
            if (!MonoSingleton<Vault>.IsInstanceValid) return;
            var vault = MonoSingleton<Vault>.Instance;
            if (vault == null || vault.EmergencyState == null) return;

            var state = vault.EmergencyState;

            // Kullanıcı bilinçli olarak baskın başlattığında, oto-savunma ilk 6 saniye bu olayı öldürmesin
            _suppressAutoDefendUntil = Time.unscaledTime + 6f;

            // Önceki bir felaket devam ediyorsa temizleyip yeniye yer aç
            if (state.InEmergency || state.EmergencyTriggered || (state.EmergencyRooms != null && state.EmergencyRooms.Count > 0))
            {
                StopAllIncidentsInternal(state);
            }

            state.EmergencyTriggered = false;
            state.Enabled = true;

            IEmergencyManager mgr = VaultEmergencyTypes.GetEmergencyManager(mode);
            if (mgr != null)
            {
                // Dışarıdan kapıya gelen istilacılar (Raiders, Deathclaw, Alien, Ghoul, Synth, Enclave vb.)
                try
                {
                    mgr.BeginAttack();
                    if (MonoSingleton<TutorialManager>.IsInstanceValid)
                    {
                        MonoSingleton<TutorialManager>.Instance.OnIncidentOccurs(mode);
                    }
                }
                catch (Exception ex)
                {
                    if (MenuPlugin.Log != null) MenuPlugin.Log.LogWarning("BeginAttack hatasi (" + mode + "): " + ex.Message);
                }
            }
            else
            {
                // Oda içi felaketler (Yangın, Radroach, Molerat, Radscorpion)
                Room targetRoom = vault.GetRandomRoomForEmergency();
                if (targetRoom == null && vault.Rooms != null && vault.Rooms.Count > 0)
                {
                    foreach (var r in vault.Rooms)
                    {
                        if (r != null && r.RoomSections != null && r.RoomSections.Count > 0 && r.RoomType != ERoomType.Elevator)
                        {
                            targetRoom = r;
                            break;
                        }
                    }
                }

                if (targetRoom != null)
                {
                    try
                    {
                        targetRoom.StartEmergency(mode, null);
                        if (MonoSingleton<TutorialManager>.IsInstanceValid)
                        {
                            MonoSingleton<TutorialManager>.Instance.OnIncidentOccurs(mode);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (MenuPlugin.Log != null) MenuPlugin.Log.LogWarning("StartEmergency hatasi: " + ex.Message);
                    }
                }
            }
        }

        public static void StopAllIncidents()
        {
            if (!MonoSingleton<Vault>.IsInstanceValid) return;
            var vault = MonoSingleton<Vault>.Instance;
            if (vault == null || vault.EmergencyState == null) return;

            StopAllIncidentsInternal(vault.EmergencyState);
        }

        private static void StopAllIncidentsInternal(VaultEmergencyState state)
        {
            if (state == null) return;

            // 1. Tüm oda içi acil durumları durdur
            if (state.EmergencyRooms != null && state.EmergencyRooms.Count > 0)
            {
                _emergencyRoomsScratch.Clear();
                _emergencyRoomsScratch.AddRange(state.EmergencyRooms);
                for (int i = 0; i < _emergencyRoomsScratch.Count; i++)
                {
                    var r = _emergencyRoomsScratch[i];
                    if (r != null)
                    {
                        try
                        {
                            state.ForceStopEmergencyInRoom(r);
                        }
                        catch { }
                    }
                }
                _emergencyRoomsScratch.Clear();
            }

            // 2. VaultEmergencyState dahili invader temizliği
            try
            {
                state.FixEmergency();
            }
            catch { }

            // 3. Kapıdan giren tüm istilacı yöneticilerini durdur & temizle
            try
            {
                StopInvaderMgr(state.RaiderMgr);
                StopInvaderMgr(state.DeathclawMgr);
                StopInvaderMgr(state.AlienMgr);
                StopInvaderMgr(state.GhoulMgr);
                StopInvaderMgr(state.ProtectronMgr);
                StopInvaderMgr(state.ScorchedMgr);
                StopInvaderMgr(state.SecuritronMgr);
                StopInvaderMgr(state.EnclaveSecurityMgr);
                StopInvaderMgr(state.EnclavePowerArmorMgr);
                StopInvaderMgr(state.Gen1SynthMgr);
                StopInvaderMgr(state.Gen2SynthMgr);
            }
            catch { }

            // 4. VaultEmergencyState temizliği
            try
            {
                state.StopEmergency();
            }
            catch { }

            state.EmergencyTriggered = false;
        }

        public static void SpawnMysteriousStranger()
        {
            if (!MonoSingleton<MysteriousStrangerMgr>.IsInstanceValid) return;
            var mgr = MonoSingleton<MysteriousStrangerMgr>.Instance;
            if (mgr == null) return;

            try
            {
                // Eğer sahnede mevcut bir yabancı varsa ödülü al
                if (mgr.CurrentMysteriousStranger != null)
                {
                    if (!mgr.CurrentMysteriousStranger.AlreadyTapped)
                    {
                        mgr.CurrentMysteriousStranger.OnHandleTapDragable();
                    }
                }

                // Belirme iznini aç
                if (_fStrangerCanAppear != null) _fStrangerCanAppear.SetValue(mgr, true);

                // Halihazırdaki bekleme coroutine'ini durdur
                if (_fStrangerAppear != null)
                {
                    var coroutine = _fStrangerAppear.GetValue(mgr) as Coroutine;
                    if (coroutine != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        MonoSingleton<GameCoroutineMgr>.Instance.StopCoroutine(coroutine);
                        _fStrangerAppear.SetValue(mgr, null);
                    }
                }

                // ShowMysteriousStranger çağır
                if (_mShowStranger != null)
                {
                    _mShowStranger.Invoke(mgr, null);
                }
                else
                {
                    mgr.ForceAppearMysteriousStranger();
                }
            }
            catch (Exception ex)
            {
                if (MenuPlugin.Log != null) MenuPlugin.Log.LogWarning("SpawnMysteriousStranger hatasi: " + ex.Message);
            }
        }

        public static void SpawnBottleAndCappy()
        {
            if (!MonoSingleton<BottleAndCappyMgr>.IsInstanceValid) return;
            var mgr = MonoSingleton<BottleAndCappyMgr>.Instance;
            if (mgr == null) return;

            try
            {
                if (_fBottleLocked != null) _fBottleLocked.SetValue(mgr, false);

                if (_fBottleCurCoroutine != null)
                {
                    var cor = _fBottleCurCoroutine.GetValue(mgr) as IEnumerator;
                    if (cor != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        MonoSingleton<GameCoroutineMgr>.Instance.StopCoroutine(cor);
                    }
                }

                if (_mBottleStartFlow != null)
                {
                    var enumerator = _mBottleStartFlow.Invoke(mgr, null) as IEnumerator;
                    if (enumerator != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        if (_fBottleCurCoroutine != null) _fBottleCurCoroutine.SetValue(mgr, enumerator);
                        MonoSingleton<GameCoroutineMgr>.Instance.StartCoroutine(enumerator);
                    }
                }
                else
                {
                    mgr.Spawn();
                }
            }
            catch (Exception ex)
            {
                if (MenuPlugin.Log != null) MenuPlugin.Log.LogWarning("SpawnBottleAndCappy hatasi: " + ex.Message);
            }
        }
    }
}
