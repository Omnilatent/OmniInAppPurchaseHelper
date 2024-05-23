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

        protected virtual void Start()
        {
            if (productPriceText != null)
                productPriceText.Setup(productData);
            CheckDisableIfOwned();
        }

        public virtual void OnClick()
        {
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
                || (disableIfAdRemoved && IAPProcessor.CheckNoAds()))
            {
                gameObject.SetActive(false);
            }
        }
    }
}