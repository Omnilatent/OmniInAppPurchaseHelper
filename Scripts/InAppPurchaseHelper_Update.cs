using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnilatent.InAppPurchase;
using UnityEngine;
using UnityEngine.Purchasing;

public partial class InAppPurchaseHelper : MonoBehaviour
{
    #region Initialization

    protected StoreController _storeController;
    protected UnityEngine.Purchasing.CatalogProvider m_DefaultCatalogProvider = new UnityEngine.Purchasing.CatalogProvider();
    protected IPurchaseService m_PurchasingService;
    protected bool _initialized;

    public delegate void CheckReceiptDelegate(string productId, bool hasReceipt);
    protected CheckReceiptDelegate _onNextReceiptCheck;
    protected CheckReceiptDelegate _onReceiptChecked;
    private string _checkingReceiptProductId;

    public static CheckReceiptDelegate OnReceiptChecked
    {
        get => Instance._onNextReceiptCheck;
        set => Instance._onReceiptChecked = value;
    }

    private ILoggerService _logger;

    ILoggerService Logger
    {
        get
        {
            if (_logger == null) { _logger = new UnityLogger(); }

            return _logger;
        }
    }

    public async void Initialize()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized(false))
        {
            // ... we are done here.
            return;
        }

        IAPEventHandler.SetupNoAds();
        if (initializeUnityService)
        {
            await InitializeUnityServiceAsync();
        }
        
        _storeController = UnityIAPServices.StoreController();
        m_PurchasingService = UnityIAPServices.DefaultPurchase();

        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnStoreDisconnected += OnStoreDisconnected;
        // _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        _storeController.OnPurchasesFetchFailed += OnPurchaseFetchFailed;
        _storeController.OnPurchaseFailed += OnPurchaseFailed;
        _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        _storeController.OnCheckEntitlement += OnCheckEntitlement;

        await _storeController.Connect();
        OnStoreConnected();

        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnPurchasesFetched += OnPurchasesFetched; //this will be invoked after product fetch -> try to restore purchase

        DebugConsumePurchase.CheckConsumeAllIAPProducts(this);
        FetchProducts();
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
    {
        Logger.Log("[IAP] Store disconnected:" + failureDescription.Message);
        LogError($"{failureDescription.Message}");
        onInitializeComplete?.Invoke(false);
    }

    private void OnPurchaseFetchFailed(PurchasesFetchFailureDescription failureDescription)
    {
        Logger.Log($"[IAP] Purchase fetch failed. Reason: {failureDescription.FailureReason}. Message: {failureDescription.Message}");
        LogError($"{failureDescription.FailureReason}:{failureDescription.Message}");
        onInitializeComplete?.Invoke(false);
    }

    protected virtual void OnStoreConnected()
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Logger.Log("OnInitialized: PASS");
        InitializeValidator();
    }

    void FetchProducts()
    {
        IAPProductData[] products = GetAllProductsData();

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
            Logger.Log($"Will fetch {item.ProductId}");
        }

        //Finally we add everything to the Catalog Provider
        m_DefaultCatalogProvider.AddProducts(initialProductsToFetch, storeSpecificIdsByProductId);
        ConfigureProductServiceCallbacks();
        m_DefaultCatalogProvider.FetchProducts(UnityIAPServices.DefaultProduct().FetchProductsWithNoRetries);
        
        //check if user has purchased any remove ads product
        /*bool hasRemovedAds = false;
        if (removeAdsProducts.Length == 0)
        {
            Logger.Log("removeAdsProducts doesn't have any products. If you have remove ads product, add it to the list");
        }
        
        _initialized = true; //has to set this so we can check receipt for remove ad
        
        RestorePurchaseHelper.Initialize();
        foreach (var item in removeAdsProducts)
        {
            foreach (var payout in item.payouts)
            {
                Logger.Log($"Checking receipt of {item.ProductId}");
                /*if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.ProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
                
                Logger.Log($"Checking receipt of {item.AppleAppStoreProductId}");
                if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.AppleAppStoreProductId))
                {
                    hasRemovedAds = true;
                    break;
                }/
                if (payout.PayoutType == PayoutTypeEnum.NoAds && RestorePurchaseHelper.GetProductOwnership(item.ProductId) > 0)
                {
                    hasRemovedAds = true;
                }
            }
            
            _storeController.CheckEntitlement(GetProduct(item.ProductId)); //schedule to refresh entitlement
            // if (hasRemovedAds) break;
        }
        Logger.Log($"Finished checking receipts");

        PlayerPrefs.SetInt(PREF_NO_ADS, hasRemovedAds ? 1 : 0);
        // IAPProcessor.Init();
        if (hasRemovedAds && hideBannerOnCheckRemoveAd)
            IAPEventHandler.HideBannerOnCheckNoAd();

        #if !UNITY_IOS //ios require button to restore
        _storeController.RestoreTransactions(OnPurchaseRestored);
        #endif
        if (debugWillConsumeAllNonConsumable) ConsumeAllPendingPurchases();
        onInitializeComplete?.Invoke(true);*/
    }

    private static IAPProductData[] GetAllProductsData()
    {
        return Resources.LoadAll(dataFolder, typeof(IAPProductData)).Cast<IAPProductData>().ToArray();
    }

    void ConfigureProductServiceCallbacks()
    {
        UnityIAPServices.DefaultProduct().OnProductsFetched += OnInitialProductsFetched;
        UnityIAPServices.DefaultProduct().OnProductsFetchFailed += OnInitialProductsFetchFailed;
    }

    /// <summary>
    /// Check if Store controller & Store extension provider has been initialized.
    /// </summary>
    public bool IsInitialized(bool logIfNotReady = true)
    {
        // Only say we are initialized if both the Purchasing references are set.
        bool ready = _storeController != null && _initialized;
        if (!ready && !hasReportedReadyError && logIfNotReady)
        {
            Logger.Log("IAP Helper not initialized");
            hasReportedReadyError = true;
        }

        return ready;
    }

    private void OnInitialProductsFetched(List<Product> products)
    {
        // Logger.Log(GetIdListString(products));

        //check if user has purchased any remove ads product
        bool hasRemovedAds = false;
        if (removeAdsProducts.Length == 0)
        {
            Logger.Log("removeAdsProducts doesn't have any products. If you have remove ads product, add it to the list");
        }
        _initialized = true; //has to set this so we can check receipt for remove ad
        /*foreach (var item in removeAdsProducts)
        {
            foreach (var payout in item.payouts)
            {
                Logger.Log($"Checking receipt of {item.ProductId}");
                if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.ProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
                
                Logger.Log($"Checking receipt of {item.AppleAppStoreProductId}");
                if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.AppleAppStoreProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
            }

            if (hasRemovedAds) break;
        }
        Logger.Log($"Finished checking receipts");

        PlayerPrefs.SetInt(PREF_NO_ADS, hasRemovedAds ? 1 : 0);
        RestorePurchaseHelper.Initialize();
        // IAPProcessor.Init();
        if (hasRemovedAds && hideBannerOnCheckRemoveAd)
            IAPEventHandler.HideBannerOnCheckNoAd();
        */
        
        //check if user has purchased any remove ads product
        
        RestorePurchaseHelper.Initialize();
        foreach (var item in removeAdsProducts)
        {
            foreach (var payout in item.payouts)
            {
                Logger.Log($"Checking receipt of {item.ProductId}");
                /*if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.ProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
                
                Logger.Log($"Checking receipt of {item.AppleAppStoreProductId}");
                if (payout.PayoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.AppleAppStoreProductId))
                {
                    hasRemovedAds = true;
                    break;
                }*/
                if (payout.PayoutType == PayoutTypeEnum.NoAds && RestorePurchaseHelper.GetProductOwnership(item.ProductId) > 0)
                {
                    hasRemovedAds = true;
                }
            }
            
            _storeController.CheckEntitlement(GetProduct(item.ProductId)); //schedule to refresh entitlement
            // if (hasRemovedAds) break;
        }
        Logger.Log($"Finished checking receipts");

        PlayerPrefs.SetInt(PREF_NO_ADS, hasRemovedAds ? 1 : 0);
        // IAPProcessor.Init();
        if (hasRemovedAds && hideBannerOnCheckRemoveAd)
            IAPEventHandler.HideBannerOnCheckNoAd();
        
#if !UNITY_IOS //ios require button to restore
        // _storeController.RestoreTransactions(OnRestorePurchase);
#endif
        _storeController.FetchPurchases();
        if (debugWillConsumeAllNonConsumable) ConsumeAllPendingPurchases();
        onInitializeComplete?.Invoke(true);
    }

    private static string GetIdListString(List<Product> products)
    {
        string deb = String.Empty;
        for (int i = 0; i < products.Count; i++)
        {
            if (i > 0)
            {
                deb += ",";
            }

            deb += products[i].definition.id;
        }

        return deb;
    }

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

        Logger.LogError($"InitialProductsFetchFailed. Reason: {fetchFailed.FailureReason}. Failed to fetch: [{failedProducts}]");
    }

    #endregion

    IEnumerator WaitForInitialize(string productId, PurchaseCompleteDelegate purchaseCompleteDelegate)
    {
        if (!IsInitialized() && Application.internetReachability != NetworkReachability.NotReachable)
        {
            onToggleLoading?.Invoke(true);
            Initialize();

            //Wait timeout
            float timeout = 5f;
            var checkInterval = new WaitForSecondsRealtime(0.1f);
            while (timeout > 0f)
            {
                if (IsInitialized())
                {
                    timeout = 0f;
                    break;
                }

                timeout -= 0.1f;
                yield return checkInterval;
            }

            onToggleLoading?.Invoke(false);
        }

        // Buy the product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        if (IsInitialized())
        {
            Product product = GetProduct(productId);

            onNextPurchaseComplete = purchaseCompleteDelegate;
            if (product != null && product.availableToPurchase)
            {
                Logger.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                onToggleLoading?.Invoke(true);
                processingPurchase = true;
                _storeController.PurchaseProduct(product);
            }
            else
            {
                string msg = $"Purchase {productId} failed. Product not found or not available.";
                PurchaseResultArgs purchaseResultArgs =
                    new PurchaseResultArgs(productId, false, msg, PurchaseFailureReason.ProductUnavailable);
                OnPurchaseFailed(purchaseResultArgs);
            }
        }
        else
        {
            string msg = $"Purchase {productId} failed. IAP not initialized. Please check internet connection or try again later.";
            PurchaseResultArgs purchaseResultArgs =
                new PurchaseResultArgs(productId, false, msg, PurchaseFailureReason.PurchasingUnavailable);
            OnPurchaseFailed(purchaseResultArgs);
        }
    }

    /// <summary>
    /// Called when purchase is successful
    /// </summary>
    /// <param name="order"></param>
    private void OnPurchasePending(PendingOrder order)
    {
        onToggleLoading?.Invoke(false);
        processingPurchase = false;

        var firstProduct = GetFirstProductInOrder(order);
        bool isValidPurchase = CheckProductData(firstProduct.definition.id);

        PurchaseResultArgs purchaseResultArgs = new PurchaseResultArgs(firstProduct.definition.id, true);
        InvokeCallbackClearNextPurchaseCallback(purchaseResultArgs);

        Logger.Log($"Processing Purchase: {firstProduct.definition.id}");
        _storeController.ConfirmPurchase(order);
        switch (GetProductData(firstProduct.definition.id).productType)
        {
            case ProductType.NonConsumable:
            case ProductType.Subscription:
                RestorePurchaseHelper.SetProductOwnership(firstProduct.definition.id, 1);
                break;
        }
    }
    
    public void OnPurchaseFailed(FailedOrder failedOrder)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        var firstProduct = GetFirstProductInOrder(failedOrder);
        Logger.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", firstProduct.definition.storeSpecificId, failedOrder.FailureReason));
        if (failedOrder.FailureReason == PurchaseFailureReason.UserCancelled)
        {
            // FirebaseManager.LogEvent("IAP_Cancelled", "message", failureReason.ToString());
            onLogEvent?.Invoke("IAP_Cancelled", "message", failedOrder.FailureReason.ToString());
        }
        else
        {
            // FirebaseManager.LogCrashlytics(failureReason.ToString());
            // FirebaseManager.LogException(new Exception("IAP Purchase Failed"));
            LogError(failedOrder.FailureReason.ToString());
            onLogException?.Invoke(new Exception(failedOrder.FailureReason.ToString()));
        }
        if (processingPurchase)
        {
            onToggleLoading?.Invoke(false);
            processingPurchase = false;
        }
        InvokeCallbackClearNextPurchaseCallback(new PurchaseResultArgs(firstProduct.definition.id, false, "Purchase failed.", failedOrder.FailureReason));
    }
    
    private void OnPurchaseDeferred(DeferredOrder order)
    {
        var firstProduct = GetFirstProductInOrder(order);
        Logger.Log(string.Format("OnPurchaseDeferred. Product: '{0}'", firstProduct.definition.storeSpecificId));
        if (processingPurchase)
        {
            onToggleLoading?.Invoke(false);
            processingPurchase = false;
        }
        InvokeCallbackClearNextPurchaseCallback(new PurchaseResultArgs(firstProduct.definition.id, false, "Purchase's being deferred."));
    }
    
    private void OnCheckEntitlement(Entitlement entitlement)
    {
        Instance.Logger.Log("Checking entitlement.");
        InvokeCheckReceiptCallback(entitlement);
    }
    
    public static bool CheckProductData(string productId)
    {
        IAPProductData productData = GetProductData(productId);
        bool isValidPurchase = true;
        if (productData == null)
        {
            //invalid product
            Instance.Logger.LogError($"Product data {productId} does not exist in Resources/ProductData folder.");
            isValidPurchase = false;
        }

        return isValidPurchase;
    }

    void OnProductsFetched(List<Product> products)
    {
        // Handle fetched products  
        // _storeController.FetchPurchases();
    }

    void OnPurchasesFetched(Orders orders)
    {
        // This is called on initialization to restore purchase
        foreach (var confirmedOrder in orders.ConfirmedOrders)
        {
            string productId = GetFirstProductInOrder(confirmedOrder).definition.id;
            if (RestorePurchaseHelper.HasRestoredProduct(productId)) { continue; }

            Logger.Log($"Fetched confirmed order: {confirmedOrder.Info}. Restoring product {productId}");
            InvokeCallbackClearNextPurchaseCallback(new PurchaseResultArgs(productId, true));
        }
    }

    static Product GetFirstProductInOrder(Order order)
    {
        return order.CartOrdered.Items().First()?.Product;
    }
    
    public Product GetProduct(string productId)
    {
        Product product = null;
        if (_storeController != null)
        {
            product = _storeController.GetProductById(productId);
            if (product != null && product.availableToPurchase)
            {
                //Logger.Log(string.Format("Product: '{0}'", product.definition.id));
            }
            else
            {
                Logger.LogError($"BuyProductID:{productId} FAIL. Not purchasing product, not found or not available for purchase. Check if ProductData with corresponding ID is in Resources/ProductData");
            }
        }
        return product;
    }
    
    public static void ConfirmPendingPurchase(string productID)
    {
        var product = Instance.GetProduct(productID);
        var pendingOrder = GetPendingOrderFromProduct(product);
        if (pendingOrder != null)
        {
            Instance._storeController.ConfirmPurchase(pendingOrder);
        }
    }
    
    static PendingOrder GetPendingOrderFromProduct(Product product)
    {
        foreach (var order in Instance.m_PurchasingService.GetPurchases())
        {
            if (order is PendingOrder pendingOrder)
            {
                var cartItem = pendingOrder.CartOrdered.Items().FirstOrDefault();
                if (cartItem != null && cartItem.Product.definition.storeSpecificId == product.definition.storeSpecificId)
                {
                    return pendingOrder; // Return the original instance
                }
            }
        }
        Instance.Logger.LogWarning($"No pending order found for product {product.definition.id}.");
        return null;
        /*var cartItemNew = new CartItem(product);
        var cartNew = new Cart(cartItemNew);
        return new PendingOrder(cartNew, new OrderInfo(string.Empty, string.Empty, string.Empty));*/
    }
    
    /// <summary>
    /// Restore manually using restore purchase button
    /// </summary>
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Logger.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        bool isIOS = false;
        #if UNITY_IOS
        isIOS = true;
        #endif
        if (isIOS)
        {
            // ... begin restoring purchases
            Logger.Log("RestorePurchases started ...");
            onToggleLoading?.Invoke(true);
            _storeController.RestoreTransactions(OnRestorePurchaseManually);
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Logger.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            IAPEventHandler.ShowMessagePopup("All restorable purchases are already active on this device.");
        }
    }
        
    void OnRestorePurchaseManually(bool success, string errorMessage)
    {
        RestorePurchaseAllProductsAsync(success, errorMessage, true);
    }
    
    void OnRestorePurchase(bool success, string errorMessage)
    {
        RestorePurchaseAllProductsAsync(success, errorMessage, false);
    }

    async Task RestorePurchaseAllProductsAsync(bool success, string errorMessage, bool showResultPopup)
    {
        if (success)
        {
            var allProductsData = GetAllProductsData();
            for (int i = 0; i < allProductsData.Length; i++)
            {
                if (RestorePurchaseHelper.HasRestoredProduct(allProductsData[i].ProductId)) { continue; }

                var entitlementStatus = await CheckEntitlementAsync(allProductsData[i].ProductId);
                if (entitlementStatus == EntitlementStatus.FullyEntitled)
                {
                    TryRestorePurchase(new PurchaseResultArgs(allProductsData[i].ProductId, true), false);
                }
            }
        }

        onToggleLoading?.Invoke(false);
        OnPurchaseRestored?.Invoke(success, errorMessage);
        string popupMessage = errorMessage;
        if (success)
        {
            popupMessage = "Restore purchase successful. Please restart the game.";
        }
        _iapEventHandler.ShowMessagePopup(popupMessage, !success);
    } 
    
    void InvokeCheckReceiptCallback(Entitlement entitlement)
    {
        if (entitlement.Product == null)
        {
            Logger.Log("Checking entitlement failed unexpectedly. Product is null.");
            _onNextReceiptCheck?.Invoke(_checkingReceiptProductId, false);
            _onNextReceiptCheck = null;
            _onReceiptChecked?.Invoke(_checkingReceiptProductId, false);
            return;
        }
        
        string productId = entitlement.Product.definition.id;
        bool hasReceipt = false;
        switch (entitlement.Status)
        {
            case EntitlementStatus.NotEntitled:
            case EntitlementStatus.Unknown:
            case EntitlementStatus.EntitledButNotFinished:
            case EntitlementStatus.EntitledUntilConsumed:
                hasReceipt = false;
                break;
            case EntitlementStatus.FullyEntitled:
                hasReceipt = true;
                break;
        }

        // TryRestorePurchase(new PurchaseResultArgs(productId, true), false);
        _onNextReceiptCheck?.Invoke(productId, hasReceipt);
        _onNextReceiptCheck = null;
        _onReceiptChecked?.Invoke(_checkingReceiptProductId, hasReceipt);
        
        if (_pendingEntitlementChecks.TryGetValue(productId, out var tcs))
        {
            tcs.TrySetResult(entitlement.Status);
            _pendingEntitlementChecks.Remove(productId);
        }
        
        Logger.Log($"Receipt for '{entitlement.Product}' status: {entitlement.Status}");
    }
}