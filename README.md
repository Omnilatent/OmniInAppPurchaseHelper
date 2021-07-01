Git repository: https://github.com/Omnilatent/OmniInAppPurchaseHelper

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

Example: [Updated] Extra script is now included in Extra package

- A message should be displayed in purchaseCompleteDelegate's code to announce purchase result.
