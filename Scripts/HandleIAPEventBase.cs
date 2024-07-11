using System;
#if JACAT_ADSMANAGER
using JacatGames.JacatAdsManager.API;
#endif
using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public abstract class HandleIAPEventBase : MonoBehaviour
    {
        bool hasAddedNoAdsDelegate;

        protected virtual void Awake()
        {
            InAppPurchaseHelper.persistentOnPurchaseCompleteCallback += OnPurchaseComplete;
            InAppPurchaseHelper.onPurchaseStart += OnPurchaseStart;
            InAppPurchaseHelper.onToggleLoading += OnToggleLoading;
            InAppPurchaseHelper.onLogError += LogEvent;
            InAppPurchaseHelper.onLogEvent += LogEvent;
            InAppPurchaseHelper.onLogException += LogException;
        }

        public virtual void SetupNoAds()
        {
            if (!hasAddedNoAdsDelegate)
            {
                #if OMNILATENT_ADS_MANAGER
                AdsManager.Instance.noAds -= CheckNoAds;
                AdsManager.Instance.noAds += CheckNoAds;
                #endif
                hasAddedNoAdsDelegate = true;
            }
            #if JACAT_ADSMANAGER
            JacatAdsManager.Instance.SetRemoveAd(CheckNoAds());
            #endif
        }

        protected abstract void OnToggleLoading(bool isLoading);

        protected abstract void ShowErrorPopup(PurchaseResultArgs resultArgs);

        protected abstract void ToggleShowAdOnResume(bool value);

        protected virtual void OnPurchaseStart(string productId)
        {
            ToggleShowAdOnResume(false);
        }

        protected virtual void OnPurchaseComplete(PurchaseResultArgs resultArgs)
        {
            ToggleShowAdOnResume(true);
            if (!resultArgs.isSuccess)
            {
                ShowErrorPopup(resultArgs);
                // MessagePopup.ShowMessage($"{resultArgs.message} {resultArgs.reason}");
                return;
            }
            else
            {
                PayoutPurchase(resultArgs);
            }
        }

        protected virtual void PayoutPurchase(PurchaseResultArgs args)
        {
            var productData = InAppPurchaseHelper.GetProductData(args.productID);
            foreach (var payout in productData.payouts)
            {
                if (payout.PayoutType == PayoutTypeEnum.Currency)
                {
                    //User.AddGems(payout.quantity);
                }
                else if (payout.PayoutType == PayoutTypeEnum.Item)
                {
                    //User.AddHint(payout.quantity);
                }
                else if (payout.PayoutType == PayoutTypeEnum.Other)
                {
                }
                else if (payout.PayoutType == PayoutTypeEnum.NoAds)
                {
                    OnRemoveAdsPurchased(args, payout);
                }
            }
        }

        protected virtual void OnRemoveAdsPurchased(PurchaseResultArgs args, Payout payout)
        {
            PlayerPrefs.SetInt(InAppPurchaseHelper.PREF_NO_ADS, 1);
            PlayerPrefs.Save();
            SetupNoAds();
            HideBannerOnCheckNoAd();
        }

        protected virtual void LogException(Exception e)
        {
            #if OMNILATENT_FIREBASE_MANAGER
            FirebaseManager.LogException(e);
            #endif
        }

        protected virtual void LogEvent(string eventname, string eventparameter, string message)
        {
            #if OMNILATENT_FIREBASE_MANAGER
            FirebaseManager.LogEvent(eventname, eventparameter, message);
            #endif
        }

        /// <returns>Return true if user has purchased remove ads</returns>
        public virtual bool CheckNoAds()
        {
            if (PlayerPrefs.GetInt(InAppPurchaseHelper.PREF_NO_ADS, 0) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void HideBannerOnCheckNoAd()
        {
            if (CheckNoAds())
            {
                #if OMNILATENT_ADS_MANAGER
                AdsManager.Instance.HideBanner();
                #endif
                #if JACAT_ADSMANAGER
                JacatAdsManager.Instance.HideBanner();
                #endif
            }
        }
    }
}