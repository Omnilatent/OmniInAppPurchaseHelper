SETUP:
- Follow Unity's IAP setup flow
- If you followed Google's guide and get a duplicate aar error, delete the billing 3.0.1 aar file
- Build an APK to upload to Google Play alpha build.
- Add a gameObject with component InAppPurchaseHelper in Main scene
- Create IAPProductData Scriptable Objects with correspond name to every products in folder "Resources/ProductData"

USAGE:
To add remove ads:
- Make Product on Google Play Console
- Add product ID in IAPProcessor.cs:
 + public const string remove_ads = "[PRODUCT_ID_HERE]";
- Use InAppPurchaseHelper.BuyProduct(string productId, PurchaseCompleteDelegate purchaseCompleteDelegate) to initiate purchasing process.
- A message should be displayed in purchaseCompleteDelegate's code to announce purchase result.
