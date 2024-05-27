using System;
using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public abstract class HandleIAPEventBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            InAppPurchaseHelper.persistentOnPurchaseCompleteCallback += OnPurchaseComplete;
            InAppPurchaseHelper.onPurchaseStart += OnPurchaseStart;
            InAppPurchaseHelper.onToggleLoading += OnToggleLoading;
            InAppPurchaseHelper.onLogError += LogEvent;
            InAppPurchaseHelper.onLogEvent += LogEvent;
            InAppPurchaseHelper.onLogException += LogException;
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
            var productData = IAPProcessor.GetProductData(args.productID);
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
            PlayerPrefs.SetInt(IAPProcessor.PREF_NO_ADS, 1);
            PlayerPrefs.Save();
            IAPProcessor.SetupNoAds();
            IAPProcessor.HideBannerOnCheckNoAd();
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
    }
}