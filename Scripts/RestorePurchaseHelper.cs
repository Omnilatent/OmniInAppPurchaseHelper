using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Omnilatent.InAppPurchase;
using UnityEngine.Purchasing;

namespace Omnilatent.InAppPurchase
{
    /// <summary>
    /// Remember owned products so restore purchase does not restore multiple times
    /// </summary>
    public static class RestorePurchaseHelper
    {
        public class Data
        {
            public Dictionary<string, int> ownedProducts = new Dictionary<string, int>();
            public string version;
        }

        const string prefKeyData = "IAP_DATA";
        const string dataVersion = "1";
        static Data data;

        #region Init
        static RestorePurchaseHelper()
        {
            Load();
            InAppPurchaseHelper.onPayoutSuccess += AddProductOwnership;
        }

        public static void Initialize() { }

        private static void AddProductOwnership(PurchaseResultArgs purchaseResultArgs)
        {
            if (!data.ownedProducts.ContainsKey(purchaseResultArgs.productID))
            {
                data.ownedProducts.Add(purchaseResultArgs.productID, 1);
                Save();
            }
        }
        
        public static void AddProductOwnership(string productId, int value)
        {
            if (!data.ownedProducts.ContainsKey(productId))
            {
                data.ownedProducts.Add(productId, 0);
            }

            data.ownedProducts[productId] = data.ownedProducts[productId] + value;
            Save();
        }

        public static void SetProductOwnership(string productId, int value)
        {
            data.ownedProducts[productId] = value;
            Save();
        }

        public static int GetProductOwnership(string productId)
        {
            if (data.ownedProducts.TryGetValue(productId, out int value))
            {
                return value;
            }

            return 0;
        }

        static void Load()
        {
            var textData = PlayerPrefs.GetString(prefKeyData, string.Empty);
            if (!string.IsNullOrEmpty(textData))
            {
                data = JsonMapper.ToObject<Data>(textData);
            }
            else
            {
                data = new Data();
                Save();
            }
        }

        static void Save()
        {
            data.version = dataVersion;
            var textData = DataToString();

            PlayerPrefs.SetString(prefKeyData, textData);
            PlayerPrefs.Save();
        }

        public static string DataToString() { return JsonMapper.ToJson(data); }

        /// <summary>
        /// Forget every owned product recorded on this device. Intended for testing:
        /// combined with <see cref="DebugAutoRestore"/> it gives a tester a device with no purchase benefits.
        /// Does not refund or consume anything on the store, and does not reset game save data
        /// that a payout already wrote.
        /// </summary>
        public static void ClearOwnershipData()
        {
            data = new Data();
            Save();
            PlayerPrefs.SetInt(InAppPurchaseHelper.PREF_NO_ADS, 0);
            PlayerPrefs.Save();
            Debug.Log("Cleared local IAP ownership data.");
        }
        #endregion

        public static bool HasRestoredProduct(PurchaseResultArgs resultArgs)
        {
            return HasRestoredProduct(resultArgs.productID);
        }
        
        public static bool HasRestoredProduct(string productId)
        {
            if (GetProductOwnership(productId) <= 0)
            {
                return false;
            }
            
            return true;
        }

        public static bool IsProductConsumable(ProductType productType)
        {
            switch (productType)
            {
                case ProductType.NonConsumable:
                case ProductType.Subscription:
                    return false;
                default: return true;
            }
        }
    }
}