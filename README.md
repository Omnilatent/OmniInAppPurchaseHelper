SETUP:
- Follow Unity's IAP setup flow
- If you followed Google's guide and get a duplicate aar error, delete the billing 3.0.1 aar file
- Build an APK to upload to Google Play alpha build.
- Add a gameObject with component InAppPurchaseHelper in Main scene
- Create IAPProductData Scriptable Objects with correspond name to every products in folder "Resources/ProductData"

USAGE:
To add remove ads:
- Make Product on Google Play Console
- Add all remove ads product to removeAdsProducts list in InAppPurchaseHelper. It'll check for remove ads on initiation.
- Use InAppPurchaseHelper.BuyProduct(string productId, PurchaseCompleteDelegate purchaseCompleteDelegate) to initiate purchasing process.
- Add HandleIAPEvent class to your project:

Example:
///BEGIN EXAMPLE
public class HandleIAPEvent : MonoBehaviour
{
    private void Awake()
    {
        InAppPurchaseHelper.persistentOnPurchaseCompleteCallback += OnPurchaseComplete;
    }

    public void OnPurchaseComplete(PurchaseResultArgs resultArgs)
    {
        if (!resultArgs.isSuccess)
        {
            //show error messange //SS.View.Manager.Add(PopupController.POPUP_SCENE_NAME, new PopupData(PopupType.OK, resultArgs.message));
            return;
        }
        var productData = IAPProcessor.GetProductData(resultArgs.productID);
        foreach (var payout in productData.payouts)
        {
            if (payout.type == PayoutType.Currency)
            {
                //Add payment logic here. You can customize this class to suit your need
            }
            else if (payout.type == PayoutType.NoAds)
            {
                PlayerPrefs.SetInt(IAPProcessor.PREF_NO_ADS, 1);
                PlayerPrefs.Save();
                IAPProcessor.SetupNoAds();
            }
        }
    }
}
///END OF EXAMPLE

- A message should be displayed in purchaseCompleteDelegate's code to announce purchase result.
