using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Handle Remove Ad
/// </summary>
public class IAPProcessor
{
    public const string dataFolder = "ProductData";
    public const string PREF_NO_ADS = "PURCHASE_ADS";
    
    static bool hasAddedNoAdsDelegate;

    public static void Init()
    {
        SetupNoAds();
    }

    public static void SetupNoAds()
    {
        if (!hasAddedNoAdsDelegate)
        {
            AdsManager.Instance.noAds -= IAPProcessor.CheckNoAds;
            AdsManager.Instance.noAds += IAPProcessor.CheckNoAds;
            hasAddedNoAdsDelegate = true;
        }
    }

    public static IAPProductData GetProductData(string id)
    {
        IAPProductData productData = Resources.Load<IAPProductData>($"{dataFolder}/{id}");
        if (productData == null) { Debug.LogError($"Product not found {id}"); }
        return productData;
    }

    public static bool OnPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;
        IAPProductData productData = GetProductData(id);
        bool isValidPurchase = true;
        if (productData == null)
        {
            //invalid product
            Debug.LogError($"Product data {id} does not exist in Resources/ProductData folder.");
            isValidPurchase = false;
        }
        else
        {
        }
        return isValidPurchase;
        //SS.View.Manager.Add(PopupController.POPUP_SCENE_NAME, new PopupData(PopupType.OK, msg));
    }

    /// <returns>Return true if user has purchased remove ads</returns>
    public static bool CheckNoAds()
    {
        if (PlayerPrefs.GetInt(PREF_NO_ADS, 0) == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void HideBannerOnCheckNoAd()
    {
        if (CheckNoAds())
        {
            AdsManager.Instance.HideBanner();
        }
    }
}
