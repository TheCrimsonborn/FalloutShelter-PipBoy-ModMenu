using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FSAutoQuest
{
    /// <summary>
    /// Kesif plugin'i: NGUI arayuzu + canli gorev durumunu dosyaya doker.
    /// F7 = tam dokum, F8 = sadece UI (hizli).
    /// F9 = gorev kurtarma (QuickLoading kilidini acmayi dener).
    /// F10 = canli izleme ac/kapa (BepInEx\FSAutoQuest-trace.on dosyasi).
    /// Yedek tetikleyici: BepInEx\FSAutoQuest-dump.trigger dosyasini olustur.
    /// Yeni Input System kullanilir, eski Input API'si ASLA cagrilmaz.
    /// </summary>
    [BepInPlugin("com.halil.fsautoquest.probe", "FSAutoQuest Probe", "0.3.0")]
    public class ProbePlugin : BaseUnityPlugin
    {
        private const string DumpPath = "FSAutoQuest-probe.txt";
        private const string TriggerName = "FSAutoQuest-dump.trigger";
        private const string TraceFlagName = "FSAutoQuest-trace.on";
        private static ManualLogSource Log;

        private const BindingFlags InstFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private bool _inputErrorLogged;
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Assembly, Type[]> AsmTypes = new Dictionary<Assembly, Type[]>();

        private void Awake()
        {
            Log = Logger;
            Application.runInBackground = true;
            Logger.LogInfo("FSAutoQuest probe 0.3.0 yuklendi. F7 = tam dokum, F8 = UI, F9 = gorev kurtarma, F10 = izleme.");
        }

        private void Update()
        {
            try
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.f7Key.wasPressedThisFrame) { Dump(true); return; }
                    if (kb.f8Key.wasPressedThisFrame) { Dump(false); return; }
                    if (kb.f9Key.wasPressedThisFrame) { RecoverQuest(); return; }
                    if (kb.f10Key.wasPressedThisFrame) { ToggleTrace(); return; }
                }

                if (Time.frameCount % 30 == 0 && TraceEnabled())
                    Logger.LogInfo(TraceLine());

                if (Time.frameCount % 60 == 0)
                {
                    string trig = Path.Combine(Paths.BepInExRootPath, TriggerName);
                    if (File.Exists(trig))
                    {
                        try { File.Delete(trig); } catch { }
                        Dump(true);
                    }
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

        // ------------------------------------------------------------- trace
        private static string TracePath { get { return Path.Combine(Paths.BepInExRootPath, TraceFlagName); } }
        private static bool TraceEnabled() { try { return File.Exists(TracePath); } catch { return false; } }

        private void ToggleTrace()
        {
            try
            {
                if (File.Exists(TracePath)) File.Delete(TracePath);
                else File.WriteAllText(TracePath, "on");
                Logger.LogInfo("Izleme: " + (File.Exists(TracePath) ? "ACIK" : "KAPALI"));
            }
            catch (Exception e) { Logger.LogError("Izleme degistirilemedi: " + e.Message); }
        }

        private string TraceLine()
        {
            var sb = new StringBuilder();
            sb.Append("TRACE scene=").Append(SceneManager.GetActiveScene().name);
            sb.Append(" frame=").Append(Time.frameCount);

            var qdm = Singleton("QuestDataManager");
            if (qdm != null)
            {
                sb.Append(" quest=").Append(M(qdm, "CurrentQuestID"));
                sb.Append(" done=").Append(M(qdm, "IsQuestDone"));
                sb.Append(" ok=").Append(M(qdm, "IsQuestSucceeded"));
                sb.Append(" cancel=").Append(M(qdm, "HasBeenCancelled"));
            }

            var setup = FindAll("QuestSetupReferences").FirstOrDefault();
            if (setup != null)
            {
                sb.Append(" setupDone=").Append(M(setup, "SetupCompleted"));
                sb.Append(" recalcs=").Append(M(setup, "RecalculationCount"));
                sb.Append(" rooms=").Append(CountOf(M(setup, "Rooms")));
            }

            var gvm = Singleton("QuestGameViewManager");
            sb.Append(" gvm=").Append(gvm == null ? "YOK" : NameOf(M(gvm, "CurrentRoom")));

            var rooms = FindAll("QuestRoom");
            int cleared = 0, looted = 0, loot = 0;
            foreach (var r in rooms)
            {
                if (Truthy(M(r, "IsRoomClearOutCompleted"))) cleared++;
                if (Truthy(M(r, "IsRoomPickUpCompleted"))) looted++;
                if (Truthy(M(r, "HasLoot"))) loot++;
            }
            sb.Append(" qrooms=").Append(rooms.Count).Append(" cleared=").Append(cleared)
              .Append(" looted=").Append(looted).Append(" loot=").Append(loot);
            return sb.ToString();
        }

        // ------------------------------------------------------------- dump
        private void Dump(bool full)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== FSAutoQuest probe 0.3.0 ===");
            sb.AppendLine("Tarih: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("Sahne: " + SceneManager.GetActiveScene().name);
            sb.AppendLine("timeScale: " + Time.timeScale + " | frame: " + Time.frameCount);
            sb.AppendLine();

            try
            {
                DumpSetup(sb);
                DumpQuestManagers(sb);
                DumpRooms(sb);
                DumpDwellers(sb);
                DumpUi(sb);
                if (full) DumpHierarchy(sb);
            }
            catch (Exception e)
            {
                sb.AppendLine("HATA: " + e);
            }

            try
            {
                File.WriteAllText(Path.Combine(Paths.BepInExRootPath, DumpPath), sb.ToString(), Encoding.UTF8);
                Logger.LogInfo("Dokum yazildi: BepInEx/" + DumpPath + " (" + sb.Length + " karakter)");
            }
            catch (Exception e)
            {
                Logger.LogError("Dokum yazilamadi: " + e);
            }
        }

        private void DumpSetup(StringBuilder sb)
        {
            sb.AppendLine("### QUEST SETUP");
            var list = FindAll("QuestSetupReferences");
            if (list.Count == 0) sb.AppendLine("(QuestSetupReferences yok)");
            foreach (var s in list)
            {
                sb.AppendLine("SETUP obj=" + NameOf(s));
                AppendKnown(sb, s, new[]
                {
                    "SetupCompleted", "m_initialLoadingDone", "RecalculationCount", "GenerationSeed",
                    "BuildingLenght", "BuildingUndergroundSize", "BuildingHeight", "TotalGridHeight",
                    "ConstructionGridEntryRow", "Rooms", "m_roomList", "m_delayedSetupList",
                    "m_wastelandRoom", "m_entranceRoom", "IsFetchSerializedScenesDone"
                });
            }
            sb.AppendLine();
        }

        private void DumpQuestManagers(StringBuilder sb)
        {
            sb.AppendLine("### QUEST YONETICILERI");

            var qdm = Singleton("QuestDataManager");
            sb.AppendLine("QuestDataManager: " + (qdm == null ? "YOK" : "VAR"));
            if (qdm != null)
                AppendKnown(sb, qdm, new[]
                {
                    "CurrentQuestID", "IsLoaded", "IsQuestDone", "IsQuestSucceeded", "HasBeenCancelled",
                    "EntranceFlowCompleted", "RetryCount", "IncrementedRetryCount", "AccumulatedRetryCost",
                    "CurrentQuestDifficulty", "IsNewQuest", "IsPremadeQuestLayout", "TakenBackCameraControl",
                    "CurrentWastelandTeam", "AvailableClue"
                });

            var qom = Singleton("QuestObjectiveMgr");
            sb.AppendLine("QuestObjectiveMgr: " + (qom == null ? "YOK" : "VAR"));
            if (qom != null)
            {
                AppendKnown(sb, qom, new[] { "ObjectiveRooms", "isPlayingObjectiveCompleteAudio" });
                TryLine(sb, "  primaryCompleted=", () => qom.GetType().GetMethod("IsPrimaryObjectiveCompleted", InstFlags).Invoke(qom, null));
                TryLine(sb, "  secondaryCompleted=", () => qom.GetType().GetMethod("IsSecondaryObjectiveCompleted", InstFlags).Invoke(qom, null));
                TryLine(sb, "  thirdCompleted=", () => qom.GetType().GetMethod("IsThirdObjectiveCompleted", InstFlags).Invoke(qom, null));
                TryLine(sb, "  primaryRoom=", () => NameOf(qom.GetType().GetMethod("GetPrimaryObjectiveRoom", InstFlags).Invoke(qom, null)));
            }

            var gvm = Singleton("QuestGameViewManager");
            sb.AppendLine("QuestGameViewManager: " + (gvm == null ? "YOK" : "VAR"));
            if (gvm != null)
            {
                AppendKnown(sb, gvm, new[] { "CurrentRoom", "RoomId", "CurrentDweller" });
                AppendKnown(sb, gvm, new[] { "m_currentRoom", "m_roomEntered", "m_currentDweller", "m_currentDwellerHandled" });
                AppendKnown(sb, gvm, new[] { "CombatViewState", "OverallViewState", "AttackCommandViewState", "AssignTeamViewState" });
            }

            var qs = Singleton("QuestScene");
            sb.AppendLine("QuestScene: " + (qs == null ? "YOK" : "VAR"));

            var qgm = Singleton("QuestGUIManager");
            sb.AppendLine("QuestGUIManager: " + (qgm == null ? "YOK" : "VAR"));
            sb.AppendLine();
        }

        private void DumpRooms(StringBuilder sb)
        {
            sb.AppendLine("### GOREV ODALARI");
            var rooms = FindAll("QuestRoom");
            sb.AppendLine("QuestRoom sayisi: " + rooms.Count);
            foreach (var r in rooms)
            {
                var comp = r as Component;
                string pos = "?";
                if (comp != null)
                {
                    var t = comp.transform;
                    pos = string.Format("({0:F1},{1:F1})", t.position.x, t.position.y);
                }
                sb.AppendLine("ROOM " + NameOf(r) + " pos=" + pos);
                AppendKnown(sb, r, new[]
                {
                    "QuestRoomType", "PrimaryObjectiveType", "SecondaryObjectiveType", "ThirdObjectiveType",
                    "IsRoomClearOutCompleted", "RoomClearOutResult", "IsRoomClearOutSucceeded",
                    "IsRoomPickUpCompleted", "RoomPickUpResult", "IsRoomPickUpSucceeded",
                    "HasLoot", "HasCompletionRoomLoot", "FighterAliveInstances", "FighterDeadInstances",
                    "ConversationState", "Invisiblestate", "VisibleState", "FightState", "Challengestate",
                    "RoomChallenge", "CombatParameters"
                });
            }
            sb.AppendLine();
        }

        private void DumpDwellers(StringBuilder sb)
        {
            sb.AppendLine("### SAKINLER (Dweller)");
            var dwellers = FindAll("Dweller");
            sb.AppendLine("Dweller sayisi: " + dwellers.Count);
            int n = 0;
            foreach (var d in dwellers)
            {
                string name = NameOf(d);
                string extra = string.Format("hp={0}/{1} dead={2} room={3}",
                    M(d, "Health"), M(d, "HealthMax"), M(d, "IsDead"), NameOf(M(d, "CurrentRoom")));
                sb.AppendLine("DWELLER " + name + " " + extra);
                if (++n >= 40) { sb.AppendLine("... (40 ile sinirlandi)"); break; }
            }
            sb.AppendLine();
        }

        private void DumpUi(StringBuilder sb)
        {
            sb.AppendLine("### NGUI BUTONLARI");
            var buttons = FindAll("UIButton");
            sb.AppendLine("UIButton sayisi: " + buttons.Count);
            foreach (var b in buttons)
            {
                var comp = b as Component;
                if (comp == null || !comp.gameObject.activeInHierarchy) continue;
                sb.AppendLine("BUTTON " + PathOf(comp.transform) +
                              " | label=\"" + Escape(LabelOf(comp.gameObject)) + "\"" +
                              " | enabled=" + M(b, "isEnabled") +
                              " | onClick=" + DelegatesOf(b));
            }
            sb.AppendLine();

            sb.AppendLine("### NGUI YAZILARI");
            var labels = FindAll("UILabel");
            sb.AppendLine("UILabel sayisi: " + labels.Count);
            int shown = 0;
            foreach (var l in labels)
            {
                var comp = l as Component;
                if (comp == null || !comp.gameObject.activeInHierarchy) continue;
                string txt = M(l, "text") as string;
                if (string.IsNullOrEmpty(txt)) txt = M(l, "mText") as string;
                if (string.IsNullOrEmpty(txt)) continue;
                sb.AppendLine("LABEL \"" + Escape(txt) + "\" @ " + PathOf(comp.transform) +
                              " | depth=" + M(l, "depth"));
                if (++shown >= 300) { sb.AppendLine("... (300 ile sinirlandi)"); break; }
            }
            sb.AppendLine();

            sb.AppendLine("### DIALOG / PENCERELER");
            var dialogs = FindAll("UIDialog");
            sb.AppendLine("UIDialog sayisi: " + dialogs.Count);
            foreach (var d in dialogs)
            {
                var comp = d as Component;
                if (comp == null || !comp.gameObject.activeInHierarchy) continue;
                sb.AppendLine("DIALOG " + PathOf(comp.transform) + " | active=" + comp.gameObject.activeSelf);
                var texts = new List<string>();
                foreach (var l in comp.GetComponentsInChildren<Component>(false))
                {
                    if (l == null) continue;
                    if (l.GetType().Name != "UILabel") continue;
                    string t = M(l, "text") as string;
                    if (!string.IsNullOrEmpty(t)) texts.Add(Escape(t));
                }
                sb.AppendLine("   texts=" + string.Join(" | ", texts.Take(10).ToArray()));
            }
            sb.AppendLine();
        }

        private void DumpHierarchy(StringBuilder sb)
        {
            sb.AppendLine("### SAHNE KOKLERI");
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
                sb.AppendLine("ROOT " + root.name + " active=" + root.activeInHierarchy);
            sb.AppendLine();
        }

        // --------------------------------------------------------- recovery
        private void RecoverQuest()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== KURTARMA " + DateTime.Now.ToString("HH:mm:ss") + " sahne=" + SceneManager.GetActiveScene().name + " ===");

            var qdm = Singleton("QuestDataManager");
            sb.AppendLine("QuestDataManager: " + (qdm == null ? "YOK" : "VAR"));
            if (qdm != null)
                AppendKnown(sb, qdm, new[] { "CurrentQuestID", "IsLoaded", "IsQuestDone", "IsQuestSucceeded", "HasBeenCancelled" });

            var qs = Singleton("QuestScene");
            sb.AppendLine("QuestScene: " + (qs == null ? "YOK" : "VAR"));

            if (qdm != null)
            {
                TryCall(sb, "SetQuestFailed", qdm, "SetQuestFailed", new object[0]);
                TryCall(sb, "SetQuestDone", qdm, "SetQuestDone", new object[0]);
            }

            if (qs != null)
                TryCall(sb, "ReturnToVault", qs, "ReturnToVault", new object[] { false });

            bool questSceneValid = qs != null;
            if (!questSceneValid)
            {
                sb.AppendLine("QuestScene yok -> 'Main' sahnesi yukleniyor");
                try
                {
                    SceneManager.LoadScene("Main", LoadSceneMode.Single);
                    sb.AppendLine("LoadScene(Main) cagrildi");
                }
                catch (Exception e) { sb.AppendLine("LoadScene(Main) HATA: " + e.Message); }
            }

            Logger.LogInfo(sb.ToString());
            try { File.WriteAllText(Path.Combine(Paths.BepInExRootPath, "FSAutoQuest-recovery.txt"), sb.ToString(), Encoding.UTF8); } catch { }
        }

        private static void TryCall(StringBuilder sb, string label, object target, string method, object[] args)
        {
            try
            {
                var m = target.GetType().GetMethod(method, InstFlags);
                if (m == null) { sb.AppendLine(label + ": metot yok"); return; }
                var r = m.Invoke(target, args);
                sb.AppendLine(label + ": OK" + (r == null ? "" : " -> " + r));
            }
            catch (Exception e)
            {
                var inner = e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;
                sb.AppendLine(label + ": HATA " + inner.GetType().Name + " " + inner.Message);
            }
        }

        // ------------------------------------------------------------ helpers
        private static void AppendKnown(StringBuilder sb, object o, string[] names)
        {
            foreach (var n in names)
            {
                object v;
                bool ok = TryGet(o, n, out v);
                if (!ok) continue;
                sb.AppendLine("  " + n + " = " + Fmt(v));
            }
        }

        private static void TryLine(StringBuilder sb, string prefix, Func<object> get)
        {
            try { sb.AppendLine(prefix + Fmt(get())); }
            catch (Exception e) { sb.AppendLine(prefix + "<hata: " + e.GetType().Name + ">"); }
        }

        private static bool TryGet(object o, string name, out object value)
        {
            value = null;
            if (o == null) return false;
            var t = o.GetType();
            while (t != null)
            {
                try
                {
                    var p = t.GetProperty(name, InstFlags);
                    if (p != null && p.GetIndexParameters().Length == 0)
                    {
                        value = p.GetValue(o, null);
                        return true;
                    }
                }
                catch (Exception e) { value = "<hata:" + e.GetType().Name + ">"; return true; }

                try
                {
                    var f = t.GetField(name, InstFlags);
                    if (f != null)
                    {
                        value = f.GetValue(o);
                        return true;
                    }
                }
                catch (Exception e) { value = "<hata:" + e.GetType().Name + ">"; return true; }

                t = t.BaseType;
            }
            return false;
        }

        private static object M(object o, string name)
        {
            object v;
            return TryGet(o, name, out v) ? v : null;
        }

        private static string Fmt(object v)
        {
            if (v == null) return "null";
            if (v is string) return "\"" + Escape((string)v) + "\"";
            if (v is bool) return ((bool)v) ? "true" : "false";
            if (v is UnityEngine.Object)
            {
                var uo = v as UnityEngine.Object;
                if (uo == null) return "null(bos)";
                return uo.name + " [" + uo.GetType().Name + "]";
            }
            var list = v as IList;
            if (list != null)
            {
                var parts = new List<string>();
                for (int i = 0; i < list.Count && i < 12; i++) parts.Add(NameOf(list[i]));
                if (list.Count > 12) parts.Add("...");
                return "[" + list.Count + "] " + string.Join(", ", parts.ToArray());
            }
            if (v is IEnumerable && !(v is string))
            {
                int c = 0;
                foreach (var _ in (IEnumerable)v) { if (++c > 24) break; }
                return "<" + v.GetType().Name + " adet~" + c + ">";
            }
            string s = v.ToString();
            return s.Length > 120 ? s.Substring(0, 120) + "..." : s;
        }

        private static string NameOf(object o)
        {
            if (o == null) return "null";
            var uo = o as UnityEngine.Object;
            if (uo != null) return uo == null ? "null(bos)" : uo.name;
            return o.ToString();
        }

        private static int CountOf(object v)
        {
            var l = v as IList;
            if (l != null) return l.Count;
            return -1;
        }

        private static bool Truthy(object v)
        {
            if (v is bool) return (bool)v;
            return false;
        }

        private static string DelegatesOf(object uiButton)
        {
            var oc = M(uiButton, "onClick");
            var list = oc as IList;
            if (list == null) return "?";
            var parts = new List<string>();
            foreach (var d in list)
            {
                if (d == null) { parts.Add("null"); continue; }
                string tgt = NameOf(M(d, "target"));
                string meth = M(d, "methodName") as string;
                parts.Add(tgt + "." + meth);
            }
            return string.Join(", ", parts.ToArray());
        }

        private static string LabelOf(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                if (c.GetType().Name != "UILabel") continue;
                string t = M(c, "text") as string;
                if (!string.IsNullOrEmpty(t)) return t.Replace("\n", " ");
            }
            return "";
        }

        private static string PathOf(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            string s = string.Join("/", parts.ToArray());
            return s.Length > 160 ? "..." + s.Substring(s.Length - 157) : s;
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\r", " ").Replace("\n", " ");
        }

        // ------------------------------------------------------ type/objects
        private static Type FindType(string name)
        {
            Type cached;
            if (TypeCache.TryGetValue(name, out cached)) return cached;

            Type found = null;
            foreach (var asm in GameAssemblies())
            {
                foreach (var t in TypesOf(asm))
                {
                    if (t == null) continue;
                    if (t.Name == name || t.FullName == name) { found = t; break; }
                }
                if (found != null) break;
            }
            TypeCache[name] = found;
            return found;
        }

        private static IEnumerable<Assembly> GameAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                {
                    try { return !a.IsDynamic; } catch { return false; }
                })
                .OrderByDescending(a => { try { return a.GetName().Name.StartsWith("Assembly-CSharp"); } catch { return false; } })
                .ToArray();
        }

        private static Type[] TypesOf(Assembly a)
        {
            Type[] arr;
            if (AsmTypes.TryGetValue(a, out arr)) return arr;
            try { arr = a.GetTypes(); }
            catch (ReflectionTypeLoadException e) { arr = e.Types.Where(t => t != null).ToArray(); }
            catch { arr = new Type[0]; }
            AsmTypes[a] = arr;
            return arr;
        }

        private static List<object> FindAll(string typeName)
        {
            var result = new List<object>();
            var t = FindType(typeName);
            if (t == null || !typeof(Component).IsAssignableFrom(t) && !typeof(UnityEngine.Object).IsAssignableFrom(t)) return result;
            try
            {
                var objs = UnityEngine.Object.FindObjectsByType(t, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var o in objs) if (o != null) result.Add(o);
            }
            catch { }
            return result;
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
                    var p = bt.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
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
                        var f = bt.GetField(fn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
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
