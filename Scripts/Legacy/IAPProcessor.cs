﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Handle Remove Ad
/// </summary>
namespace Omnilatent.InAppPurchase
{
    [Obsolete]
    public class IAPProcessor
    {
        [Obsolete]
        public const string dataFolder = "ProductData";
        [Obsolete]
        public const string PREF_NO_ADS = "PURCHASE_ADS";

        static bool hasAddedNoAdsDelegate;

        [Obsolete("Use HandleIAPEventBase instead")]
        public static void Init()
        {
            SetupNoAds();
        }

        [Obsolete("Use HandleIAPEventBase instead")]
        public static void SetupNoAds()
        {
#if OMNILATENT_ADS_MANAGER
            if (!hasAddedNoAdsDelegate)
            {
                AdsManager.Instance.noAds -= IAPProcessor.CheckNoAds;
                AdsManager.Instance.noAds += IAPProcessor.CheckNoAds;
                hasAddedNoAdsDelegate = true;
            }
#endif
        }

        [Obsolete]
        public static IAPProductData GetProductData(string id)
        {
            /*IAPProductData productData = Resources.Load<IAPProductData>($"{dataFolder}/{id}");
            if (productData == null) { Debug.LogError($"Product not found {id}"); }
            return productData;*/
            return InAppPurchaseHelper.GetProductData(id);
        }

        [Obsolete]
        public static bool OnPurchase(PurchaseEventArgs args)
        {
            /*string id = args.purchasedProduct.definition.id;
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
            return isValidPurchase;*/
            return InAppPurchaseHelper.CheckProductData(args);
        }

        [Obsolete]
        /// <returns>Return true if user has purchased remove ads</returns>
        public static bool CheckNoAds()
        {
            return InAppPurchaseHelper.Instance.IAPEventHandler.CheckNoAds();
            /*if (PlayerPrefs.GetInt(PREF_NO_ADS, 0) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }*/
        }

        [Obsolete]
        public static void HideBannerOnCheckNoAd()
        {
/*#if OMNILATENT_ADS_MANAGER
            if (CheckNoAds())
            {
                AdsManager.Instance.HideBanner();
            }
#endif*/
            InAppPurchaseHelper.Instance.IAPEventHandler.HideBannerOnCheckNoAd();
        }
    }
}