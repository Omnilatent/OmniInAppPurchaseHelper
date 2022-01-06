using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Omnilatent.InAppPurchase
{
    public class OmniIAPButton : MonoBehaviour
    {
        [SerializeField] IAPProductData productData;
        [SerializeField] ProductPriceText productPriceText;
        [SerializeField] UnityEvent<bool> onPurchaseSuccess;
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
            onPurchaseSuccess?.Invoke(purchaseResultArgs.isSuccess);
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