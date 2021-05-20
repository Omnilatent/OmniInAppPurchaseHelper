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

        private void Start()
        {
            if (productPriceText != null)
                productPriceText.Setup(productData);
        }

        public void OnClick()
        {
            InAppPurchaseHelper.Instance.BuyProduct(productData.ProductId, OnPurchaseProduct);
        }

        void OnPurchaseProduct(PurchaseResultArgs purchaseResultArgs)
        {
            onPurchaseSuccess?.Invoke(purchaseResultArgs.isSuccess);
        }
    }
}