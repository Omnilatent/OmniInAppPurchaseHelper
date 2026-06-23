using System;
#if JACAT_ADSMANAGER
using JacatGames.JacatAdsManager.API;
#endif
#if JACAT_ADSMANAGER_V2
using JacatGames.JacatAds;
#endif
using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public abstract class HandleIAPEventBase : MonoBehaviour
    {
        bool hasAddedNoAdsDelegate;
        private bool _showAdOnResume = true;

        protected virtual void Awake()
        {
            InAppPurchaseHelper.persistentOnPurchaseCompleteCallback += OnPurchaseComplete;
            InAppPurchaseHelper.onPurchaseStart += OnPurchaseStart;
            InAppPurchaseHelper.onToggleLoading += OnLoadingToggled;
            InAppPurchaseHelper.onLogError += LogEvent;
            InAppPurchaseHelper.onLogEvent += LogEvent;
            InAppPurchaseHelper.onLogException += LogException;
            InAppPurchaseHelper.OnPurchaseRestored += OnPurchaseRestored;
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
            
                #if JACAT_ADSMANAGER_V2
                AdGuard.AddShowRule("removeAds", CanShowAd, AdGuard.AllExceptRewarded);
                AdGuard.AddShowRule("appResumeAdAfterIap", CanShowAppResumeAd, AdFormat.OpenAd);
                #endif
            }
            #if JACAT_ADSMANAGER || JACAT_ADSMANAGER_V2 
            JacatAdsManager.Instance.SetRemoveAd(CheckNoAds());
            #endif
        }

        protected bool CanShowAd() { return !CheckNoAds(); }

        protected abstract void OnToggleLoading(bool isLoading);

        protected abstract void ShowErrorPopup(PurchaseResultArgs resultArgs);
        
        public abstract void ShowMessagePopup(string message, bool isError = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">If true, allow show ad on resume</param>
        protected virtual void ToggleShowAdOnResume(bool value)
        {
            _showAdOnResume = value;
            #if JACAT_ADSMANAGER
            JacatGames.JacatAdsManager.API.JacatAdsManager.Instance.SetShowAdOnResume(value);
            #endif
            
            #if OMNILATENT_ADS_MANAGER
            Omnilatent.AdsMediation.ShowAdOnAppResume.overrideShowAdOnResume = value;
            #endif
        }

        protected abstract void OnPurchaseRestored(bool success, string errorMessage);

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
            bool noAds = PlayerPrefs.GetInt(InAppPurchaseHelper.PREF_NO_ADS, 0) == 1;
            #if JACAT_ADSMANAGER
            // CanShowAds() should be the opposite of noAds. If they don't match, sync Jacat ads manager.
            if (JacatAdsManager.Instance.CanShowAds() == noAds)
            {
                JacatAdsManager.Instance.SetRemoveAd(noAds);
            }
            #endif
            return noAds;
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

        protected virtual void OnLoadingToggled(bool loading)
        {
            ToggleShowAdOnResume(!loading);
            OnToggleLoading(loading);
        }
        
        protected virtual bool CanShowAppResumeAd()
        {
            return _showAdOnResume;
        }
    }
}