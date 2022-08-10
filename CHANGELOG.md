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
