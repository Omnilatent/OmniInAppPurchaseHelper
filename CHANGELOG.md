## 2.5.3
New Features
- Implemented Jacat Ads Manager.
- Added debug code for consuming purchases.
- Created a custom drawer for ProductDataPayout.

Changes
- Updated block to show ads on app resume.
- IAP Button: Block resume ad once when making a purchase.
- IAP Button: Always check and disable if disableIfAdRemoved is enabled.
- Made SetupNoAds method virtual for customization.

## 2.5.2
Changes:
- Include HandleIAPEvent class in library namespace.
- Make HandleIAPEventBase class to have consistent functions defined in the library. HandleIAPEvent class in extra files will implement HandleIAPEventBase.

## 2.5.1
News:
- Add initial setup window to make sure essential files are imported. Update location of Tangle file to match Unity IAP 4.11. Add Scripting define symbol OMNILATENT_IAP_HELPER.
- Add public property ProcessingPurchase.
- Iap product data add displayName field.

Changes:
- Make class Iap Button and method protected virtual.
- Set subtype field to public set to allow modification by script.

## 2.5.0
News:
- Added compatibility with Unity IAP 4.6.0. 
- Removed dependency on Firebase, moved Firebase's code to HandleIAPEvent in extra files.

Upgrade 2.4.x - 2.5.x notices:
- If you want to keep Firebase logging IAP event, please update the Handle IAP Event script to handle the callbacks (by updating the script yourself or re-importing the script from extra package). 

## 2.4.1
Changes:
- Update extra file: handle iap event will stop app open ad from showing when resuming app after purchasing. (Omni Ads Manager)

## v2.4.0
News:
- Independence from Ads Manager: codes that refer to Omnilatent Ads Manager will be wrapped in scripting define symbol '#if OMNILATENT_ADS_MANAGER'.
  Notes: OMNILATENT_ADS_MANAGER is automatically added to project by Omni Ads Manager 2.7.2.

## v2.3.0
News:
- Add compatibility with Unity In App Purchasing 4.4.1:
Changes to initialization process:
	- Add option to initialize Unity Service.
	- After Unity Service has been initialized, initialize purchasing module.
- Add option to initialize manually.

Changes:
- Deprecated variable normal case instance.

- Dependency change: require Unity Services Core 1.4.0.

## v2.2.1
Fixes:
- Remove using RuntimePlatform because it lacks iPad, check iOS platform using scripting define symbol.

## v2.2.0
New features:
- Restore purchase helper to check restored purchase.
- InvokeCallbackClearNextPurchaseCallback: when restore purchase, check product ownership before calling persistentOnPurchaseCompleteCallback to prevent repeated payout.
- Product price text auto display price after IAP initialize success. 

Changes:
- InAppPurchaseHelper remove self timeout after having store controller initiate purchase because after initating purchase, the process cannot be interrupted.
- Encapsulate payout type variable of IAP Product Data in a property for easier update in future.
- IsInitialized(): only log ready error once.
- Product Price Text: show loading if load product price fail. 
- IAPProductData: change property localized price string to default price string.
- When purchase self timed out, callback purchase failed.
- Suppress obsolete warning in update old field code.
- Update extra file handle IAP event to use PayoutType instead.

## v2.1.0
- Add support for Apple App Store:
 + IAP product data: add field Apple App store product ID.
- Upgrade product data type & subtype no longer automatic via OnValidate, will need to be called manually from menu item.
- Invoke callback combine into same function. add invoke callback in purchase failed of IStoreListener.
- Debug feature consume all non-consumable products: toggle debug consume all to on, then restart app once to consume all purchases, then restart app again.
- Add loading when processing purchase because iOS process purchase take a lot of time. time out processing 15 seconds.

## v2.0.0 (2022/01/12)
- Update to unity IAP 4.1.2:
- Change check receipt: don't use Cross Platform Validator anymore, use Product.hasReceipt instead.
- Restore transaction every time app is initialized using IGooglePlayStoreExtensions.
- Update payout: change PayoutType to enum instead of int struct.
- Subtype change from int to string.
- IAP button check disable if owned
- IAP button callback with purchase result args instead of bool.
- Always use standard purchasing module instance when create builder.
- Validate payout subtype when initializing in debug build.

## v1.1.1 (2021/05/20)
- Add ExtraPackageImporter to import Extra scripts and Message popup

## v1.1.0 (2021/03/30)
- Change PurchaseCompleteDelegate to use PurchaseResultArgs instead of passing 3 parameters
