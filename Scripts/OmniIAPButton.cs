using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Omnilatent.InAppPurchase
{
    [System.Serializable]
    public class PurchaseEvent : UnityEvent<PurchaseResultArgs>
    {
    }

    public class OmniIAPButton : MonoBehaviour
    {
        [SerializeField] protected IAPProductData productData;
        [SerializeField] protected ProductPriceText productPriceText;
        [SerializeField] protected PurchaseEvent onPurchase;
        [SerializeField] protected bool disableIfOwned;
        [SerializeField] protected bool disableIfAdRemoved;
        
        [Tooltip("Listen to all purchase event to invoke disable event if it's a non consumable product")] [SerializeField]
        protected bool _listenToGlobalPurchaseEvent;
        [SerializeField] UnityEvent _onDisable;
        [SerializeField] protected bool _deactivateSelfOnDisable = true;

        protected virtual void Start()
        {
            if (productPriceText != null)
                productPriceText.Setup(productData);
            CheckDisableIfOwned();
            if (_listenToGlobalPurchaseEvent)
            {
                InAppPurchaseHelper.persistentOnPurchaseCompleteCallback += OnGlobalPurchaseComplete;
            }
        }

        public virtual void OnClick()
        {
            #if JACAT_ADSMANAGER
            JacatGames.JacatAdsManager.API.JacatAdsManager.Instance.BlockResumeAdForNextResumes(1);
            #endif
            InAppPurchaseHelper.Instance.BuyProduct(productData.ProductId, OnPurchaseProduct);
        }

        protected virtual void OnPurchaseProduct(PurchaseResultArgs purchaseResultArgs)
        {
            onPurchase?.Invoke(purchaseResultArgs);
            CheckDisableIfOwned();
        }

        protected virtual void CheckDisableIfOwned()
        {
            if ((disableIfOwned && productData.productType == UnityEngine.Purchasing.ProductType.NonConsumable && InAppPurchaseHelper.CheckReceipt(productData.ProductId))
                || (disableIfAdRemoved && InAppPurchaseHelper.Instance.IAPEventHandler.CheckNoAds()))
            {
                Disable();
            }
        }
        
        public void Disable()
        {
            if (_deactivateSelfOnDisable)
            {
                gameObject.SetActive(false);
            }
            
            _onDisable.Invoke();
        }
        
        protected virtual void OnGlobalPurchaseComplete(PurchaseResultArgs purchaseresultargs)
        {
            if (disableIfAdRemoved)
            {
                CheckDisableIfOwned();
            }
            else if (purchaseresultargs.productID == productData.ProductId)
            {
                CheckDisableIfOwned();
            }
        }
        
        protected virtual void OnDestroy()
        {
            InAppPurchaseHelper.persistentOnPurchaseCompleteCallback -= OnGlobalPurchaseComplete;
        }
    }
}