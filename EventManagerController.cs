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
        private static float _lastTapCheck = 0f;

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
            // 1. Otomatik Olay Savuşturma (Auto-Defend)
            if (AutoDefendEnabled && Time.unscaledTime >= _suppressAutoDefendUntil)
            {
                if (MonoSingleton<Vault>.IsInstanceValid)
                {
                    var vault = MonoSingleton<Vault>.Instance;
                    if (vault != null && vault.EmergencyState != null)
                    {
                        var state = vault.EmergencyState;
                        bool hasRoomEmergency = state.EmergencyRooms != null && state.EmergencyRooms.Count > 0;
                        if (state.InEmergency || state.EmergencyTriggered || hasRoomEmergency)
                        {
                            StopAllIncidents();
                        }
                    }
                }
            }

            // 2. Ziyaretçileri Otomatik Yakalama (Mysterious Stranger & Bottle/Cappy)
            if (AutoTapVisitorsEnabled && Time.unscaledTime - _lastTapCheck >= 0.25f)
            {
                _lastTapCheck = Time.unscaledTime;

                // Gizemli Yabancı
                if (MonoSingleton<MysteriousStrangerMgr>.IsInstanceValid)
                {
                    var sm = MonoSingleton<MysteriousStrangerMgr>.Instance;
                    if (sm != null && sm.CurrentMysteriousStranger != null)
                    {
                        var stranger = sm.CurrentMysteriousStranger;
                        if (!stranger.AlreadyTapped && stranger.CanClick)
                        {
                            try
                            {
                                stranger.OnHandleTapDragable();
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
            if (state.EmergencyRooms != null)
            {
                var rooms = new List<Room>(state.EmergencyRooms);
                foreach (var r in rooms)
                {
                    if (r != null)
                    {
                        try
                        {
                            state.ForceStopEmergencyInRoom(r);
                        }
                        catch { }
                    }
                }
            }

            // 2. Kapıdan giren tüm istilacı yöneticilerini durdur & temizle
            try
            {
                object[] mgrs = new object[]
                {
                    state.RaiderMgr,
                    state.DeathclawMgr,
                    state.AlienMgr,
                    state.GhoulMgr,
                    state.ProtectronMgr,
                    state.ScorchedMgr,
                    state.SecuritronMgr,
                    state.EnclaveSecurityMgr,
                    state.EnclavePowerArmorMgr,
                    state.Gen1SynthMgr,
                    state.Gen2SynthMgr
                };

                foreach (var mgr in mgrs)
                {
                    if (mgr != null)
                    {
                        try
                        {
                            var mForceStop = mgr.GetType().GetMethod("ForceStop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (mForceStop != null)
                            {
                                mForceStop.Invoke(mgr, null);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // 3. VaultEmergencyState temizliği
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
                var fCanAppear = typeof(MysteriousStrangerMgr).GetField("m_canAppear", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fCanAppear != null) fCanAppear.SetValue(mgr, true);

                // Halihazırdaki bekleme coroutine'ini durdur
                var fAppear = typeof(MysteriousStrangerMgr).GetField("m_appear", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fAppear != null)
                {
                    var coroutine = fAppear.GetValue(mgr) as Coroutine;
                    if (coroutine != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        MonoSingleton<GameCoroutineMgr>.Instance.StopCoroutine(coroutine);
                        fAppear.SetValue(mgr, null);
                    }
                }

                // ShowMysteriousStranger çağır
                var mShow = typeof(MysteriousStrangerMgr).GetMethod("ShowMysteriousStranger", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (mShow != null)
                {
                    mShow.Invoke(mgr, null);
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
                var fLocked = typeof(BottleAndCappyMgr).GetField("m_locked", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fLocked != null) fLocked.SetValue(mgr, false);

                var fCur = typeof(BottleAndCappyMgr).GetField("m_currentCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fCur != null)
                {
                    var cor = fCur.GetValue(mgr) as IEnumerator;
                    if (cor != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        MonoSingleton<GameCoroutineMgr>.Instance.StopCoroutine(cor);
                    }
                }

                var mStartFlow = typeof(BottleAndCappyMgr).GetMethod("StartApperanceFlow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (mStartFlow != null)
                {
                    var enumerator = mStartFlow.Invoke(mgr, null) as IEnumerator;
                    if (enumerator != null && MonoSingleton<GameCoroutineMgr>.IsInstanceValid)
                    {
                        if (fCur != null) fCur.SetValue(mgr, enumerator);
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
