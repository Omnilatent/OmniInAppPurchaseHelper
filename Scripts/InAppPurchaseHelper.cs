using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using System.Linq;
using Omnilatent.InAppPurchase;
using System.Collections;

namespace Omnilatent.InAppPurchase
{
    public class PurchaseResultArgs
    {
        public string productID;
        public string message;
        public bool isSuccess;
        public PurchaseFailureReason? reason;

        public PurchaseResultArgs(string productID, bool isSuccess, string message = "", PurchaseFailureReason? reason = null)
        {
            this.productID = productID;
            this.message = message;
            this.isSuccess = isSuccess;
            this.reason = reason;
        }
    }
}

public partial class InAppPurchaseHelper : MonoBehaviour, IStoreListener
{
    [SerializeField] IAPProductData[] removeAdsProducts; //Products to check receipt on initialized 
    public IAPProductData[] RemoveAdsProducts { get => removeAdsProducts; }
    [SerializeField] bool hideBannerOnCheckRemoveAd = true;

    [Tooltip("List of payout subtype. Log error if there are any product payout with subtype not included.")]
    [SerializeField] List<string> payoutSubtypes;
    [SerializeField] bool allowUnexpectedSubtype;

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
    IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

    public delegate void PurchaseCompleteDelegate(PurchaseResultArgs purchaseResultArgs);
    PurchaseCompleteDelegate onNextPurchaseComplete; //This handle only get callback once, will be removed after callback
    public static PurchaseCompleteDelegate persistentOnPurchaseCompleteCallback; //always callback on purchase

    /// <summary>
    /// Callback on initialize complete. Pass true if initialize successfully.
    /// </summary>
    public event Action<bool> onInitializeComplete;

    /// <summary>
    /// Call a loading scene to block user's action when IAP controller is initializing
    /// </summary>
    public static Action<bool> onToggleLoading;

    public static Action<System.Exception> onLogException;

    Dictionary<string, SubscriptionManager> subscriptionManagers = new Dictionary<string, SubscriptionManager>();
    bool processingPurchase = false;
    bool debugWillConsumeAllNonConsumable = false; //If set to true before initializing, will consume all non-consumable product to allow re-purchasing.

    static InAppPurchaseHelper _instance;

    [Obsolete("Use Instance (capitalized I) instead", true)]
    public static InAppPurchaseHelper instance => Instance;
    public static InAppPurchaseHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Instantiate(Resources.Load<InAppPurchaseHelper>("OmnilatentRes/InAppPurchaseHelper"));
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }

        //add no ads listener earlier: because initiating IAP take some time so splash ads was shown before no ads listener could be added in IAP initiation process
        IAPProcessor.SetupNoAds();
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

        IAPProductData[] products = Resources.LoadAll(IAPProcessor.dataFolder, typeof(IAPProductData)).Cast<IAPProductData>().ToArray();

        foreach (var item in products)
        {
            ProductType productType = item.productType;
            if (debugWillConsumeAllNonConsumable && productType == ProductType.NonConsumable) { productType = ProductType.Consumable; }
            builder.AddProduct(item.ProductId, productType, new IDs
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

    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        bool value = m_StoreController != null && m_StoreExtensionProvider != null;
        if (!value) Debug.Log("IAP Helper not initialized");
        return value;
    }

    //Example code
    void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    /// <summary>
    /// Inititate purchase process
    /// </summary>
    /// <param name="productId">Product ID, a ProductData file with matching name in Resources/Product Data should exists</param>
    /// <param name="purchaseCompleteDelegate">Callback on purchase complete, callback should display a message dialog displaying purchase result</param>
    public void BuyProduct(string productId, PurchaseCompleteDelegate purchaseCompleteDelegate)
    {
        StartCoroutine(WaitForInitialize(productId, purchaseCompleteDelegate));
    }

    IEnumerator WaitForInitialize(string productId, PurchaseCompleteDelegate purchaseCompleteDelegate)
    {
        if (!IsInitialized() && Application.internetReachability != NetworkReachability.NotReachable)
        {
            onToggleLoading?.Invoke(true);
            InitializePurchasing();

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
            Product product = m_StoreController.products.WithID(productId);

            onNextPurchaseComplete = purchaseCompleteDelegate;
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                onToggleLoading?.Invoke(true);
                m_StoreController.InitiatePurchase(product);
                processingPurchase = true;

                //Wait timeout
                float timeout = 15f;
                var checkInterval = new WaitForSecondsRealtime(0.1f);
                while (timeout > 0f)
                {
                    if (!processingPurchase)
                    {
                        timeout = 0f;
                        break;
                    }
                    timeout -= 0.1f;
                    yield return checkInterval;
                }

                if (processingPurchase)
                {
                    onToggleLoading?.Invoke(false);
                    processingPurchase = false;
                    var e = new System.Exception("Processing purchase self timed out.");
                    Debug.LogException(e);
                    onLogException?.Invoke(e);
                }
            }
            else
            {
                string msg = $"BuyProduct {productId} failed. Product is either not found or not available for purchase";
                PurchaseResultArgs purchaseResultArgs = new PurchaseResultArgs(productId, false, msg, PurchaseFailureReason.ProductUnavailable);
                OnPurchaseFailed(purchaseResultArgs);
            }
        }
        else
        {
            string msg = $"BuyProduct {productId} failed. IAP was not initialized. Please check internet connection and restart the app.";
            PurchaseResultArgs purchaseResultArgs = new PurchaseResultArgs(productId, false, msg, PurchaseFailureReason.PurchasingUnavailable);
            OnPurchaseFailed(purchaseResultArgs);
        }
    }

    public Product GetProduct(string productId)
    {
        Product product = null;
        if (IsInitialized())
        {
            product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                //Debug.Log(string.Format("Product: '{0}'", product.definition.id));
            }
            else
            {
                Debug.LogError($"BuyProductID:{productId} FAIL. Not purchasing product, not found or not available for purchase. Check if ProductData with corresponding ID is in Resources/ProductData");
            }
        }
        return product;
    }

    public string GetPriceString(string productId)
    {
        Product product = GetProduct(productId);
        if (product != null)
        {
            System.Globalization.CultureInfo culture = GetCultureInfoFromISOCurrencyCode(product.metadata.isoCurrencyCode);
            if (culture != null)
            {
                return product.metadata.localizedPrice.ToString("C", culture);
            }
            else
            {
                // Fallback to just using localizedPrice decimal
                return product.metadata.localizedPriceString;
            }
        }
        return null;
    }

    public static System.Globalization.CultureInfo GetCultureInfoFromISOCurrencyCode(string code)
    {
        foreach (System.Globalization.CultureInfo ci in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures))
        {
            System.Globalization.RegionInfo ri = new System.Globalization.RegionInfo(ci.LCID);
            if (ri.ISOCurrencySymbol == code)
                return ci;
        }
        return null;
    }

    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions(OnRestore);
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
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
                if (payout.payoutType == PayoutTypeEnum.NoAds && InAppPurchaseHelper.CheckReceipt(item.ProductId))
                {
                    hasRemovedAds = true;
                    break;
                }
            }
            if (hasRemovedAds) break;
        }
        PlayerPrefs.SetInt(IAPProcessor.PREF_NO_ADS, hasRemovedAds ? 1 : 0);
        IAPProcessor.Init();
        if (hasRemovedAds && hideBannerOnCheckRemoveAd)
            IAPProcessor.HideBannerOnCheckNoAd();

        m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
#if UNITY_ANDROID
        m_GooglePlayStoreExtensions.RestoreTransactions(OnRestore);
#endif
        if (debugWillConsumeAllNonConsumable) ConsumeAllPendingPurchases();
        onInitializeComplete?.Invoke(true);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        LogError(error.ToString());
        onInitializeComplete?.Invoke(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        onToggleLoading?.Invoke(false);
        processingPurchase = false;
        bool isValidPurchase = IAPProcessor.OnPurchase(args);
        //if isValidPurchase was false, you should display an error message

        PurchaseResultArgs purchaseResultArgs = new PurchaseResultArgs(args.purchasedProduct.definition.id, true);
        persistentOnPurchaseCompleteCallback?.Invoke(purchaseResultArgs);
        if (onNextPurchaseComplete != null)
        {
            onNextPurchaseComplete.Invoke(purchaseResultArgs);
            onNextPurchaseComplete = null;
            //persistentOnPurchaseCompleteCallback?.Invoke(isValidPurchase, PurchaseProcessingResult.Complete, args.purchasedProduct.definition.id);
        }

        Debug.Log($"Processing Purchase: {args.purchasedProduct.definition.id}");

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.tvOS)
        {
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            var receipt = apple.GetTransactionReceiptForProduct(args.purchasedProduct);
            Debug.Log($"Product receipt for deferred purchase: {receipt}");
            // Send transaction receipt to server for validation
        }

        /*// A consumable product has been purchased by this user.
        if (CompareProductId(productIDDiamond1, args))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
        }
        // Or ... a non-consumable product has been purchased by this user.
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // TODO: The non-consumable item has been successfully purchased, grant this item to the player.
        }
        // Or ... a subscription product has been purchased by this user.
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // TODO: The subscription item has been successfully purchased, grant this to the player.
        }
        // Or ... an unknown product has been purchased by this user. Fill in additional products here....
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }*/

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }

    public static bool CompareProductId(string productId, PurchaseEventArgs args)
    {
        return String.Equals(args.purchasedProduct.definition.id, productId, StringComparison.Ordinal);
    }

    /// <summary>
    /// IStoreListener function.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        if (failureReason == PurchaseFailureReason.UserCancelled)
        {
            FirebaseManager.LogEvent("IAP_Cancelled", "message", failureReason.ToString());
        }
        else
        {
            FirebaseManager.LogCrashlytics(failureReason.ToString());
            FirebaseManager.LogException(new Exception("IAP Purchase Failed"));
        }
        if (processingPurchase)
        {
            onToggleLoading?.Invoke(false);
            processingPurchase = false;
        }
    }

    void OnPurchaseFailed(PurchaseResultArgs resultArgs)
    {
        string logMessage = string.Empty;
        if (resultArgs.reason != null)
        {
            logMessage = resultArgs.reason.ToString();
        }
        LogError(logMessage);
        onNextPurchaseComplete?.Invoke(resultArgs);
        persistentOnPurchaseCompleteCallback?.Invoke(resultArgs);
    }

    public static void ConfirmPendingPurchase(string productID)
    {
        var product = Instance.GetProduct(productID);
        m_StoreController.ConfirmPendingPurchase(product);
    }

    public static GoogleProductMetadata GetGoogleProductMetadata(string productID)
    {
        Product product = Instance.GetProduct(productID);
        GoogleProductMetadata googleProductMetaData = UnityEngine.Purchasing.GetGoogleProductMetadataExtension.GetGoogleProductMetadata(product.metadata);
        return googleProductMetaData;
    }

    /// <summary>
    /// Warning: WIP function, doesn't work in last test
    /// </summary>
    public static SubscriptionInfo GetSubscriptionInfo(string productID)
    {
        SubscriptionManager subscriptionManager;
        if (!Instance.subscriptionManagers.TryGetValue(productID, out subscriptionManager))
        {
            Product product = Instance.GetProduct(productID);
            string intro_json = null;
#if UNITY_IOS
            Debug.LogWarning("IOS need intro json for introduction price");
#endif
            subscriptionManager = new SubscriptionManager(product, intro_json);
            Instance.subscriptionManagers.Add(productID, subscriptionManager);
        }
        SubscriptionInfo info;
#if UNITY_EDITOR
        Debug.Log("Fake store not supported and will throw StoreSubscriptionInfoNotSupportedException error. Returning fake sub info.");
        info = new SubscriptionInfo(productID);
#else
        info = subscriptionManager.getSubscriptionInfo();
#endif
        return info;
    }

    void OnRestore(bool success)
    {
        var restoreMessage = "";
        if (success)
        {
            // This does not mean anything was restored,
            // merely that the restoration process succeeded.
            restoreMessage = "Restore Successful";
        }
        else
        {
            // Restoration failed.
            restoreMessage = "Restore Failed";
        }

        Debug.Log(restoreMessage);
    }

    void ValidateProductPayoutSubtype(IAPProductData productData)
    {
        //Validate payout subtype, only in debug build.
        if (!allowUnexpectedSubtype && Debug.isDebugBuild && payoutSubtypes.Count > 0)
        {
            foreach (var payout in productData.payouts)
            {
                if (!string.IsNullOrEmpty(payout.subtype) && !payoutSubtypes.Contains(payout.subtype))
                {
                    Debug.LogException(new System.Exception($"Subtype {payout.subtype} of product {productData.ProductId} not registered in HandleIAPEvent"));
                }
            }
        }
    }

    /// <summary>
    /// Call this before enabling IAPHelper object to consume all non-consumable products
    /// </summary>
    public void ToggleDebugConsumeAllNonConsumable()
    {
        debugWillConsumeAllNonConsumable = true;
    }

    void ConsumeAllPendingPurchases()
    {
        IAPProductData[] products = Resources.LoadAll(IAPProcessor.dataFolder, typeof(IAPProductData)).Cast<IAPProductData>().ToArray();
        foreach (var item in products)
        {
            ConfirmPendingPurchase(item.ProductId);
        }
    }

    static void LogError(string msg)
    {
        Debug.LogError(msg);
        FirebaseManager.LogEvent("IAP_Error", "message", msg);
    }
}