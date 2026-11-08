using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace FSAutoQuest.Menu
{
    [HarmonyPatch]
    public static class QuestRequirementsBypassPatches
    {
        public static bool BypassEnabled
        {
            get
            {
                return AutoQuestPlugin.CfgBypassQuestReqs != null && AutoQuestPlugin.CfgBypassQuestReqs.Value;
            }
        }

        // 1. Tum gereksinimlerin karsilandigi kontrolunu baypas et
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "get_IsAllRequirementsMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsAllRequirementsMeet(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 2. Sakin seviyesi ve SPECIAL sartini baypas et (Sakin secim listesinde sakinlerin secilebilmesini saglar)
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "ValidateDwellerBasedRequirements")]
        [HarmonyPrefix]
        private static bool Prefix_ValidateDwellerBasedRequirements(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 3. Silah sartini baypas et
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "ValidateWeaponRequirements")]
        [HarmonyPrefix]
        private static bool Prefix_ValidateWeaponRequirements(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 4. Kiyafet sartini baypas et
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "ValidateOutfitRequirements")]
        [HarmonyPrefix]
        private static bool Prefix_ValidateOutfitRequirements(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 5. Silah sarti karsilandi mi
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "IsWeaponRequirementMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsWeaponRequirementMeet(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 6. Kiyafet sarti karsilandi mi
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "IsOutfitRequirementMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsOutfitRequirementMeet(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 7. Kiyafet adedi karsilandi mi
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "ValidateRequiredOutfitQuantity")]
        [HarmonyPrefix]
        private static bool Prefix_ValidateRequiredOutfitQuantity(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 8. Seviye sarti karsilandi mi
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "IsDwellerLevelRequirementMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsDwellerLevelRequirementMeet(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 9. SPECIAL nitelik sarti karsilandi mi
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "IsSpecialStatRequirementMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsSpecialStatRequirementMeet(ref bool __result)
        {
            if (BypassEnabled)
            {
                __result = true;
                return false;
            }
            return true;
        }

        // 10. Takim kisi sayisi sarti (En az 1 sakin secilmisse baslatmaya izin ver)
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "IsTeamCountRequirementMeet")]
        [HarmonyPrefix]
        private static bool Prefix_IsTeamCountRequirementMeet(ref bool __result, List<global::Dweller> dwellers)
        {
            if (BypassEnabled)
            {
                __result = dwellers != null && dwellers.Count > 0;
                return false;
            }
            return true;
        }

        // 11. Ekranda gereksinim tiklerini (yesil CheckMarks) aktif et ve m_isAllRequirementsMeet = true yap
        [HarmonyPatch(typeof(global::QuestEntryRequirements), "ValidateRequirements")]
        [HarmonyPostfix]
        private static void Postfix_ValidateRequirements(global::QuestEntryRequirements __instance)
        {
            if (BypassEnabled)
            {
                try
                {
                    Traverse.Create(__instance).Field("m_isAllRequirementsMeet").SetValue(true);
                    if (__instance.CheckMarks != null)
                    {
                        for (int i = 0; i < __instance.CheckMarks.Length; i++)
                        {
                            var cm = __instance.CheckMarks[i];
                            if (cm != null && cm.gameObject != null)
                            {
                                cm.gameObject.SetActive(true);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        // 12. Gorev ekipman penceresinde "Goreve Basla" butonunu aktif et
        [HarmonyPatch(typeof(global::QuestEquipmentWindow), "CanStartQuest")]
        [HarmonyPrefix]
        private static bool Prefix_CanStartQuest(global::QuestEquipmentWindow __instance, ref bool __result)
        {
            if (BypassEnabled)
            {
                try
                {
                    var list = Traverse.Create(__instance).Field<List<global::Dweller>>("m_selectedDwellers").Value;
                    if (list != null && list.Count > 0)
                    {
                        __result = true;
                        return false;
                    }
                }
                catch { }
            }
            return true;
        }
    }
}
