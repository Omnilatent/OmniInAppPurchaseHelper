using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnilatent.InAppPurchase;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

public partial class InAppPurchaseHelper : MonoBehaviour
{
    protected CrossPlatformValidator m_Validator = null;
    bool m_UseAppleStoreKitTestCertificate;
    private readonly Dictionary<string, TaskCompletionSource<EntitlementStatus>> _pendingEntitlementChecks = new ();
    
    void InitializeValidator()
    {
        //5.0.0 change: Only validate on Google store. App store has its own validation now
        if (IsGooglePlayStoreSelected())
        {
            #if !UNITY_EDITOR && OMNILATENT_IAP_HELPER
            m_Validator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
            #endif
        }
        else
        {
            Debug.LogWarning($"The cross-platform validator is not implemented for the currently selected store: {StandardPurchasingModule.Instance().appStore}.");
        }
    }
    
    public bool IsPurchaseValid(Order order)
    {
        //If the validator doesn't support the current store, we assume the purchase is valid
        if (IsGooglePlayStoreSelected())
        {
            try
            {
                var result = m_Validator.Validate(order.Info.Receipt);

                //The validator returns parsed receipts.
                LogReceipts(result);
            }

            //If the purchase is deemed invalid, the validator throws an IAPSecurityException.
            catch (IAPSecurityException reason)
            {
                Debug.Log($"Invalid receipt: {reason}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if user own this product and purchase was restored. On iOS, use CheckEntitlement to check if user has bought this product.
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public static bool CheckReceipt(string productId)
    {
        var product = Instance.GetProduct(productId);
        if (product == null)
        {
            return false;
        }
        
        return CheckReceipt(product);
    }

    static bool CheckReceipt(Product purchasedProduct)
    {
        if (!InAppPurchaseHelper.Instance.IsInitialized()) return false;

        //If we the validator doesn't support the current store, we assume the purchase is valid
        if (IsCurrentStoreSupportedByValidator())
        {
            if (purchasedProduct.hasReceipt)
            {
                try
                {
                    var result = Instance.m_Validator.Validate(purchasedProduct.receipt);
                    //The validator returns parsed receipts.
                    LogReceipts(result);
                }
                //If the purchase is deemed invalid, the validator throws an IAPSecurityException.
                catch (IAPSecurityException reason)
                {
                    Debug.Log($"Invalid receipt for '{purchasedProduct.definition.id}': {reason}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            //iOS does not allow checking receipt synchronously, this will use cached result
            // Instance._storeController.CheckEntitlement(purchasedProduct);
            return RestorePurchaseHelper.GetProductOwnership(purchasedProduct.definition.id) > 0;
        }

        return purchasedProduct.hasReceipt;
    }

    /// <summary>
    /// Callback true if user owns this product. Use RestorePurchaseHelper to check if purchase was restored.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="onReceiptChecked"></param>
    public static void CheckEntitlement(string productId, CheckReceiptDelegate onReceiptChecked)
    {
        var product = Instance.GetProduct(productId);
        if (product == null)
        {
            onReceiptChecked?.Invoke(productId, false);
            return;
        }
        
        Instance._onNextReceiptCheck = onReceiptChecked;
        Instance._storeController.CheckEntitlement(product);
    }
    
    public static Task<EntitlementStatus> CheckEntitlementAsync(string productId)
    {
        var product = Instance.GetProduct(productId);
        if (product == null)
        {
            return Task.FromResult(EntitlementStatus.Unknown);
        }

        var tcs = new TaskCompletionSource<EntitlementStatus>();

        // store in dictionary
        Instance._pendingEntitlementChecks[productId] = tcs;
        Instance._storeController.CheckEntitlement(product);
        return tcs.Task;
    }

    static bool IsCurrentStoreSupportedByValidator()
    {
        //The CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
        return IsGooglePlayStoreSelected();
    }

    static bool IsGooglePlayStoreSelected()
    {
        var currentAppStore = StandardPurchasingModule.Instance().appStore;
        return currentAppStore == AppStore.GooglePlay;
    }

    static bool IsAppleAppStoreSelected()
    {
        var currentAppStore = StandardPurchasingModule.Instance().appStore;
        return currentAppStore == AppStore.AppleAppStore ||
               currentAppStore == AppStore.MacAppStore;
    }

    static void LogReceipts(IEnumerable<IPurchaseReceipt> receipts)
    {
        Debug.Log("Receipt is valid. Contents:");
        foreach (var receipt in receipts)
        {
            LogReceipt(receipt);
        }
    }

    static void LogReceipt(IPurchaseReceipt receipt)
    {
        Debug.Log($"Product ID: {receipt.productID}\n" +
                  $"Purchase Date: {receipt.purchaseDate}\n" +
                  $"Transaction ID: {receipt.transactionID}");

        if (receipt is GooglePlayReceipt googleReceipt)
        {
            Debug.Log($"Purchase State: {googleReceipt.purchaseState}\n" +
                      $"Purchase Token: {googleReceipt.purchaseToken}");
        }

        if (receipt is AppleInAppPurchaseReceipt appleReceipt)
        {
            Debug.Log($"Original Transaction ID: {appleReceipt.originalTransactionIdentifier}\n" +
                      $"Subscription Expiration Date: {appleReceipt.subscriptionExpirationDate}\n" +
                      $"Cancellation Date: {appleReceipt.cancellationDate}\n" +
                      $"Quantity: {appleReceipt.quantity}");
        }
    }
}
