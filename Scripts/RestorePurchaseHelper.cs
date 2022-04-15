using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Omnilatent.InAppPurchase;

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

    private static void AddProductOwnership(PurchaseResultArgs purchaseResultArgs)
    {
        if (!data.ownedProducts.ContainsKey(purchaseResultArgs.productID))
        {
            data.ownedProducts.Add(purchaseResultArgs.productID, 1);
        }
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
    #endregion

    public static bool HasRestoredProduct(PurchaseResultArgs resultArgs)
    {
        if (data.ownedProducts.ContainsKey(resultArgs.productID))
        {
            return true;
        }
        return false;
    }
}