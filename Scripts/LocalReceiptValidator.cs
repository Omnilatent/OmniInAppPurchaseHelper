using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

public partial class InAppPurchaseHelper : MonoBehaviour
{
    CrossPlatformValidator m_Validator = null;
    bool m_UseAppleStoreKitTestCertificate;

    void InitializeValidator()
    {
        if (IsCurrentStoreSupportedByValidator())
        {
#if !UNITY_EDITOR
            var appleTangleData = m_UseAppleStoreKitTestCertificate ? AppleStoreKitTestTangle.Data() : AppleTangle.Data();
            m_Validator = new CrossPlatformValidator(GooglePlayTangle.Data(), appleTangleData, Application.identifier);
#endif
        }
        else
        {
            var warningMsg = $"The cross-platform validator is not implemented for the currently selected store: {StandardPurchasingModule.Instance().appStore}. \n" +
                                "Build the project for Android, iOS, macOS, or tvOS and use the Google Play Store or Apple App Store. See README for more information.";
            Debug.LogWarning(warningMsg);
        }
    }

    public static bool CheckReceipt(string productId)
    {
        return CheckReceipt(Instance.GetProduct(productId));
    }

    static bool CheckReceipt(Product purchasedProduct)
    {
        if (!InAppPurchaseHelper.Instance.IsInitialized()) return false;

        //If we the validator doesn't support the current store, we assume the purchase is valid
        if (IsCurrentStoreSupportedByValidator() && purchasedProduct.hasReceipt)
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

        return purchasedProduct.hasReceipt;
    }

    static bool IsCurrentStoreSupportedByValidator()
    {
        //The CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
        return IsGooglePlayStoreSelected() || IsAppleAppStoreSelected();
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
