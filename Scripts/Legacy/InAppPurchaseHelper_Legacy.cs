/*using UnityEngine;
using UnityEngine.Purchasing;

namespace Omnilatent.InAppPurchase
{
    public class InAppPurchaseHelper_Legacy
    {
        public async void Initialize()
        {
            if (IsInitialized()) return;
        
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null)
            {
                if (initializeUnityService)
                {
                    await InitializeUnityServiceAsync();
                }
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }
        
        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.

            IAPProductData[] products = Resources.LoadAll(dataFolder, typeof(IAPProductData)).Cast<IAPProductData>().ToArray();

            foreach (var item in products)
            {
                ProductType productType = item.productType;
                if (debugWillConsumeAllNonConsumable && productType == ProductType.NonConsumable) { productType = ProductType.Consumable; }
                builder.AddProduct(item.ProductId, productType, new StoreSpecificIds
                {
                    { item.ProductId, GooglePlay.Name },
                    { item.AppleAppStoreProductId, AppleAppStore.Name }
                });
                ValidateProductPayoutSubtype(item);
            }

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }
        
        /// <summary>
        /// Check if Store controller & Store extension provider has been initialized.
        /// </summary>
        public bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            bool ready = m_StoreController != null && m_StoreExtensionProvider != null;
            if (!ready && !hasReportedReadyError)
            {
                Debug.Log("IAP Helper not initialized");
                hasReportedReadyError = true;
            }
            return ready;
        }
    }
}*/