using System;
using System.Collections.Generic;
using System.Linq;
using Omnilatent.InAppPurchase;
using UnityEngine;
using UnityEngine.Purchasing;

public partial class InAppPurchaseHelper : MonoBehaviour
{
    #region Initialization

    StoreController _storeController;
    UnityEngine.Purchasing.CatalogProvider m_DefaultCatalogProvider = new UnityEngine.Purchasing.CatalogProvider();

    async void InitializeIAP()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }
        
        _storeController = UnityIAPServices.StoreController();

        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnStoreDisconnected += OnStoreDisconnected;
        // _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        _storeController.OnPurchasesFetchFailed += OnPurchaseFetchFailed;

        await _storeController.Connect();
        OnStoreConnected();

        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnPurchasesFetched += OnPurchasesFetched;

        FetchProducts();
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
    {
        Debug.Log("[IAP] Store disconnected:" + failureDescription.Message);
        LogError($"{failureDescription.Message}");
        onInitializeComplete?.Invoke(false);
    }

    private void OnPurchaseFetchFailed(PurchasesFetchFailureDescription failureDescription)
    {
        Debug.Log($"[IAP] Purchase fetch failed. Reason: {failureDescription.FailureReason}. Message: {failureDescription.Message}");
        LogError($"{failureDescription.FailureReason}:{failureDescription.Message}");
        onInitializeComplete?.Invoke(false);
    }

    protected virtual void OnStoreConnected()
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");
        InitializeValidator();

        //check if user has purchased any remove ads product
        bool hasRemovedAds = false;
        if (removeAdsProducts.Length == 0)
        {
            Debug.Log("removeAdsProducts doesn't have any products. If you have remove ads product, add it to the list");
        }
        foreach (var item in removeAdsProducts)
        {
            foreach (var payout in item.payouts)
            {
                if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.ProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
            }
            if (hasRemovedAds) break;
        }
        PlayerPrefs.SetInt(PREF_NO_ADS, hasRemovedAds ? 1 : 0);
        RestorePurchaseHelper.Initialize();
        // IAPProcessor.Init();
        if (hasRemovedAds && hideBannerOnCheckRemoveAd)
            IAPEventHandler.HideBannerOnCheckNoAd();
        
        #if !UNITY_IOS //ios require button to restore
        _storeController.RestoreTransactions(OnPurchaseRestored);
        #endif
        if (debugWillConsumeAllNonConsumable) ConsumeAllPendingPurchases();
        onInitializeComplete?.Invoke(true);
    }
    
    void FetchProducts()
    {
        IAPProductData[] products = Resources.LoadAll(dataFolder, typeof(IAPProductData)).Cast<IAPProductData>().ToArray();

        var initialProductsToFetch = new List<ProductDefinition>();
        var storeSpecificIdsByProductId = new Dictionary<string, StoreSpecificIds>();
        foreach (var item in products)
        {
            ProductType productType = item.productType;
            if (debugWillConsumeAllNonConsumable && productType == ProductType.NonConsumable) { productType = ProductType.Consumable; }

            initialProductsToFetch.Add(new ProductDefinition(id: item.ProductId, type: productType));
            var storeSpecificIds = new StoreSpecificIds
            {
                { item.ProductId, UnityEngine.Purchasing.GooglePlay.Name },
                { item.AppleAppStoreProductId, UnityEngine.Purchasing.AppleAppStore.Name },
            };
            storeSpecificIdsByProductId.Add(item.ProductId, storeSpecificIds);
            ValidateProductPayoutSubtype(item);
        }

        //Finally we add everything to the Catalog Provider
        m_DefaultCatalogProvider.AddProducts(initialProductsToFetch, storeSpecificIdsByProductId);
        ConfigureProductServiceCallbacks();
        m_DefaultCatalogProvider.FetchProducts(UnityIAPServices.DefaultProduct().FetchProductsWithNoRetries);
    }

    void ConfigureProductServiceCallbacks()
    {
        UnityIAPServices.DefaultProduct().OnProductsFetched += OnInitialProductsFetched;
        UnityIAPServices.DefaultProduct().OnProductsFetchFailed += OnInitialProductsFetchFailed;
    }
    
    /// <summary>
    /// Check if Store controller & Store extension provider has been initialized.
    /// </summary>
    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        bool ready = _storeController != null && m_StoreExtensionProvider != null;
        if (!ready && !hasReportedReadyError)
        {
            Debug.Log("IAP Helper not initialized");
            hasReportedReadyError = true;
        }
        return ready;
    }

    private void OnInitialProductsFetched(List<Product> products) { }

    private void OnInitialProductsFetchFailed(ProductFetchFailed fetchFailed)
    {
        string failedProducts = string.Empty;

        for (int i = 0; i < fetchFailed.FailedFetchProducts.Count; i++)
        {
            failedProducts += fetchFailed.FailedFetchProducts[i].id;
            if (i < fetchFailed.FailedFetchProducts.Count - 1)
            {
                failedProducts += ", ";
            }
        }
        
        Debug.LogError($"InitialProductsFetchFailed. Reason: {fetchFailed.FailureReason}. Failed to fetch: [{failedProducts}]");
    }

    #endregion

    private void OnPurchasePending(PendingOrder order)
    {
        onToggleLoading?.Invoke(false);
        processingPurchase = false;
        
        var firstProduct = GetFirstProductInOrder(order);
        bool isValidPurchase = CheckProductData(firstProduct.definition.id);
        
        PurchaseResultArgs purchaseResultArgs = new PurchaseResultArgs(firstProduct.definition.id, true);
        InvokeCallbackClearNextPurchaseCallback(purchaseResultArgs);

        Debug.Log($"Processing Purchase: {firstProduct.definition.id}");
        _storeController.ConfirmPurchase(order);
    }
    
    public static bool CheckProductData(string productId)
    {
        IAPProductData productData = GetProductData(productId);
        bool isValidPurchase = true;
        if (productData == null)
        {
            //invalid product
            Debug.LogError($"Product data {productId} does not exist in Resources/ProductData folder.");
            isValidPurchase = false;
        }
        return isValidPurchase;
    }

    void OnProductsFetched(List<Product> products)
    {
        // Handle fetched products  
        _storeController.FetchPurchases();
    }

    void OnPurchasesFetched(Orders orders)
    {
        // Process purchases, e.g. check for entitlements from completed orders  
    }
    
    static Product GetFirstProductInOrder(Order order)
    {
        return order.CartOrdered.Items().First()?.Product;
    }
}