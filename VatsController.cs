using System;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace FSAutoQuest.Vats
{
    /// <summary>
    /// V.A.T.S. (Vault Accelerated Time System) modunu RobCo Pip-Boy Mod Menusu ile tam uyumlu yoneten kontrolcu.
    /// VATS.dll yukluyse onunla senkronize calisir; yuklu degilse kendi ic zaman motorunu kullanir.
    /// </summary>
    public static class VatsController
    {
        public static ConfigEntry<bool> CfgEnabled;
        public static ConfigEntry<float> CfgMultiplier;
        public static ConfigEntry<float> CfgWarpMultiplier;
        public static ConfigEntry<float> CfgAnimSpeed;
        public static ConfigEntry<bool> CfgAutoPauseIncidents;
        public static ConfigEntry<bool> CfgAutoPauseStranger;
        public static ConfigEntry<bool> CfgShowOverlay;

        private static bool _standaloneActive = false;
        private static bool _standaloneWarp = false;

        // VATS.dll eklentisi yukluyse refleksiyonla baglanacak alanlar
        private static object _vatsPluginInstance = null;
        private static FieldInfo _fToggleActive = null;
        private static FieldInfo _fToggleWarp = null;
        private static FieldInfo _fFullMult = null;
        private static FieldInfo _fWarpMult = null;
        private static FieldInfo _fAnimSpeed = null;
        private static FieldInfo _fAutoPauseIncidents = null;
        private static FieldInfo _fOverlayOpacity = null;
        private static MethodInfo _mApplyBoost = null;
        private static MethodInfo _mResetIfBoosting = null;

        private static bool _vatsResolved = false;

        public static void Initialize(ConfigFile config)
        {
            CfgEnabled = config.Bind("VATS", "Etkin", true, "V.A.T.S. Hizlandirma Sistemi acik mi?");
            CfgMultiplier = config.Bind("VATS", "HizCarpani", 10f, "Standart hizlandirma carpani (Varsayilan 10x).");
            CfgWarpMultiplier = config.Bind("VATS", "ZamanBukmeCarpani", 50f, "Zaman bukme carpani (Varsayilan 50x).");
            CfgAnimSpeed = config.Bind("VATS", "AnimasyonHizi", 3f, "Gorsel animasyon hizi (Varsayilan 3x).");
            CfgAutoPauseIncidents = config.Bind("VATS", "FelaketteDuraklat", true, "Yangin/baskin aninda zamani normale dondur.");
            CfgAutoPauseStranger = config.Bind("VATS", "YabancidaDuraklat", true, "Gizemli Yabanci belirdiginde duraklat.");
            CfgShowOverlay = config.Bind("VATS", "GostergeEfekti", true, "Ekranda CRT tarama cizgilerini ve sayaci goster.");
        }

        private static void EnsureVatsBinding()
        {
            if (_vatsResolved && _vatsPluginInstance != null) return;
            _vatsResolved = true;

            try
            {
                Type vatsType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var t = asm.GetType("VATS.SpeedBoostPlugin");
                        if (t != null) { vatsType = t; break; }
                    }
                    catch { }
                }

                if (vatsType != null)
                {
                    var comp = UnityEngine.Object.FindObjectOfType(vatsType);
                    _vatsPluginInstance = comp;

                    var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    _fToggleActive = vatsType.GetField("_toggleActive", flags);
                    _fToggleWarp = vatsType.GetField("_toggleWarp", flags);
                    _fFullMult = vatsType.GetField("_fullMult", flags);
                    _fWarpMult = vatsType.GetField("_warpMult", flags);
                    _fAnimSpeed = vatsType.GetField("_animSpeed", flags);
                    _fAutoPauseIncidents = vatsType.GetField("_autoPauseIncidents", flags);
                    _fOverlayOpacity = vatsType.GetField("_overlayOpacity", flags);
                    _mApplyBoost = vatsType.GetMethod("ApplyBoost", flags);
                    _mResetIfBoosting = vatsType.GetMethod("ResetIfBoosting", flags);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[VATS] Binding hatasi: " + ex.Message);
            }
        }

        public static bool IsBoostEnabled
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fToggleActive != null && _fToggleWarp != null)
                {
                    try
                    {
                        bool active = (bool)_fToggleActive.GetValue(_vatsPluginInstance);
                        bool warp = (bool)_fToggleWarp.GetValue(_vatsPluginInstance);
                        return active && !warp;
                    }
                    catch { }
                }
                return _standaloneActive && !_standaloneWarp;
            }
            set
            {
                if (value)
                {
                    SetBoost(true, false);
                }
                else
                {
                    SetReset();
                }
            }
        }

        public static bool IsTimeWarpEnabled
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fToggleActive != null && _fToggleWarp != null)
                {
                    try
                    {
                        bool active = (bool)_fToggleActive.GetValue(_vatsPluginInstance);
                        bool warp = (bool)_fToggleWarp.GetValue(_vatsPluginInstance);
                        return active && warp;
                    }
                    catch { }
                }
                return _standaloneActive && _standaloneWarp;
            }
            set
            {
                if (value)
                {
                    SetBoost(true, true);
                }
                else
                {
                    SetReset();
                }
            }
        }

        public static float SpeedMultiplier
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fFullMult != null)
                {
                    try
                    {
                        var ce = _fFullMult.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) return ce.Value;
                    }
                    catch { }
                }
                return CfgMultiplier != null ? CfgMultiplier.Value : 10f;
            }
            set
            {
                EnsureVatsBinding();
                if (CfgMultiplier != null) CfgMultiplier.Value = value;
                if (_vatsPluginInstance != null && _fFullMult != null)
                {
                    try
                    {
                        var ce = _fFullMult.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) ce.Value = value;
                    }
                    catch { }
                }
                if (IsBoostEnabled)
                {
                    SetBoost(true, false);
                }
            }
        }

        public static float AnimationSpeed
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fAnimSpeed != null)
                {
                    try
                    {
                        var ce = _fAnimSpeed.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) return ce.Value;
                    }
                    catch { }
                }
                return CfgAnimSpeed != null ? CfgAnimSpeed.Value : 3f;
            }
            set
            {
                EnsureVatsBinding();
                if (CfgAnimSpeed != null) CfgAnimSpeed.Value = value;
                if (_vatsPluginInstance != null && _fAnimSpeed != null)
                {
                    try
                    {
                        var ce = _fAnimSpeed.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) ce.Value = value;
                    }
                    catch { }
                }
                if (IsBoostEnabled)
                {
                    SetBoost(true, false);
                }
            }
        }

        public static bool AutoPauseIncidents
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fAutoPauseIncidents != null)
                {
                    try
                    {
                        var ce = _fAutoPauseIncidents.GetValue(_vatsPluginInstance) as ConfigEntry<bool>;
                        if (ce != null) return ce.Value;
                    }
                    catch { }
                }
                return CfgAutoPauseIncidents == null || CfgAutoPauseIncidents.Value;
            }
            set
            {
                EnsureVatsBinding();
                if (CfgAutoPauseIncidents != null) CfgAutoPauseIncidents.Value = value;
                if (_vatsPluginInstance != null && _fAutoPauseIncidents != null)
                {
                    try
                    {
                        var ce = _fAutoPauseIncidents.GetValue(_vatsPluginInstance) as ConfigEntry<bool>;
                        if (ce != null) ce.Value = value;
                    }
                    catch { }
                }
            }
        }

        public static bool AutoPauseStranger
        {
            get => CfgAutoPauseStranger == null || CfgAutoPauseStranger.Value;
            set { if (CfgAutoPauseStranger != null) CfgAutoPauseStranger.Value = value; }
        }

        public static bool ShowOverlay
        {
            get
            {
                EnsureVatsBinding();
                if (_vatsPluginInstance != null && _fOverlayOpacity != null)
                {
                    try
                    {
                        var ce = _fOverlayOpacity.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) return ce.Value > 0.05f;
                    }
                    catch { }
                }
                return CfgShowOverlay == null || CfgShowOverlay.Value;
            }
            set
            {
                EnsureVatsBinding();
                if (CfgShowOverlay != null) CfgShowOverlay.Value = value;
                if (_vatsPluginInstance != null && _fOverlayOpacity != null)
                {
                    try
                    {
                        var ce = _fOverlayOpacity.GetValue(_vatsPluginInstance) as ConfigEntry<float>;
                        if (ce != null) ce.Value = value ? 0.7f : 0f;
                    }
                    catch { }
                }
            }
        }

        public static void SetBoost(bool active, bool warp)
        {
            EnsureVatsBinding();
            if (_vatsPluginInstance != null)
            {
                try
                {
                    if (_fToggleActive != null) _fToggleActive.SetValue(_vatsPluginInstance, active);
                    if (_fToggleWarp != null) _fToggleWarp.SetValue(_vatsPluginInstance, warp);
                    if (active)
                    {
                        float mult = warp ? (CfgWarpMultiplier?.Value ?? 50f) : SpeedMultiplier;
                        if (_mApplyBoost != null)
                        {
                            _mApplyBoost.Invoke(_vatsPluginInstance, new object[] { mult, warp });
                        }
                    }
                    else
                    {
                        if (_mResetIfBoosting != null)
                        {
                            _mResetIfBoosting.Invoke(_vatsPluginInstance, null);
                        }
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[VATS] SetBoost hatasi: " + ex.Message);
                }
            }

            // Standalone motoru
            _standaloneActive = active;
            _standaloneWarp = warp;
            if (!active)
            {
                ApplyEngineTimescale(1f, 1f);
            }
            else
            {
                float mult = warp ? (CfgWarpMultiplier?.Value ?? 50f) : (CfgMultiplier?.Value ?? 10f);
                float anim = warp ? mult : (CfgAnimSpeed?.Value ?? 3f);
                ApplyEngineTimescale(mult, anim);
            }
        }

        public static void SetReset()
        {
            SetBoost(false, false);
        }

        public static void TickUpdate()
        {
            EnsureVatsBinding();
            if (_vatsPluginInstance != null)
            {
                // VATS.dll kendisi Update ve LateUpdate icinde zaman skalasini tazeliyor
                return;
            }

            if (!_standaloneActive) return;

            if (!Application.isFocused || !MonoSingleton<Vault>.IsInstanceValid)
            {
                ApplyEngineTimescale(1f, 1f);
                return;
            }

            if (AutoPauseStranger && IsStrangerAppearing())
            {
                ApplyEngineTimescale(1f, 1f);
                return;
            }

            if (AutoPauseIncidents && IsVaultInEmergency())
            {
                ApplyEngineTimescale(2f, 1f);
                return;
            }

            float mult = _standaloneWarp ? (CfgWarpMultiplier?.Value ?? 50f) : (CfgMultiplier?.Value ?? 10f);
            float anim = _standaloneWarp ? mult : (CfgAnimSpeed?.Value ?? 3f);
            ApplyEngineTimescale(mult, anim);
        }

        private static void ApplyEngineTimescale(float gameClockMult, float animMult)
        {
            try
            {
                if (MonoSingleton<TimeMgr>.IsInstanceValid)
                {
                    MonoSingleton<TimeMgr>.Instance.m_timeScale = gameClockMult;
                }
                Time.timeScale = animMult;
            }
            catch { }
        }

        private static bool IsVaultInEmergency()
        {
            try
            {
                if (MonoSingleton<Vault>.IsInstanceValid)
                {
                    var rooms = MonoSingleton<Vault>.Instance.Rooms;
                    if (rooms != null)
                    {
                        for (int i = 0; i < rooms.Count; i++)
                        {
                            var r = rooms[i];
                            if (r != null && r.IsInEmergency)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool IsStrangerAppearing()
        {
            try
            {
                if (MonoSingleton<MysteriousStrangerMgr>.IsInstanceValid)
                {
                    return MonoSingleton<MysteriousStrangerMgr>.Instance.CurrentState == EMysteriousStrangerState.Appearing;
                }
            }
            catch { }
            return false;
        }
    }
}
