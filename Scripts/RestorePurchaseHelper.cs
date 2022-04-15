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
            Debug.Log("Already restored this product, won't restore again.");
            return true;
        }
        return false;
    }
}