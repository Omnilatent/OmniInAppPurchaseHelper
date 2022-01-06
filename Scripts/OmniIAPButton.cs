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
        [SerializeField] IAPProductData productData;
        [SerializeField] ProductPriceText productPriceText;
        [SerializeField] PurchaseEvent onPurchase;
        [SerializeField] bool disableIfOwned;
        [SerializeField] bool disableIfAdRemoved;

        private void Start()
        {
            if (productPriceText != null)
                productPriceText.Setup(productData);
            CheckDisableIfOwned();
        }

        public void OnClick()
        {
            InAppPurchaseHelper.Instance.BuyProduct(productData.ProductId, OnPurchaseProduct);
        }

        void OnPurchaseProduct(PurchaseResultArgs purchaseResultArgs)
        {
            onPurchase?.Invoke(purchaseResultArgs);
            CheckDisableIfOwned();
        }

        void CheckDisableIfOwned()
        {
            if ((disableIfOwned && productData.productType == UnityEngine.Purchasing.ProductType.NonConsumable && InAppPurchaseHelper.CheckReceipt(productData.ProductId))
                || (disableIfAdRemoved && IAPProcessor.CheckNoAds()))
            {
                gameObject.SetActive(false);
            }
        }
    }
}