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
            InAppPurchaseHelper.instance.BuyProduct(productData.ProductId, OnPurchaseProduct);
        }

        void OnPurchaseProduct(bool success, PurchaseProcessingResult result, string productID)
        {
            onPurchaseSuccess?.Invoke(success);
        }
    }
}