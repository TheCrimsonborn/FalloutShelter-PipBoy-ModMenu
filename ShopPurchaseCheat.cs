using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FSAutoQuest.Menu
{
    /// <summary>
    /// Magaza (Shop) icin "ucretsiz satin alma" hilesi.
    /// Oyuncu magazada bir urun secip "Satin Al" butonuna bastiginda gercek odeme akisi
    /// (Steam/IAP) hic baslatilmaz; icerik oyunun kendi dagitim yoluyla verilir.
    /// Kapsam: tum pack'ler (Lunchbox, Mr. Handy, Pet Carrier, Starter Pack, Nuka Quantum),
    /// karakter paketleri (Victor, Curie) ve Battle Pass yukseltmeleri.
    /// </summary>
    [HarmonyPatch]
    public static class FreeShopPatches
    {
        public static bool Enabled
        {
            get { return AutoQuestPlugin.CfgFreeShop != null && AutoQuestPlugin.CfgFreeShop.Value; }
        }

        /// <summary>
        /// ShopWindow.BuySelectedItem: buton tiklaninca gercek satin almayi baslatan ozel metot.
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

                bool granted;

                if (IsKnownToStore(item, productId))
                {
                    // Steam/IAP katalogu bu urunu taniyor: oyunun kendi odul hattini kullan.
                    // force = false gonderilir; oyun iceride zaten odeme sorgusunu atlar.
                    granted = __instance.ProcessSuccessfulPurchase(productId, false);
                }
                else
                {
                    // Katalog urunu tanimiyor (ozellikle Victor/Curie/Battle Pass):
                    // odulu dogrudan oyun API'leri ile ver.
                    granted = GrantDirectly(item);
                }

                if (granted)
                {
                    LogMessage("Ucretsiz satin alma: " + item.Type + " x" + item.Quantity + " (" + productId + ")");
                }
                else
                {
                    LogMessage("Ucretsiz satin alma tamamlanamadi: " + item.Type + " (" + productId + ")");
                }

                // Hile acikken gercek odeme akisina asla dusmeyiz (ucretlendirme olmasin).
                return false;
            }
            catch (Exception e)
            {
                LogMessage("Ucretsiz magaza hatasi: " + e.GetType().Name + " " + e.Message);
                return false;
            }
        }

        private static bool IsFreeEligible(global::EShopItem type)
        {
            // None (0) ve sayac (Count) disindaki tum urunler: pack'ler, karakter
            // paketleri (Victor/Curie) ve Battle Pass yukseltmeleri dahil.
            return type != global::EShopItem.None && type != global::EShopItem.Count;
        }

        /// <summary>Secili urun, oyunun odeme katalogunda (PurchaseManager) taniniyor mu?</summary>
        private static bool IsKnownToStore(global::SellableItem item, string productId)
        {
            try
            {
                var manager = GetSingleton<global::PurchaseManager>();
                if (manager == null) return false;

                var storeType = manager.GetTypeByProductID(productId);
                int storeQuantity = manager.GetQuantityByProductID(productId);

                return storeType == item.Type && storeQuantity > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Katalogun tanimadigi urunleri dogrudan oyun API'leri ile verir.</summary>
        private static bool GrantDirectly(global::SellableItem item)
        {
            switch (item.Type)
            {
                case global::EShopItem.Lunchbox:
                    return AddLunchBoxes(global::ELunchBoxType.Regular, item.Quantity);
                case global::EShopItem.MrHandy:
                    return AddLunchBoxes(global::ELunchBoxType.MrHandy, item.Quantity);
                case global::EShopItem.PetCarrier:
                    return AddLunchBoxes(global::ELunchBoxType.PetCarrier, item.Quantity);
                case global::EShopItem.StarterPack:
                    return AddLunchBoxes(global::ELunchBoxType.StarterPack, item.Quantity);
                case global::EShopItem.Victor:
                    return AddLunchBoxes(global::ELunchBoxType.Victor, item.Quantity);
                case global::EShopItem.Curie:
                    return AddLunchBoxes(global::ELunchBoxType.Curie, item.Quantity);
                case global::EShopItem.BattlepassUpgrade1:
                    return GrantBattlepass(false);
                case global::EShopItem.BattlepassUpgrade2:
                case global::EShopItem.BattlepassUpgrade1To2:
                    return GrantBattlepass(true);
                default:
                    // Nuka Quantum gibi kaynak tabanli urunler yalnizca oyunun kendi
                    // hattiyla verilebilir; katalog tanimiyorsa dokunmuyoruz.
                    return false;
            }
        }

        private static bool AddLunchBoxes(global::ELunchBoxType boxType, int quantity)
        {
            var vault = GetSingleton<global::Vault>();
            if (vault == null) return false;

            int count = Mathf.Max(1, quantity);
            for (int i = 0; i < count; i++)
            {
                vault.AddLunchBox(boxType);
            }

            RefreshCollectionButton();
            SaveGame();
            return true;
        }

        /// <summary>
        /// Battle Pass yukseltmesi: oyunun kendi ic metotlarini (yerel fonksiyonlar)
        /// yansima ile cagirir; bunlar odul dagitimini ve sezon bileti durumunu yonetir.
        /// </summary>
        private static bool GrantBattlepass(bool premiumPlus)
        {
            string needle = premiumPlus ? "g__ProcessBattlepassUpgrade2" : "g__ProcessBattlepassUpgrade1";

            var methods = typeof(global::ShopWindow).GetMethods(
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.Name.IndexOf(needle, StringComparison.Ordinal) < 0) continue;
                if (method.GetParameters().Length != 0) continue;

                method.Invoke(null, null);
                SaveGame();
                return true;
            }

            LogMessage("Battle Pass metodu bulunamadi: " + needle);
            return false;
        }

        private static void RefreshCollectionButton()
        {
            try
            {
                var gui = GetSingleton<global::VaultGUIManager>();
                if (gui != null && gui.m_VaultHUD != null)
                {
                    gui.m_VaultHUD.UpdateLunchBoxCollectionButton();
                }
            }
            catch { }
        }

        private static void SaveGame()
        {
            try
            {
                global::SerializeHelper.Save(false, true);
            }
            catch { }
        }

        /// <summary>MonoSingleton&lt;T&gt;.Instance degerini taban siniflari dolasarak bulur.</summary>
        private static T GetSingleton<T>() where T : UnityEngine.Object
        {
            var type = typeof(T);
            while (type != null && type != typeof(object))
            {
                var prop = type.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (prop != null)
                {
                    return prop.GetValue(null, null) as T;
                }
                type = type.BaseType;
            }
            return null;
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
