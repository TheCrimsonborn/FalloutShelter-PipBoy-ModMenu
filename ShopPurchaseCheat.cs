using System;
using HarmonyLib;
using UnityEngine;

namespace FSAutoQuest.Menu
{
    /// <summary>
    /// Magaza (Shop) icin "ucretsiz pack" hilesi.
    /// Oyuncu magazada bir pack (Lunchbox, Pet Carrier, Mr. Handy, Starter Pack, Nuka) secip
    /// "Satin Al" butonuna bastiginda gercek odeme akisi (Steam/IAP) hic baslatilmaz;
    /// oyunun kendi odul dagitim metodu dogrudan cagrilir.
    /// </summary>
    [HarmonyPatch]
    public static class FreeShopPatches
    {
        public static bool Enabled
        {
            get { return AutoQuestPlugin.CfgFreeShop != null && AutoQuestPlugin.CfgFreeShop.Value; }
        }

        /// <summary>
        /// ShopWindow.BuySelectedItem: buton tiklaninca gercek satin alma baslatan ozel metot.
        /// Prefix false donerse orijinal metot (PurchaseManager.PurchaseItem) hic calismaz.
        /// </summary>
        [HarmonyPatch(typeof(global::ShopWindow), "BuySelectedItem")]
        [HarmonyPrefix]
        private static bool Prefix_BuySelectedItem(global::ShopWindow __instance)
        {
            if (!Enabled) return true;
            if (__instance == null) return true;

            try
            {
                var item = __instance.SelectedSellableItem;
                if (item == null) return true;

                if (!IsFreeEligible(item.Type)) return true;

                string productId = item.ProductID;
                if (string.IsNullOrEmpty(productId)) return true;

                int quantity = item.Quantity;

                // force = false: secili urunun kendi Quantity/Type bilgisi kullanilir,
                // PurchaseManager (Steam/IAP) sorgulanmaz.
                bool granted = __instance.ProcessSuccessfulPurchase(productId, false);
                if (granted)
                {
                    LogMessage("Ucretsiz satin alma: " + item.Type + " x" + quantity + " (" + productId + ")");
                    return false;
                }

                LogMessage("Ucretsiz satin alma basarisiz, normal akisa donuluyor: " + item.Type + " (" + productId + ")");
            }
            catch (Exception e)
            {
                LogMessage("Ucretsiz magaza hatasi: " + e.GetType().Name + " " + e.Message);
            }

            return true;
        }

        private static bool IsFreeEligible(global::EShopItem type)
        {
            switch (type)
            {
                case global::EShopItem.Lunchbox:
                case global::EShopItem.MrHandy:
                case global::EShopItem.PetCarrier:
                case global::EShopItem.StarterPack:
                case global::EShopItem.NukaColaQuantum1:
                case global::EShopItem.NukaColaQuantum2:
                    return true;
                default:
                    return false;
            }
        }

        private static void LogMessage(string msg)
        {
            try
            {
                if (MenuPlugin.Log != null) MenuPlugin.Log.LogInfo(msg);
                else Debug.Log("[FSAutoQuest] " + msg);
            }
            catch { }
        }
    }
}
